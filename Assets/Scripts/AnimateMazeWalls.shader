Shader "Custom/AnimateMazeWalls"
{
    Properties
    {
        _WallHeight ("Wall Height", Float) = 2.0
        _AnimProgress ("Anim Progress", Range(0,1)) = 0.0
        _BaseColor ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct VERT_IN
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL; 
                float2 stateData  : TEXCOORD0;  // x: Prev (0 or 1), y: Curr (0 or 1)
                float3 upVector   : TEXCOORD1;  // Wall Normal
            };

            struct FRAG_IN
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD3; 
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _WallHeight;
                float _AnimProgress;
            CBUFFER_END

            FRAG_IN vert (VERT_IN v)
            {
                FRAG_IN o;

                float t = smoothstep(0, 1, _AnimProgress);

                // 1 - 0 = +1 (Growing)
                // 0 - 1 = -1 (Shrinking)
                // 0 - 0 =  0 (No change)
                // 1 - 1 =  0 (No change)
                float stateDelta = v.stateData.y - v.stateData.x;
                float heightOffset = stateDelta * _WallHeight * (1.0 - t);
                float3 displacedPos = v.positionOS.xyz - (v.upVector * heightOffset);
                o.positionCS = TransformObjectToHClip(displacedPos);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                
                return o;
            }

            half4 frag (FRAG_IN i) : SV_Target
            {   
                // Pulled random Lambertian lighting from internet. Doesn't matter.
                float3 normal = normalize(i.normalWS);
                Light mainLight = GetMainLight();
                float3 lightColor = mainLight.color * saturate(dot(normal, mainLight.direction));
                // Use a simpler ambient fetch
                float3 ambient = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w); 
                float3 finalColor = _BaseColor.rgb * (lightColor + ambient);
                
                return half4(finalColor, _BaseColor.a);
            }
            
            ENDHLSL
        }
    }
}