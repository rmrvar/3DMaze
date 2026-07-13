Shader "Custom/MazeFloor"
{
    Properties
    {
    	_MazeCenter ("Maze Center", Vector) = (0, 0, 0)
    	_MazeRadius ("Maze Radius", Float) = 20
    	_FloorColor ("Floor Color", Color) = (1, 1, 1, 1)
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
            };

            struct FRAG_IN
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
				float3 _MazeCenter;
				float _MazeRadius;
                float4 _FloorColor;
            CBUFFER_END

            FRAG_IN vert (VERT_IN i)
            {
                FRAG_IN o;
                o.positionCS = TransformObjectToHClip(i.positionOS);
                o.positionWS = mul(unity_ObjectToWorld, i.positionOS.xyz).xyz;
                return o;
            }

            half4 frag (FRAG_IN i) : SV_Target
            {   
                float3 lookOriginWS = _WorldSpaceCameraPos;
                float3 lookDirectionWS = normalize(-GetWorldSpaceViewDir(i.positionWS));

                float3 s = lookDirectionWS;
                float3 o = lookOriginWS;
                float3 c = _MazeCenter;
                float3 r = _MazeRadius;

                float t = o - c;
                float ka = dot(s, s);
                float kb = 2 * dot(s, t);
                float kc = - r * r + dot(t, t);

                float determinant = kb * kb - 4 * ka * kc;
                float root1 = -kb + sqrt(max(0, determinant)) / (2 * ka);
                float root2 = -kb + sqrt(max(0, determinant)) / (2 * ka);
                float root = max(root1, root2);

                float3 position = o + s * root;
                float3 normal = normalize(_MazeCenter - position);

                Light mainLight = GetMainLight();
                float3 lightColor = mainLight.color * saturate(dot(normal, mainLight.direction));
                // Use a simpler ambient fetch
                float3 ambient = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w); 
                float3 finalColor = _FloorColor * (lightColor + ambient);

                return half4(finalColor, _FloorColor.a);
            }
            
            ENDHLSL
        }
    }
}