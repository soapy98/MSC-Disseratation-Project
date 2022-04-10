Shader "Custom/BumpMapWithRayMarch"
{
    Properties
    {
          _Color ("Tint", Color) = (1, 1, 1, 1)
        _ColorMap ("Color", 2D) = "gray" {}
        
        [NoScaleOffset] _NormalMap ("Normals", 2D) = "bump" {}
		_BumpScale ("Bump Scale", Float) = 1
        
        [NoScaleOffset] _HeightMap ("Heights", 2D) = "gray" {}
        _ParallaxStrength ("Parallax Strength", Range(0, 0.1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.1

		[Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
		Bias0 ("Bias", Range(0, 0.5)) = 0.42

		[HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1
		[HideInInspector] _DstBlend ("_DstBlend", Float) = 0
		[HideInInspector] _ZWrite ("_ZWrite", Float) = 1
    }
    	CGINCLUDE
    #define BINORMAL_PER_FRAGMENT

	ENDCG
    SubShader
    {
    Pass{
        Tags { "RenderType"="Opaque" }
        LOD 200
	Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]
        CGPROGRAM
       #pragma vertex vert
        #pragma fragment frag
#include "LightInput.cginc"
        #pragma target 4.6
  v2f vert (appdata v) {
                v2f o;
  UNITY_INITIALIZE_OUTPUT(v2f, o);
 UNITY_SETUP_INSTANCE_ID(v);
 UNITY_TRANSFER_INSTANCE_ID(v, o);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.worldPos = mul(unity_ObjectToWorld, v.vertex);
  o.tex.xy = TRANSFORM_TEX(v.tex, _ColorMap);
 o.tex.zw = TRANSFORM_TEX(v.tex, _DetailTex);
  o.normal = UnityObjectToWorldNormal(v.normal);
  
  #if defined(BINORMAL_PER_FRAGMENT)
     o.tangent =  float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
 #else
     o.tangent =  UnityObjectToWorldDir(v.tangent.xyz);
     o.binormal = CreateBinormal(o.normal, o.tangent, v.tangent.w);
 #endif
float3x3 mat2Tang = float3x3(v.tangent.xyz,	cross(v.normal, v.tangent.xyz) * v.tangent.w,v.normal);
o.ViewDirinTang = mul(mat2Tang, ObjSpaceViewDir(v.vertex));
   return o;
            }
float4 frag(v2f input) : SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(input);
   ApplyParallax(input);
     InitializeFragmentNormal(input);
     
    float3 specularTint;
	float oneMinusReflectivity;
	
	float3 albedo = DiffuseAndSpecularFromMetallic(
		GetAlbedo(input), _Metallic, specularTint, oneMinusReflectivity
	);
	
	float3 viewDir = normalize(_WorldSpaceCameraPos - input.worldPos.xyz);
	float4 color = UNITY_BRDF_PBS(
		albedo, specularTint,
		oneMinusReflectivity, GetSmoothness(input),
		input.normal, viewDir,
		CreateLight(input), CreateIndirectLight(input, viewDir)
	);
    return color;
}
        ENDCG
        }
    }
}
