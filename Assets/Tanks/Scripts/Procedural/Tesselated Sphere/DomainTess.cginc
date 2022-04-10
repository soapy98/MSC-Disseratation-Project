// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#include "VertexData.cginc"
 #pragma vertex vert

float3 CreateBinormal (float3 norm, float3 tang, float binormSign) {
	float3 normaltang = cross(norm, tang.xyz);
	return normaltang *(binormSign * unity_WorldTransformParams.w);
}
  d2f vert (appdata v)
            {
                d2f i;
	UNITY_INITIALIZE_OUTPUT(d2f, i);
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, i);
	i.pos = UnityObjectToClipPos(v.vertex);
	  i.worldPos = mul(unity_ObjectToWorld, i.pos);
	i.uv = (sign(v.vertex.xy) + 1.0) / 2.0;
    i.normal = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
	return i;
}
[UNITY_domain("quad")]
d2f Dom(in PatchConstantOutput input, in const float2 domain : SV_DomainLocation, const OutputPatch<TessellationControlPoint, 4> patch)
{
   appdata data;
   //macro to interpolate off original triangle domain through the barycentrix coordinates
	data.vertex.x = 1.0f * cos((domain.x + 0.5f) * (2 * PI)) * sin((domain.y + 0.5f) * (2 * PI)) * patch[0].vertex;
    data.vertex.y = 1.0f * sin((domain.x + 0.5f) * (2 * PI)) * sin((domain.y + 0.5f) * (2 * PI)) * patch[1].vertex;
    data.vertex.z = 1.0f * cos((domain.y + 0.5f) * (2 * PI)) * patch[2].vertex;
     data.vertex.w =1;
       
     data.normal.x = 1.0f * cos((domain.x + 0.5f) * (2 * PI)) * sin((domain.y + 0.5f) * (2 * PI)) * patch[0].normal;
   data.normal.y=1.0f * sin((domain.x + 0.5f) * (2 * PI)) * sin((domain.y + 0.5f) * (2 * PI)) * patch[1].normal;
   data.normal.z=1.0f * cos((domain.y + 0.5f) * (2 * PI)) * patch[2].normal;

    return vert(data);
}