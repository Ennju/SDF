using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Liquid2D
{
    public class LevelSet2DRes : ScriptableObject
    {
        public ComputeShader LevelSet2DFMMStarComputeShader;
        public ComputeShader LevelSet2DFIMComputeShader;
        public ComputeShader LevelSet2DFSMComputeShader;
        public ComputeShader LevelSet2DInitRT;

        public Shader DebugShader;
    }
}