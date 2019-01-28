using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Liquid2D
{
    public class Liquid2DLevelSet : MonoBehaviour
    {

        #region Level Set Mode
        public enum LevelSetMode
        {
            //FMM,                // use compute shader, every time dispatch all grid
            FMMStar,            // use compute shader, every time dispatch outline grid
            //FMMStarJobSystem,   // use job system, every time dispatch outline grid
            FSM,
            FIM,
        }
        public enum DebugMode
        {
            BoundTex,
            SourceTex,
            //ListBuffer,
        }
        #endregion

        #region Custom Global Param
        public int resolutionX = 64;
        public int resolutionY = 32;
        public LevelSetMode solverMode = LevelSetMode.FMMStar;
        public float totalLengthX = 10f;
        public float totalLengthY = 10f;
        public int dilateTime = 5;
        public LevelSet2DRes resourcesLiquid2D;
        public Texture2D sourceTexture;
        public DebugMode debugMode = DebugMode.BoundTex;
        public float debugScale = 0.5f; 
        #endregion

        #region Runtime State
        private bool m_SystemShouldInit = true;
        private bool m_SystemIsReady = false;
        private LevelSetMode m_HistorySolverMode = LevelSetMode.FMMStar;
        #endregion

        #region Runtime Param
        private float m_GridSpacingX;
        private float m_GridSpacingY;
        private Dictionary<LevelSetMode, Liquid2DLSBase> solvers = new Dictionary<LevelSetMode, Liquid2DLSBase>
        {
            //{LevelSetMode.FMM, new Liquid2DLSFMM() },
            {LevelSetMode.FMMStar, new Liquid2DLSFMMStar() },
            //{LevelSetMode.FMMStarJobSystem, new Liquid2DLSFMMStarJobSystem() },
            {LevelSetMode.FIM, new Liquid2DLSFIM() },
            {LevelSetMode.FSM, new Liquid2DLSFSM() },
        };
        private Liquid2DLSBase m_HistorySolver;
        private Liquid2DLSBase m_CurrentSolver;

        private RenderTexture sourceRT;
        private RenderTexture boundRT;
        private ComputeShader buildBoundShader;
        private Shader debugShader;
        private Material debugMat;
        #endregion

        private void Start()
        {
            m_SystemShouldInit = true;
            m_SystemIsReady = false;
        }

        private void Update()
        {
            CheckSystemStateBegin();

            if (!m_SystemIsReady)
            {
                Debug.LogError("System Init Failed, Please Check Res");
                return;
            }

            UpdateSolver();

            CheckSystemStateEnd();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if(debugMat == null)
            {
                Graphics.Blit(source, destination);
                return;
            }
            debugMat.SetTexture("_BoundRT", boundRT);
            debugMat.SetTexture("_SourceRT", sourceRT);
            debugMat.SetInt("_ResolutionX", resolutionX + 2);
            debugMat.SetInt("_ResolutionY", resolutionY + 2);
            ComputeBuffer debugBuf = m_CurrentSolver.GetDebugBuffer();
            debugMat.SetBuffer("_ListBuffer", debugBuf);
            RenderTexture debugRT = m_CurrentSolver.GetDebugTex();
            debugMat.SetTexture("_DebugRT", debugRT);
            debugMat.SetFloat("_Scale", debugScale);
            Graphics.Blit(boundRT, destination, debugMat, (int)debugMode);
            //Graphics.Blit(sourceRT, destination);
        }

        private void PrepareRTDatas()
        {
            sourceRT = new RenderTexture(resolutionX + 2, resolutionY + 2, 0, RenderTextureFormat.RFloat);
            boundRT = new RenderTexture(resolutionX + 2, resolutionY + 2, 0, RenderTextureFormat.RInt);
            sourceRT.filterMode = FilterMode.Point;
            boundRT.filterMode = FilterMode.Point;
            sourceRT.enableRandomWrite = true;
            boundRT.enableRandomWrite = true;
            sourceRT.Create();
            boundRT.Create();

            buildBoundShader.SetTexture(0, "_BoundTex", boundRT);
            buildBoundShader.SetTexture(0, "_SDFTarget", sourceRT);
            buildBoundShader.SetInt("_ResolutionX", resolutionX + 2);
            buildBoundShader.SetInt("_ResolutionY", resolutionY + 2);
            buildBoundShader.Dispatch(0, resolutionX + 2, resolutionY + 2, 1);
            // 1、 generate sourceRT from a Texture2D
            // if (sourceTexture != null)
            // {
            //     SDFData.GenerateBeginRT2D(sourceTexture, ref sourceRT, resolutionX, resolutionY);
            // }
            // 2、 generate standard Geometry
            // xy is center coord, w is circle radius
            List<Vector4> EmitCircles = new List<Vector4>();
            EmitCircles.Add(new Vector4(0.9f, 0.66f, 0f, 0.3f * Mathf.Min(resolutionX, resolutionY)));
            EmitCircles.Add(new Vector4(0.3f, 0.3f, 0f, 0.1f * Mathf.Min(resolutionX, resolutionY)));
            for (int i = 0; i < EmitCircles.Count; ++i)
            {
                buildBoundShader.SetVector("_EmitCircle", EmitCircles[i]);
                buildBoundShader.SetTexture(1, "_SDFTarget", sourceRT);
                buildBoundShader.SetInt("_ResolutionX", resolutionX + 2);
                buildBoundShader.SetInt("_ResolutionY", resolutionY + 2);
                buildBoundShader.Dispatch(1, resolutionX + 2, resolutionY + 2, 1);
            }

        }

        private void PrepareRes(LevelSet2DRes res)
        {
            buildBoundShader = res.LevelSet2DInitRT;
            debugShader = res.DebugShader;
            if (debugShader != null && debugMat == null)
                debugMat = new Material(debugShader);
        }

        private void ReleaseRTDatas()
        {
            if (sourceRT != null)
                sourceRT.Release();
            if (boundRT != null)
                boundRT.Release();
        }

        private void UpdateSolver()
        {
            m_CurrentSolver.OnUpdateSystem(true, sourceRT, boundRT);
        }

        private void OnDestroy()
        {
            if (m_SystemIsReady)
                m_CurrentSolver.OnDestroySystem();
            ReleaseRTDatas();
            if (debugMat != null)
                Destroy(debugMat);
        }

        private void CheckSystemStateBegin()
        {
            if (m_SystemShouldInit)
            {
                m_SystemIsReady = false;
                // check compute shader res
                if (!CheckComputeShaderResources())
                    return;
                // reset system
                solvers.TryGetValue(m_HistorySolverMode, out m_HistorySolver);
                solvers.TryGetValue(solverMode, out m_CurrentSolver);

                m_HistorySolver.OnDestroySystem();
                ReleaseRTDatas();
                PrepareRes(resourcesLiquid2D);
                PrepareRTDatas();
                m_CurrentSolver.OnInitSystem(resolutionX, resolutionY, totalLengthX, totalLengthY, dilateTime, resourcesLiquid2D);

                m_SystemShouldInit = false;
                m_SystemIsReady = true;
            }
        }

        private void CheckSystemStateEnd()
        {
            if(m_SystemIsReady)
                m_HistorySolverMode = solverMode;
        }

        private bool CheckComputeShaderResources()
        {
            if (resourcesLiquid2D == null)
                return false;
            if (resourcesLiquid2D.LevelSet2DFMMStarComputeShader == null)
                return false;
            if (resourcesLiquid2D.LevelSet2DFIMComputeShader == null)
                return false;
            if (resourcesLiquid2D.LevelSet2DFSMComputeShader == null)
                return false;
            if (resourcesLiquid2D.LevelSet2DInitRT == null)
                return false;
            if (resourcesLiquid2D.DebugShader == null)
                return false;
            return true;
        }

        // Editor call back
        public void ResetSystem()
        {
            m_SystemShouldInit = true;
        }

    }
}
