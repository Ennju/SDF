using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Liquid2D
{
    public class Liquid2DLSFMMStar : Liquid2DLSBase
    {
        private int resolutionX;
        private int resolutionY;
        private float totalLengthX;
        private float totalLengthY;
        private int dilateTime;
        private ComputeShader LevelSet2DFMMStarComputeShader;
        private float gridSpacingX;
        private float gridSpacingY;
        private RenderTexture anotherTex;
        private RenderTexture[] rts;
        private const int oriREAD = 0;
        private const int oriWRITE = 1;
        private int READ = oriREAD;
        private int WRITE = oriWRITE;

        private const int BuildInitBoundTex = 0;
        private const int MarkSurface = 1;
        private const int BuildFirstQueue = 2;
        private const int DebugKernel = 3;
        private const int CalcUpwindLevelSet = 4;
        private const int BuildNextQueue = 5;
        private const int BuildNextQueue2 = 6;

        private ComputeBuffer iterateListBuffer;
        private ComputeBuffer iterateListArgBuffer;
        private ComputeBuffer iterateListBufferTo;
        private ComputeBuffer iterateListArgBufferTo;

        public override void OnInitSystem(int resolutionX, int resolutionY, float totalLengthX, float totalLengthY, int dilateTime, LevelSet2DRes res)
        {
            this.resolutionX = resolutionX;
            this.resolutionY = resolutionY;
            this.totalLengthX = totalLengthX;
            this.totalLengthY = totalLengthY;
            this.dilateTime = dilateTime;
            this.LevelSet2DFMMStarComputeShader = res.LevelSet2DFMMStarComputeShader;
            this.gridSpacingX = totalLengthX / (float)resolutionX;
            this.gridSpacingY = totalLengthY / (float)resolutionY;

            anotherTex = new RenderTexture(resolutionX + 2, resolutionY + 2, 0, RenderTextureFormat.RFloat);
            anotherTex.enableRandomWrite = true;
            anotherTex.filterMode = FilterMode.Point;
            anotherTex.Create();

            rts = new RenderTexture[2];

            int iterateListBufferNum = 128 * 128;
            iterateListBuffer = new ComputeBuffer(iterateListBufferNum, 2 * sizeof(uint), ComputeBufferType.Append);
            iterateListBufferTo = new ComputeBuffer(iterateListBufferNum, 2 * sizeof(uint), ComputeBufferType.Append);

            iterateListArgBuffer = new ComputeBuffer(3, sizeof(uint), ComputeBufferType.IndirectArguments);
            iterateListArgBufferTo = new ComputeBuffer(3, sizeof(uint), ComputeBufferType.IndirectArguments);
            int[] initarg = new int[] { 0, 1, 1 };
            iterateListArgBuffer.SetData(initarg);
            iterateListArgBufferTo.SetData(initarg);
        }

        public override void OnDestroySystem()
        {
            if (anotherTex != null)
                anotherTex.Release();
            if (iterateListBuffer != null)
                iterateListBuffer.Release();
            if (iterateListArgBuffer != null)
                iterateListArgBuffer.Release();
            if (iterateListArgBufferTo != null)
                iterateListArgBufferTo.Release();
            if (iterateListBufferTo != null)
                iterateListBufferTo.Release();
        }

        public override ComputeBuffer GetDebugBuffer()
        {
            return iterateListBuffer;
        }

        public override RenderTexture GetDebugTex()
        {
            return rts[WRITE];
        }

        public override void OnUpdateSystem(bool rebuildSDF, RenderTexture sourceRT, RenderTexture boundRT)
        {
            if (!rebuildSDF)
                return;
            
            rts[READ] = sourceRT;
            rts[WRITE] = anotherTex;
            
            // clear list buffer
            iterateListBuffer.SetCounterValue(0);
            
            // kernel0: init bound texture and other buffers
            LevelSet2DFMMStarComputeShader.SetTexture(BuildInitBoundTex, "_BoundTex", boundRT);
            LevelSet2DFMMStarComputeShader.SetTexture(BuildInitBoundTex, "_SDFRead", rts[READ]);
            LevelSet2DFMMStarComputeShader.SetInt("_ResolutionX", resolutionX);
            LevelSet2DFMMStarComputeShader.SetInt("_ResolutionY", resolutionY);
            LevelSet2DFMMStarComputeShader.Dispatch(BuildInitBoundTex, resolutionX, resolutionY, 1);
            
            // kernel1: mark known point
            LevelSet2DFMMStarComputeShader.SetTexture(MarkSurface, "_BoundTex", boundRT);
            LevelSet2DFMMStarComputeShader.SetTexture(MarkSurface, "_SDFRead", rts[READ]);
            LevelSet2DFMMStarComputeShader.SetInt("_ResolutionX", resolutionX);
            LevelSet2DFMMStarComputeShader.SetInt("_ResolutionY", resolutionY);
            LevelSet2DFMMStarComputeShader.Dispatch(MarkSurface, resolutionX, resolutionY, 1);
            
            // kernel2: build first queue, copy read to write tex
            LevelSet2DFMMStarComputeShader.SetBuffer(BuildFirstQueue, "_ItrateListBuffer", iterateListBuffer);
            LevelSet2DFMMStarComputeShader.SetTexture(BuildFirstQueue, "_BoundTex", boundRT);
            LevelSet2DFMMStarComputeShader.SetTexture(BuildFirstQueue, "_SDFRead", rts[READ]);
            LevelSet2DFMMStarComputeShader.SetTexture(BuildFirstQueue, "_SDFWrite", rts[WRITE]);
            LevelSet2DFMMStarComputeShader.SetInt("_ResolutionX", resolutionX);
            LevelSet2DFMMStarComputeShader.SetInt("_ResolutionY", resolutionY);
            LevelSet2DFMMStarComputeShader.Dispatch(BuildFirstQueue, resolutionX, resolutionY, 1);
            
            // kernel3: debug queue
            // LevelSet2DFMMStarComputeShader.SetBuffer(DebugKernel, "_ItrateListBufferRead", iterateListBuffer);
            // LevelSet2DFMMStarComputeShader.SetTexture(DebugKernel, "_SDFWrite", rts[WRITE]);
            // LevelSet2DFMMStarComputeShader.DispatchIndirect(DebugKernel, iterateListArgBuffer);
            
            // loop: 
            // 1、 dispatchIndirect our list, calc SDF val due to BOUND_TEX_IS_KNOWN pixel
            // 2、 dispatchIndirect our list, set BOUND_TEX_IS_KNOWN, build next list, generate next queue, swap listbuffer
            for(int dilate = 0;dilate<dilateTime;++dilate)
            {
                ComputeBuffer.CopyCount(iterateListBuffer, iterateListArgBuffer, 0);
                iterateListBufferTo.SetCounterValue(0);
            
                // kernel4: CalcUpwind
                LevelSet2DFMMStarComputeShader.SetTexture(CalcUpwindLevelSet, "_BoundTex", boundRT);
                LevelSet2DFMMStarComputeShader.SetTexture(CalcUpwindLevelSet, "_SDFWrite", rts[WRITE]);
                LevelSet2DFMMStarComputeShader.SetInt("_ResolutionX", resolutionX);
                LevelSet2DFMMStarComputeShader.SetInt("_ResolutionY", resolutionY);
                LevelSet2DFMMStarComputeShader.SetBuffer(CalcUpwindLevelSet, "_ItrateListBufferRead", iterateListBuffer);
                LevelSet2DFMMStarComputeShader.DispatchIndirect(CalcUpwindLevelSet, iterateListArgBuffer);
                LevelSet2DFMMStarComputeShader.DispatchIndirect(CalcUpwindLevelSet, iterateListArgBuffer);
            
                // kernel5: Build Next Queue
                LevelSet2DFMMStarComputeShader.SetTexture(BuildNextQueue, "_BoundTex", boundRT);
                LevelSet2DFMMStarComputeShader.SetTexture(BuildNextQueue, "_SDFWrite", rts[WRITE]);
                LevelSet2DFMMStarComputeShader.SetTexture(BuildNextQueue, "_SDFRead", rts[READ]);
                LevelSet2DFMMStarComputeShader.SetInt("_ResolutionX", resolutionX);
                LevelSet2DFMMStarComputeShader.SetInt("_ResolutionY", resolutionY);
                LevelSet2DFMMStarComputeShader.SetBuffer(BuildNextQueue, "_ItrateListBufferRead", iterateListBuffer);
                LevelSet2DFMMStarComputeShader.SetBuffer(BuildNextQueue, "_ItrateListBufferNew", iterateListBufferTo);
                LevelSet2DFMMStarComputeShader.DispatchIndirect(BuildNextQueue, iterateListArgBuffer);
            
                ComputeBuffer.CopyCount(iterateListBufferTo, iterateListArgBufferTo, 0);
            
                Liquid2DUtils.Swap(ref iterateListBuffer, ref iterateListBufferTo);
                Liquid2DUtils.Swap(ref iterateListArgBuffer, ref iterateListArgBufferTo);
            }
        }
    }
}
 