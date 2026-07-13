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

            #define MAX_RAYMARCH_STEPS 160
            #define RAYMARCH_THRESHOLD 0.005

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
                float height = _AnimProgress * _WallHeight;

                float dx = dot(position - _WallCenter, _WallRight);
                float dy = dot(position - _WallCenter, _WallUp);
                float dz = dot(position - _WallCenter, _WallForward);

                // Spheres
                float innerSphereSD = (_MazeRadius - height) - length(position - _MazeCenter);
                float outerSphereSD = length(position - _MazeCenter) - _MazeRadius;
				
                // Box with slant on X
                float xExtentSD = abs(dx) - _WallRadius - _WallRadius * (outerSphereSD / _MazeRadius);
                float yExtentSD = abs(dy) - _WallExtents.y;
                float zExtentSD = abs(dz) - _WallExtents.z;
                float boxSD = max(xExtentSD, max(yExtentSD, zExtentSD));

                // Quadratic formula (from cicle centered on (0, _MazeRadius) and we want to find y at x=_WallExtents.z).
                // Finds intersection of Y axis on z extents.
            	float a = 1;
                float b = -2 * _MazeRadius;
                float c = _WallExtents.z * _WallExtents.z;
                float discriminant = max(0, b * b - 4 * a * c);
                float root1 = (-b + sqrt(discriminant)) / (2 * a);
                float root2 = (-b - sqrt(discriminant)) / (2 * a);
                float root = max(root1, root2);
				float deltaY = root;

                // Slants
                float3 tangent1 = normalize(+_WallExtents.z * _WallForward + (_MazeRadius - deltaY) * -_WallUp);
                float3 tangent2 = normalize(-_WallExtents.z * _WallForward + (_MazeRadius - deltaY) * -_WallUp);
                float3 normal1 = +normalize(cross(tangent1, _WallRight));
                float3 normal2 = -normalize(cross(tangent2, _WallRight));

                // Moves left/right from z extents by minimum angle to fit _WallRadius
                float theta = 2 * asin(_WallRadius / _MazeRadius);
                float3 coneForward1 = normal1 * sin(theta) + tangent1 * cos(theta);
                float3 coneForward2 = normal2 * sin(theta) + tangent2 * cos(theta);

            	float3 fromTo = position - _MazeCenter;

                // Half-Cone 1
                float prllDelta1 = dot(fromTo, coneForward1);
                float3 prllComponent1 = coneForward1 * prllDelta1;
                float3 perpComponent1 = fromTo - prllComponent1;

                float perpRadius1 = abs(prllDelta1) / _MazeRadius * _WallRadius;
                float3 newTangent1 = normalize(perpComponent1) * perpRadius1 + prllComponent1;
                float3 newRight1 = cross(normalize(newTangent1), normalize(perpComponent1));
                float3 newNormal1 = -normalize(cross(newTangent1, newRight1));
                float dist1_1 = dot(fromTo, newNormal1);
                float dist2_1 = prllDelta1;
                float dist1 = max(dist1_1, dist2_1);

                // Half-Cone 2
                float prllDelta2 = dot(fromTo, coneForward2);
                float3 prllComponent2 = coneForward2 * prllDelta2;
                float3 perpComponent2 = fromTo - prllComponent2;

                float perpRadius2 = abs(prllDelta2) / _MazeRadius * _WallRadius;
                float3 newTangent2 = normalize(perpComponent2) * perpRadius2 + prllComponent2;
                float3 newRight2 = cross(normalize(newTangent2), normalize(perpComponent2));
                float3 newNormal2 = -normalize(cross(newTangent2, newRight2));
                float dist1_2 = dot(fromTo, newNormal2);
                float dist2_2 = prllDelta2;
                float dist2 = max(dist1_2, dist2_2);

                // Actual slants (the other half of cones + everything else)
                float3 anotherNormal1 = normalize(cross(coneForward1, _WallRight));
                float3 anotherNormal2 = normalize(cross(coneForward2, _WallRight));
                float proj1 = +dot(position - _MazeCenter, anotherNormal1);
                float proj2 = -dot(position - _MazeCenter, anotherNormal2);

                return max(max(boxSD, max(min(dist1, proj1), min(dist2, proj2))), max(innerSphereSD, outerSphereSD));
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
					float dist = abs(SDF(posWS));
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