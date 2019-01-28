using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Liquid2D
{
    public abstract class Liquid2DLSBase {
        public abstract void OnInitSystem(int resolutionX, int resolutionY, float totalLengthX, float totalLengthY, int dilateTime, LevelSet2DRes res);
        public abstract void OnDestroySystem();
        public abstract void OnUpdateSystem(bool rebuildSDF, RenderTexture sourceRT, RenderTexture boundRT);
        
        public virtual ComputeBuffer GetDebugBuffer()
        {
            return null;
        }

        public virtual RenderTexture GetDebugTex()
        {
            return null;
        }
	}
}
