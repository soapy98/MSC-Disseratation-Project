Shader "Custom/SphereifyCube"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1) //The color of our object
        _Shininess ("Shininess", Float) = 10 //Shininess
        _SpecColor ("Specular Color", Color) = (1, 1, 1, 1) //Specular highlights color
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Pass
        {
        CGPROGRAM
           
            #pragma target 4.6
            #pragma vertex MyTessellationVertexProgram
            #pragma fragment frag
            #pragma hull Hul
            #pragma domain Dom
            #include "HullTess.cginc"
           
    TessellationControlPoint MyTessellationVertexProgram (appdata v) {
	TessellationControlPoint p;
	p.vertex = v.vertex;
	p.normal = v.normal;
	return p;
}


   uniform float4 _LightColor0; 
       uniform float4 _Color; //Use the above variables in here
                uniform float4 _SpecColor;
                uniform float _Shininess;


       fixed4 frag (d2f i):SV_Target
            {
                    float3 normalDirection = normalize( i.normal);
                    float3 viewDirection = normalize(_WorldSpaceCameraPos - i.worldPos.xyz);

                    float3 vert2LightSource = _WorldSpaceLightPos0.xyz - i.worldPos.xyz;
                    float oneOverDistance = 1.0 / length(vert2LightSource);
                    float attenuation = lerp(1.0, oneOverDistance, _WorldSpaceLightPos0.w); //Optimization for spot lights. This isn't needed if you're just getting started.
                    float3 lightDirection = _WorldSpaceLightPos0.xyz - i.worldPos.xyz * _WorldSpaceLightPos0.w;

                    float3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb; //Ambient component
                    float3 diffuseReflection = attenuation * _LightColor0.rgb * _Color.rgb * max(0.0, dot(normalDirection, lightDirection)); //Diffuse component
                    float3 specularReflection;
                    if (dot(i.normal, lightDirection) < 0.0) //Light on the wrong side - no specular
                    {
                        specularReflection = float3(0.0, 0.0, 0.0);
                	  }
                    else
                    {
                        //Specular component
                        specularReflection = attenuation * _LightColor0.rgb * _SpecColor.rgb * pow(max(0.0, dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
                    }

                    float3 color = (ambientLighting + diffuseReflection); //Texture is not applient on specularReflection
                    return float4(color, 1.0);
            }
        ENDCG
        }
    }
}