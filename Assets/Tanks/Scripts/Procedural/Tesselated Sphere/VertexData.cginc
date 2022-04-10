#include "UnityCG.cginc"
  #include "Lighting.cginc"
             #include "AutoLight.cginc"
static float PI = 3.14159265359;

 struct v2h {
	UNITY_VERTEX_INPUT_INSTANCE_ID
	float4 pos : SV_POSITION;
};
struct h2d
{
	float4 vertex : POSITION;
};

struct d2f
{
  float4 pos : SV_POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
	float4 worldPos : TEXCOORD1;
};

   struct appdata
            {
    UNITY_VERTEX_INPUT_INSTANCE_ID
	float4 vertex : POSITION;
	float3 normal : NORMAL;
};
      struct TessellationControlPoint {
	float4 vertex : INTERNALTESSPOS;
	float3 normal : NORMAL;
};
       struct PatchConstantOutput
{
    float edges[4] : SV_TessFactor;
    float inside[2] : SV_InsideTessFactor;
};     