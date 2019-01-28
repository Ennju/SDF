#if !defined(WATERGEOMETRY_INCLUDED)
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members vertex)
#pragma exclude_renderers d3d11
#define WATERGEOMETRY_INCLUDED
// ¡ü y			
// |		   7-----------6			
// |		  /			  /|
// |	  z	 /			 / |
// |     /	4-----------5  |
// |    /	|			|  |  
// |   /	|   3		|  2
// |  /		|			| /
// | /		|			|/
// |/		0-----------1
// -------------------------¡ú x
static const int shapeVertexNum = 8;
static const float4 vertexOffset[shapeVertexNum] = {
	float4(-1, -1, -1, 0),
	float4(1, -1, -1, 0),
	float4(1, -1, 1, 0),
	float4(-1, -1, 1, 0),
	float4(-1, 1, -1, 0),
	float4(1, 1, -1, 0),
	float4(1, 1, 1, 0),
	float4(-1, 1, 1, 0)
};

struct v2f
{
	float4 vertex : SV_POSITION;
	float3 normal : TEXCOORD1;
};

float _EdgeLength;

[maxvertexcount(36)]
void Geometry(
	point v2f i[1],
	inout TriangleStream<v2f> stream
) {
	v2f p[shapeVertexNum];
	p[0].vertex = mul(UNITY_MATRIX_VP, i[0].vertex + _EdgeLength * vertexOffset[0]);
	p[1].vertex = mul(UNITY_MATRIX_VP, i[0].vertex + _EdgeLength * vertexOffset[1]);
	p[2].vertex = mul(UNITY_MATRIX_VP, i[0].vertex + _EdgeLength * vertexOffset[2]);
	p[3].vertex = mul(UNITY_MATRIX_VP, i[0].vertex + _EdgeLength * vertexOffset[3]);
	p[4].vertex = mul(UNITY_MATRIX_VP, i[0].vertex + _EdgeLength * vertexOffset[4]);
	p[5].vertex = mul(UNITY_MATRIX_VP, i[0].vertex + _EdgeLength * vertexOffset[5]);
	p[6].vertex = mul(UNITY_MATRIX_VP, i[0].vertex + _EdgeLength * vertexOffset[6]);
	p[7].vertex = mul(UNITY_MATRIX_VP, i[0].vertex + _EdgeLength * vertexOffset[7]);
	p[0].normal = vertexOffset[0];
	p[1].normal = vertexOffset[1];
	p[2].normal = vertexOffset[2];
	p[3].normal = vertexOffset[3];
	p[4].normal = vertexOffset[4];
	p[5].normal = vertexOffset[5];
	p[6].normal = vertexOffset[6];
	p[7].normal = vertexOffset[7];
	//for (int i = 0; i < shapeVertexNum; ++i) {
	//	p[i].vertex = mul(UNITY_MATRIX_VP, i.vertex + _EdgeLength * vertexOffset[i]);
	//}
	stream.Append(p[2]);
	stream.Append(p[3]);
	stream.Append(p[7]);
	stream.Append(p[2]);
	stream.Append(p[7]);
	stream.Append(p[6]);

	stream.Append(p[0]);
	stream.Append(p[1]);
	stream.Append(p[5]);
	stream.Append(p[0]);
	stream.Append(p[5]);
	stream.Append(p[4]);

	stream.Append(p[3]);
	stream.Append(p[0]);
	stream.Append(p[4]);
	stream.Append(p[3]);
	stream.Append(p[4]);
	stream.Append(p[7]);

	stream.Append(p[1]);
	stream.Append(p[2]);
	stream.Append(p[6]);
	stream.Append(p[1]);
	stream.Append(p[6]);
	stream.Append(p[5]);

	stream.Append(p[4]);
	stream.Append(p[5]);
	stream.Append(p[6]);
	stream.Append(p[4]);
	stream.Append(p[6]);
	stream.Append(p[7]);

	stream.Append(p[2]);
	stream.Append(p[3]);
	stream.Append(p[0]);
	stream.Append(p[2]);
	stream.Append(p[0]);
	stream.Append(p[1]);

}




#endif