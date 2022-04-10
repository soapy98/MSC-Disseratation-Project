#include "UnityCG.cginc"

#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

UNITY_DEFINE_INSTANCED_PROP(float4, _Color)

sampler2D _ColorMap;
float4 _ColorMap_ST,_DetailTex_ST;
    
sampler2D _NormalMap;
float _BumpScale;
    
sampler2D _HeightMap;
float _ParallaxStrength;
 
float _Metallic;
float _Smoothness;
float Bias0;
    #define RayMarchSteps 30


struct appdata
 {
UNITY_VERTEX_INPUT_INSTANCE_ID
 float4 vertex : POSITION;
 float3 normal : NORMAL;
 float4 tex:TEXCOORD0;
 float2 uv1		: TEXCOORD1;
 float2 uv2	: TEXCOORD2;
  float4 tangent : TANGENT;
};
 struct v2f {
UNITY_VERTEX_INPUT_INSTANCE_ID
  float4 pos : SV_POSITION;
 float3 worldPos:TEXCOORD0;
 float4 tex : TEXCOORD1;
 float3 normal : NORMAL;
#if defined(BINORMAL_PER_FRAGMENT)
    float4 tangent : TEXCOORD2;
 #else
    float3 tangent : TEXCOORD2;
    float3 binormal : TEXCOORD3;
  #endif
float3 ViewDirinTang : TEXCOORD4;
};
            
     float3 CreateBinormal (float3 normal, float3 tangent, float sign) {
     	return cross(normal, tangent.xyz) *	(sign * unity_WorldTransformParams.w);
     }     
UnityLight CreateLight (v2f i) {
	UnityLight light;
	light.dir = _WorldSpaceLightPos0.xyz;
	UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz);
	light.color = _LightColor0.rgb * attenuation;
	return light;
}
       UnityIndirect CreateIndirectLight (v2f i, float3 viewDir) {
	UnityIndirect indirectLight;
	indirectLight.diffuse = 0;
	indirectLight.specular = 0;
	return indirectLight;
} 


void InitializeFragmentNormal(inout v2f i) {
	float3 tangentSpaceNormal = UnpackScaleNormal(tex2D(_NormalMap, i.tex.xy), _BumpScale);
	#if defined(BINORMAL_PER_FRAGMENT)
		float3 binormal =CreateBinormal(i.normal, i.tangent.xyz, i.tangent.w);
	#else
		float3 binormal = i.binormal;
	#endif
	
	i.normal = normalize(
		tangentSpaceNormal.x * i.tangent +
		tangentSpaceNormal.y * binormal +
		tangentSpaceNormal.z * i.normal
	);
}

float GetHeight (float2 uv) {
	return tex2D(_HeightMap, uv).g;
}

bool SurfaceHeight(float step,float surface)
{
return step>surface;
}

float2 ParallaxRayMarcher(float2 startUV, float2 viewDir)
{
float2 offset =0;
float step = 1.0/RayMarchSteps;
float2 delta = -viewDir*(step*_ParallaxStrength);

float currentStepHeight =1;
float currentSurfaceHeight = GetHeight(startUV);

float previousOffset = offset;
float previousHeight = currentStepHeight;
float previousSurfaceHeight =currentSurfaceHeight;

for(int i=1; i<RayMarchSteps && currentStepHeight>currentSurfaceHeight; i++)
{
    previousOffset = offset;
    previousHeight = currentStepHeight;
    previousSurfaceHeight = currentSurfaceHeight;

    offset-=delta;
    currentStepHeight -= step;
    currentSurfaceHeight = GetHeight(startUV+offset);
}
return offset;
}


void ApplyParallax (inout v2f i) {
		i.ViewDirinTang = normalize(i.ViewDirinTang);
		i.ViewDirinTang.xy /= (i.ViewDirinTang.z + Bias0);
		float2 uvOffset = ParallaxRayMarcher(i.tex.xy, i.ViewDirinTang.xy);
		i.tex.xy += uvOffset;
		i.tex.zw += uvOffset * (_DetailTex_ST.xy / _ColorMap_ST.xy);
}




float3 GetAlbedo (v2f i) {
	return tex2D(_ColorMap, i.tex.xy).rgb * UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color).rgb;
}
float GetSmoothness (v2f i) {
	float smoothness = 1;
	smoothness = tex2D(_ColorMap, i.tex.xy).a;
	return smoothness * _Smoothness;
}





