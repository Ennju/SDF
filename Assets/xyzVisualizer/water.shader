Shader "Unlit/water"
{
	Properties
	{
		_Color("Color", Color) = (0,0,1,0)
		_EdgeLength("EdgeLength", float) = 0.005
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry Geometry

			#include "UnityCG.cginc"
			#include "waterGeometry.cginc"

			float4 _Color;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_M, v.vertex);
				o.normal = 0;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 color = _Color;
				float3 l = normalize(-_WorldSpaceLightPos0);
				float3 n = normalize(i.normal);
				float ndotl = dot(n, l)/2 + 0.5;
				float3 diffuse = ndotl * float3(1,1,1);
				color = float4(diffuse, 1) * color;
				return color;
			}
			ENDCG
		}
	}
}
