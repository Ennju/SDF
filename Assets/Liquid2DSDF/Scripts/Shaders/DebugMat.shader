Shader "Unlit/DebugMat"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Scale("Scale", float) = 0.5
	}
	SubShader{
		ZTest Always Cull Off ZWrite Off

		HLSLINCLUDE
		#pragma vertex vert  
		#pragma fragment frag  
		#pragma target 4.5  
		#include "UnityCG.cginc"  
		#include "LiquidUtils.hlsl"
		#include "Liquid2DDef.hlsl"
		Texture2D<uint> _BoundRT;
		Texture2D<float> _SourceRT;
		Texture2D<float> _DebugRT;
		uint _ResolutionX;
		uint _ResolutionY;
		float _Scale;
		RWStructuredBuffer<uint2> _ListBuffer;
		struct v2f {
			float4 pos : SV_POSITION;
			half2 uv: TEXCOORD0;
		};

		v2f vert(appdata_img v) {
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.texcoord;
			return o;
		}

		uint2 GetTileFromUV(float2 uv) {
			float2 tileSpacing = float2(1, 1) / float2(_ResolutionX, _ResolutionY);
			float2 scale = uv / tileSpacing;
			return uint2(scale.xy);
		}
		ENDHLSL

	Pass{ // 0: boundrt
		HLSLPROGRAM

		fixed4 frag(v2f i) : SV_Target{
			uint2 tileIndex = GetTileFromUV(i.uv);

			bool isBoundary = GetBoundaryR2D(_BoundRT, tileIndex, BOUND_TEX_IS_BOUNDARY);
			return isBoundary ? 1 : 0;
		}

		ENDHLSL
	}

	Pass{ // 1: sourcert
		HLSLPROGRAM

		fixed4 frag(v2f i) : SV_Target{
			uint2 tileIndex = GetTileFromUV(i.uv);

			float debugVal = _DebugRT[tileIndex];
			//if (debugVal < -1000)
			//	return 1;
			//if (debugVal > 10000000)
			//	return 0.5;
			//return 0;

			return debugVal * _Scale;
			return _SourceRT[tileIndex];
			bool test = _SourceRT[tileIndex] > 0;
			return test ? 1 : 0;
		}

			ENDHLSL
		}

	}

		Fallback Off
}
