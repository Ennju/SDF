using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Liquid2D
{
    public class Liquid2DLSFSM : Liquid2DLSBase
    {
        private int resolutionX;
        private int resolutionY;
        private float totalLengthX;
        private float totalLengthY;
        private int dilateTime;
        private ComputeShader LevelSet2DFSMComputeShader;
        private float gridSpacingX;
        private float gridSpacingY;
        private RenderTexture anotherTex;
        private RenderTexture[] rts;
        private const int oriREAD = 0;
        private const int oriWRITE = 1;
        private int READ = oriREAD;
        private int WRITE = oriWRITE;

        private const int InitSDFKernel = 0;
        private const int SweepingKernel = 1;

        private ComputeBuffer iterateListBuffer;
        private ComputeBuffer iterateListArgBuffer;
        private ComputeBuffer iterateListBufferTo;
        private ComputeBuffer iterateListArgBufferTo;
        private ComputeBuffer sweepingCompleteTag;

        public override void OnInitSystem(int resolutionX, int resolutionY, float totalLengthX, float totalLengthY, int dilateTime, LevelSet2DRes res)
        {
            this.resolutionX = resolutionX;
            this.resolutionY = resolutionY;
            this.totalLengthX = totalLengthX;
            this.totalLengthY = totalLengthY;
            //this.dilateTime = dilateTime;
            this.dilateTime = CalcFSMDilateTime(resolutionX, resolutionY);
            this.LevelSet2DFSMComputeShader = res.LevelSet2DFSMComputeShader;
            this.gridSpacingX = totalLengthX / (float)resolutionX;
            this.gridSpacingY = totalLengthY / (float)resolutionY;

            anotherTex = new RenderTexture(resolutionX + 2, resolutionY + 2, 0, RenderTextureFormat.RFloat);
            anotherTex.enableRandomWrite = true;
            anotherTex.filterMode = FilterMode.Point;
            anotherTex.Create();

            rts = new RenderTexture[2];

            int iterateListBufferNum = 128 * 128;
            iterateListBuffer = new ComputeBuffer(iterateListBufferNum, 3 * sizeof(uint), ComputeBufferType.Append);
            iterateListBufferTo = new ComputeBuffer(iterateListBufferNum, 3 * sizeof(uint), ComputeBufferType.Append);

            iterateListArgBuffer = new ComputeBuffer(3, sizeof(uint), ComputeBufferType.IndirectArguments);
            iterateListArgBufferTo = new ComputeBuffer(3, sizeof(uint), ComputeBufferType.IndirectArguments);
            int[] initarg = new int[] { 0, 1, 1 };
            iterateListArgBuffer.SetData(initarg);
            iterateListArgBufferTo.SetData(initarg);
        }

        private int CalcFSMDilateTime(int resolutionX, int resolutionY)
        {
            if (resolutionX % 2 == 1)
                resolutionX += 1;
            if (resolutionY % 2 == 1)
                resolutionY += 1;
            int resolution = Mathf.Max(resolutionX, resolutionY);
            return 2 * resolution - 1;
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
            return rts[READ];
        }

        public override void OnUpdateSystem(bool rebuildSDF, RenderTexture sourceRT, RenderTexture boundRT)
        {
            if (!rebuildSDF)
                return;

            rts[READ] = sourceRT;
            rts[WRITE] = anotherTex;

            // clear list buffer
            // resolutionX and resolutionY should be odd, otherwise it will cause symchronized problem
            iterateListBuffer.SetCounterValue(0);

            // kernel0: just need bound tex BOUND_TEX_IS_BOUNDARY, init grid val and surface grid val, build first queue, init complete tag
            LevelSet2DFSMComputeShader.SetTexture(InitSDFKernel, "_BoundTex", boundRT);
            LevelSet2DFSMComputeShader.SetTexture(InitSDFKernel, "_SDFRead", rts[READ]);
            LevelSet2DFSMComputeShader.SetInt("_ResolutionX", resolutionX);
            LevelSet2DFSMComputeShader.SetInt("_ResolutionY", resolutionY);
            LevelSet2DFSMComputeShader.SetBuffer(InitSDFKernel, "_ItrateListBuffer", iterateListBuffer);
            LevelSet2DFSMComputeShader.Dispatch(InitSDFKernel, resolutionX, resolutionY, 1);

            for (int dilate = 0; dilate < dilateTime; ++dilate)
            {
                ComputeBuffer.CopyCount(iterateListBuffer, iterateListArgBuffer, 0);
                iterateListBufferTo.SetCounterValue(0);
            
                // kernel2: SweepingKernel, sweeping, build next queue
                LevelSet2DFSMComputeShader.SetInt("_ResolutionX", resolutionX);
                LevelSet2DFSMComputeShader.SetInt("_ResolutionY", resolutionY);
                LevelSet2DFSMComputeShader.SetTexture(SweepingKernel, "_BoundTex", boundRT);
                LevelSet2DFSMComputeShader.SetTexture(SweepingKernel, "_SDFRead", rts[READ]);
                LevelSet2DFSMComputeShader.SetBuffer(SweepingKernel, "_ItrateListBufferNew", iterateListBufferTo);
                LevelSet2DFSMComputeShader.SetBuffer(SweepingKernel, "_ItrateListBufferRead", iterateListBuffer);
                LevelSet2DFSMComputeShader.DispatchIndirect(SweepingKernel, iterateListArgBuffer);

                Liquid2DUtils.Swap(ref iterateListBuffer, ref iterateListBufferTo);
            }
        }
    }
}
