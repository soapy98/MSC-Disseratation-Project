Shader "Unlit/Test"
{
    Properties
    {
         _Color ("Color", Color) = (0,0,1,1)
    }
    SubShader
    {
        Tags{  "Queue"="Geometry+1" "DisableBatching"="True" "IgnoreProjector"="True" }
         ZWrite On
        Pass
        {
            CGPROGRAM
             #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0
            fixed4 _Color;
            #define Max_Step 32
            #define Thickness 0.001
            #define Epsilon 0.01
            uniform float4 _LightColor0; 

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            { 
                float4 vertexPos : SV_POSITION;
                float4 LocalPos : TEXCOORD0;
                float4 viewDir : TEXCOORD1;
            };
            v2f vert (appdata v)
            {
                v2f o;
                o.vertexPos = UnityObjectToClipPos(v.vertex);
                o.LocalPos = v.vertex;
                float4 CamToObjectSpace = mul(unity_WorldToObject,float4(_WorldSpaceCameraPos,1));
                o.viewDir = v.vertex-CamToObjectSpace;
                return o;
            }
            
            float sphereSDF(float3 pos,float radius){
                 return length(pos)-radius;
            }
              float sphereSDF(float3 pos){
                 return length(pos)-0.5f;
            }
            float3 calculateNormal(float3 pos){
                 float x = sphereSDF(pos+float3(Epsilon,0,0))-sphereSDF(pos-float3(Epsilon,0,0));
                 float y = sphereSDF(pos+float3(0,Epsilon,0))-sphereSDF(pos-float3(0,Epsilon,0));
                 float z = sphereSDF(pos+float3(0,0,Epsilon))-sphereSDF(pos-float3(0,0,Epsilon));
                 float3 surface = float3(x,y,z);
                 surface = mul(unity_ObjectToWorld,float4(surface,0));
                 return normalize(surface);
            }

            float4 lightCalc(float3 pos){
                float3 surfaceNorm = calculateNormal(pos);
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float LightAngle = saturate(dot(surfaceNorm,lightDir));
                return LightAngle*_LightColor0;
            }
            float4 RenderToSurface(float3 pos){
                float4 lightCol = lightCalc(pos);
                return float4 (_Color*lightCol);
            }
           
            fixed4 frag (v2f i, out float depth:SV_Depth):SV_Target
            {
                 float3 pos = i.LocalPos;
                 float3 direction = normalize(i.viewDir.xyz);
                 float step=0;
                 float3 sample =0;
                 bool hit =false;
                 for (uint x=0;x<Max_Step;x++)
                 {
                    sample=pos+direction*step;
                    float dist = sphereSDF(sample,0.5f);
                    if(dist<Thickness)
                    {
                        hit=true;
                        break;
                    }
                    step+=dist;
                    
                 }
                 if(!hit)
                   discard;
                float4 colour = RenderToSurface(sample);
                //calculate surface depth
                float4 tracedClipPos = UnityObjectToClipPos(float4(sample, 1.0));
                depth = tracedClipPos.z / tracedClipPos.w;
                return colour;
            }
            ENDCG
        }
    }
}
