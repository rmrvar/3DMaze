Shader "Custom/MazeWall"
{
    Properties
    {
    	[Toggle] _PrevIsRaised ("Prev Is Raised", Float) = 0
    	[Toggle] _CurrIsRaised ("Curr Is Raised", Float) = 0
    	_MazeCenter ("Maze Center", Vector) = (0, 0, 0)
    	_MazeRadius ("Maze Radius", Float) = 20
        _WallHeight ("Wall Height", Float) = 2.0
    	_WallRadius ("Wall Radius", Float) = 0.5
    	_WallU ("Wall U", Vector) = (1, 0, 0, 0)
    	_WallV ("Wall V", Vector) = (0, 1, 0, 0)
    	_WallW ("Wall W", Vector) = (0, 0, 1, 0)
    	_WallExtents ("Wall Extents", Vector) = (0, 0, 1, 0)
    	_WallCenter ("Wall Center", Vector) = (0, 0, 1, 0)
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

            float _PrevIsRaised;
            float _CurrIsRaised;
            float3 _WallU;
            float3 _WallV;
            float3 _WallW;
            float3 _WallExtents;
            float3 _WallCenter;

            CBUFFER_START(UnityPerMaterial)
				float3 _MazeCenter;
				float _MazeRadius;
                float _WallHeight;
				float _WallRadius;
                float4 _TopColor;
				float4 _SideColor;
                float _AnimProgress;
            CBUFFER_END

            FRAG_IN vert (VERT_IN i)
            {
                float3 positionOS = i.positionOS.xyz;
                FRAG_IN o;
                o.positionCS = TransformObjectToHClip(positionOS);
                o.positionWS = TransformObjectToWorld(positionOS);
                return o;
            }

            float SDF(float3 position)
			{
				//const float3x3 worldRot = unity_ObjectToWorld;

				//const float3 scale = float3(length(worldRot[0]), length(worldRot[1]), length(worldRot[2]));
				//const float3x3 worldBasis = float3x3(normalize(worldRot[0]), normalize(worldRot[1]), normalize(worldRot[2]));

				const float3 wallU = _WallU;//worldBasis[0];
				const float3 wallV = _WallV;// worldBasis[1];
				const float3 wallW = _WallW;//worldBasis[2];
				const float3 wallExtents = _WallExtents;//scale * 0.5;
				const float3 wallCenter = _WallCenter;//unity_ObjectToWorld[3].xyz;

                const float height = lerp(_PrevIsRaised, _CurrIsRaised, clamp(_AnimProgress, 0, 1)) * _WallHeight;

                const float dx = dot(position - wallCenter, wallU);
                const float dy = dot(position - wallCenter, wallV);
                const float dz = dot(position - wallCenter, wallW);

                // Spheres
                const float innerSphereSD = -(length(position - _MazeCenter) - (_MazeRadius - height));
                const float outerSphereSD = length(position - _MazeCenter) - _MazeRadius;

            	// Box with slant on X
                const float xExtentSD = abs(dx) - _WallRadius - _WallRadius * (outerSphereSD / _MazeRadius);
                const float yExtentSD = abs(dy) - wallExtents.y;
                const float zExtentSD = abs(dz) - wallExtents.z;
                const float boxSD = max(xExtentSD, max(yExtentSD, zExtentSD));

                // Quadratic formula (from cicle centered on (0, _MazeRadius) and we want to find y at x=_WallExtents.z). Finds intersection of Y axis on z extents.
            	const float a = 1;
                const float b = -2 * _MazeRadius;
                const float c = wallExtents.z * wallExtents.z;
                const float discriminant = max(0, b * b - 4 * a * c);
                const float root1 = (-b + sqrt(discriminant)) / (2 * a);
                const float root2 = (-b - sqrt(discriminant)) / (2 * a);
                const float root = max(root1, root2);
				const float deltaY = root;

                // Slants
                const float3 tangent1 = normalize(+wallExtents.z * wallW + (_MazeRadius - deltaY) * -wallV);
                const float3 tangent2 = normalize(-wallExtents.z * wallW + (_MazeRadius - deltaY) * -wallV);
                const float3 normal1 = +normalize(cross(tangent1, wallU));
                const float3 normal2 = -normalize(cross(tangent2, wallU));

                // Moves left/right from z extents by minimum angle to fit _WallRadius
                const float theta = 2 * asin(_WallRadius / _MazeRadius);
                const float3 coneForward1 = normal1 * sin(theta) + tangent1 * cos(theta);
                const float3 coneForward2 = normal2 * sin(theta) + tangent2 * cos(theta);

            	const float3 fromTo = position - _MazeCenter;

                // Half-Cone 1
                const float prllDelta1 = dot(fromTo, coneForward1);
                const float3 prllComponent1 = coneForward1 * prllDelta1;
                const float3 perpComponent1 = fromTo - prllComponent1;

                const float perpRadius1 = abs(prllDelta1) / _MazeRadius * _WallRadius;
                const float3 newTangent1 = normalize(perpComponent1) * perpRadius1 + prllComponent1;
                const float3 newRight1 = cross(normalize(newTangent1), normalize(perpComponent1));
                const float3 newNormal1 = -normalize(cross(newTangent1, newRight1));
                const float dist1_1 = dot(fromTo, newNormal1);
                const float dist2_1 = prllDelta1;
                const float dist1 = max(dist1_1, dist2_1);

                // Half-Cone 2
                const float prllDelta2 = dot(fromTo, coneForward2);
                const float3 prllComponent2 = coneForward2 * prllDelta2;
                const float3 perpComponent2 = fromTo - prllComponent2;

                const float perpRadius2 = abs(prllDelta2) / _MazeRadius * _WallRadius;
                const float3 newTangent2 = normalize(perpComponent2) * perpRadius2 + prllComponent2;
                const float3 newRight2 = cross(normalize(newTangent2), normalize(perpComponent2));
                const float3 newNormal2 = -normalize(cross(newTangent2, newRight2));
                const float dist1_2 = dot(fromTo, newNormal2);
                const float dist2_2 = prllDelta2;
                const float dist2 = max(dist1_2, dist2_2);

                // Actual slants (the other half of cones + everything else)
                const float3 anotherNormal1 = normalize(cross(coneForward1, wallU));
                const float3 anotherNormal2 = normalize(cross(coneForward2, wallU));
                const float proj1 = +dot(position - _MazeCenter, anotherNormal1);
                const float proj2 = -dot(position - _MazeCenter, anotherNormal2);

                //return max(boxSD, max(innerSphereSD, outerSphereSD));
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
                const float3 lookOriginWS = _WorldSpaceCameraPos;
                const float3 lookDirectionWS = normalize(i.positionWS - lookOriginWS);
                const Hit hit = RayMarch(lookOriginWS, lookDirectionWS);
            	clip(MAX_RAYMARCH_STEPS - hit.numSteps - 1);

                const float3 normal = hit.wsNormal;
                const Light mainLight = GetMainLight();
                const float3 lightColor = mainLight.color * saturate(dot(normal, mainLight.direction));
                // Use a simpler ambient fetch
                const float3 ambient = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w); 

                const bool isTowardsCenter = dot(normal, normalize(_MazeCenter - hit.wsPoint)) > 0.5;
                const float3 color = lerp(_SideColor, _TopColor, isTowardsCenter);

                const float3 finalColor = color * (lightColor + ambient);

                return half4(finalColor, _TopColor.a);
            }
            
            ENDHLSL
        }
    }
}