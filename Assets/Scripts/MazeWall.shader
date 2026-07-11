Shader "Custom/MazeWall"
{
    Properties
    {
    	_MazeCenter ("Maze Center", Vector) = (0, 0, 0)
    	_MazeRadius ("Maze Radius", Float) = 20
    	_WallExtents ("Wall Extents", Vector) = (0.5, 0.5, 0.5)
    	_WallCenter ("Wall Center", Vector) = (0, 0, 0)
    	_WallRight ("Wall Right", Vector) = (1, 0, 0)
    	_WallUp ("Wall Up", Vector) = (0, 1, 0)
    	_WallForward ("Wall Forward", Vector) = (0, 0, 1)
        _WallHeight ("Wall Height", Float) = 2.0
    	_WallRadius ("Wall Radius", Float) = 0.5
    	_TopColor ("Top Color", Color) = (1, 1, 1, 1)
        _SideColor ("Side Color", Color) = (1, 1, 1, 1)
        _AnimProgress ("Anim Progress", Range(0,1)) = 0.0
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
            };

            struct FRAG_IN
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            #define MAX_RAYMARCH_STEPS 100
            #define RAYMARCH_THRESHOLD 0.0025

            CBUFFER_START(UnityPerMaterial)
				float3 _MazeCenter;
				float _MazeRadius;
                float3 _WallCenter;
				float3 _WallExtents;
				float3 _WallRight;
				float3 _WallUp;
				float3 _WallForward;
                float _WallHeight;
				float _WallRadius;
                float4 _TopColor;
				float4 _SideColor;
                float _AnimProgress;
            CBUFFER_END

            FRAG_IN vert (VERT_IN i)
            {
                FRAG_IN o;
                o.positionCS = TransformObjectToHClip(i.positionOS);
                o.positionWS = mul(unity_ObjectToWorld, i.positionOS.xyz).xyz;
                return o;
            }

            float SDF(float3 position)
			{
                float DY = dot(_WallCenter - _MazeCenter, _WallUp);

                float dx = dot(position - _WallCenter, _WallRight);
                float dy = dot(position - _WallCenter, _WallUp);
                float dz = dot(position - _WallCenter, _WallForward);

                // Box
                float xExtentSD = abs(dx) - _WallExtents.x;
                float yExtentSD = abs(dy) - _WallExtents.y;
                float zExtentSD = abs(dz) - _WallExtents.z;
                float boxSD = max(xExtentSD, max(yExtentSD, zExtentSD));

                // Sphere
                float innerSphereSD = _MazeRadius - length(position - _MazeCenter);
				
                // Slants
                float3 tangent1 = normalize(+_WallRadius * _WallRight + (abs(DY) + _WallExtents.y) * -_WallUp);
                float3 tangent2 = normalize(-_WallRadius * _WallRight + (abs(DY) + _WallExtents.y) * -_WallUp);
                float3 normal1 = -normalize(cross(tangent1, _WallForward));
                float3 normal2 = +normalize(cross(tangent2, _WallForward));
                float proj1 = dot(position - _MazeCenter, normal1);
                float proj2 = dot(position - _MazeCenter, normal2);

                float3 tangent3 = normalize(+_WallExtents.z * _WallForward + (abs(DY) + _WallExtents.y) * -_WallUp);
                float3 tangent4 = normalize(-_WallExtents.z * _WallForward + (abs(DY) + _WallExtents.y) * -_WallUp);
                float3 normal3 = +normalize(cross(tangent3, _WallRight));
                float3 normal4 = -normalize(cross(tangent4, _WallRight));

                float theta = lerp(0.5 * PI, 0, abs(dx) / _WallRadius);

                // This one is a slant also, but trying to make a _WallRadius circle on ends of Z axis by adding fake distance
                float proj3 = dot(position - _MazeCenter, normal3) + (1 - sin(theta)) * _WallRadius * (abs(DY) + _WallExtents.y) / (abs(DY) + dy);
                float proj4 = dot(position - _MazeCenter, normal4) + (1 - sin(theta)) * _WallRadius * (abs(DY) + _WallExtents.y) / (abs(DY) + dy);

                // Intersection
            	//return max(boxSD, proj4);
            	return max(max(boxSD, innerSphereSD), max(max(proj1, proj2), max(proj3, proj4)));
            	//return max(boxSD, max(proj3, proj4));
            	//return max(boxSD, max(innerSphereSD, xSliceSD));
			}

            float3 GetNormal(float3 p)
			{
			    const float2 e = float2(0.001, 0.0);
			    return normalize(float3(
			        SDF(p + e.xyy) - SDF(p - e.xyy),
			        SDF(p + e.yxy) - SDF(p - e.yxy),
			        SDF(p + e.yyx) - SDF(p - e.yyx)
			    ));
			}

            struct Hit
			{
				float3 wsPoint;
                float3 wsNormal;
                float wsDist;
				int numSteps;
			};

            Hit RayMarch(float3 rayOriginWS, float3 rayDirectionWS)
			{
                float3 posWS = rayOriginWS;
                float distWS = 0;

                int i = 0;
				//[unroll]
				for ( ; i < MAX_RAYMARCH_STEPS; ++i)
				{
					float dist = SDF(posWS);
                    if (dist < RAYMARCH_THRESHOLD)
                    {
	                    break;
                    }

                    distWS += dist;
                    posWS = rayOriginWS + rayDirectionWS * distWS;
				}

                Hit hit;
                hit.wsPoint = posWS;
                hit.wsNormal = GetNormal(posWS);
				hit.wsDist = distWS;
                hit.numSteps = i;
                return hit;
			}

            half4 frag (FRAG_IN i) : SV_Target
            {   
                float3 lookOriginWS = _WorldSpaceCameraPos;
                float3 lookDirectionWS = normalize(-GetWorldSpaceViewDir(i.positionWS));
                Hit hit = RayMarch(lookOriginWS, lookDirectionWS);
            	clip(MAX_RAYMARCH_STEPS - hit.numSteps - 1);

                float3 normal = hit.wsNormal;
                Light mainLight = GetMainLight();
                float3 lightColor = mainLight.color * saturate(dot(normal, mainLight.direction));
                // Use a simpler ambient fetch
                float3 ambient = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w); 

                bool isTowardsCenter = dot(normal, normalize(_MazeCenter - hit.wsPoint)) > 0.5;
                float3 color = lerp(_SideColor, _TopColor, isTowardsCenter);

                float3 finalColor = color * (lightColor + ambient);

                return half4(finalColor, _TopColor.a);
            }
            
            ENDHLSL
        }
    }
}