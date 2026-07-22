Shader "Custom/MazeFloor"
{
    Properties
    {
    	_MazeCenter ("Maze Center", Vector) = (0, 0, 0)
    	_MazeRadius ("Maze Radius", Float) = 20
    	_FloorColor ("Floor Color", Color) = (1, 1, 1, 1)
    	_StencilInputValue ("Stencil Input Value", Int) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Stencil
        {
            Ref [_StencilInputValue]  
            Comp NotEqual 
        }
        
        Pass
        {
        	ZWrite On
			ZTest Less
			
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct VERT_IN
            {
                float4 positionOS : POSITION;
            };

            struct FRAG_IN
            {
                float4 positionCS : SV_POSITION;
                //float3 positionWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
				float3 _MazeCenter;
				float _MazeRadius;
                float4 _FloorColor;
            CBUFFER_END

            FRAG_IN vert(VERT_IN i)
            {
                float3 positionOS = i.positionOS.xyz;
                FRAG_IN o;
                o.positionCS = TransformObjectToHClip(positionOS);
                //o.positionWS = TransformObjectToWorld(positionOS);
                return o;
            }

            half4 frag(FRAG_IN i) : SV_Target
            {   
                return _FloorColor;
                /*
                float3 lookOriginWS = _WorldSpaceCameraPos;
                float3 lookDirectionWS = normalize(-GetWorldSpaceViewDir(i.positionWS));

                float3 s = lookDirectionWS;
                float3 o = lookOriginWS;
                float3 c = _MazeCenter;
                float3 r = _MazeRadius;

                // From vector equation of sphere intersecting with vector equation of ray/line and distributive property of scalar product.
                float t = o - c;
                float ka = dot(s, s);
                float kb = 2 * dot(s, t);
                float kc = dot(t, t) - r * r;
                float temp1 = sqrt(max(0, kb * kb - 4 * ka * kc));
                float temp2 = 2 * ka;
                float root1 = (-kb + temp1) / temp2;
                float root2 = (-kb - temp1) / temp2;
                float root = max(root1, root2);

                float3 position = o + s * root;

                float3 ndc = ComputeNormalizedDeviceCoordinatesWithZ(position, UNITY_MATRIX_VP);//i.positionCS);
                float2 screenUV = ndc.xy;
				float sceneDepth = SampleSceneDepth(screenUV);

                //return float4(sceneDepth, 0, 0, 1);// float4(sceneDepth, sceneDepth, sceneDepth, 1);
                
            	float linearDepth = LinearEyeDepth(sceneDepth, _ZBufferParams);
                // TODO: Figure out how to fix this clipping (suspicion, vert shader writes depth which is always less due to being OBB)
                //clip(sceneDepth - i.positionCS.z + 0.0001);
            	//outDepth = ndc.z;

                float3 normal = normalize(_MazeCenter - position);

                Light mainLight = GetMainLight();
                float3 lightColor = mainLight.color * saturate(dot(normal, mainLight.direction));
                // Use a simpler ambient fetch
                float3 ambient = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w); 
                float3 finalColor = _FloorColor.xyz * (lightColor + ambient);

                return half4(finalColor, _FloorColor.a);
				*/
            }
            
            ENDHLSL
        }
    }
}