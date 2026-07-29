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
    	_StencilOutputValue ("Stencil Output Value", Int) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" }

        Stencil
        {
            Ref [_StencilOutputValue]  
            Comp Always 
            Pass Replace
        }
        
        Pass
        {
        	//Name "ForwardLit"
			//Tags { "LightMode" = "UniversalForward" }
        
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
                float3 positionWS : TEXCOORD0;
            };

			struct FRAG_OUT
            {
                float4 color : SV_Target;
                //float depth  : SV_Depth;
            };

            //#define MAX_RAYMARCH_STEPS 100
            //#define RAYMARCH_THRESHOLD 0.01
            #define MAX_RAYMARCH_STEPS 150
            #define RAYMARCH_THRESHOLD 0.01

            float _PrevIsRaised;
            float _CurrIsRaised;
            float3 _WallU;
            float3 _WallV;
            float3 _WallW;
            float3 _WallExtents;
            float3 _WallCenter;
            float _AnimProgress;

            CBUFFER_START(UnityPerMaterial)
				float3 _MazeCenter;
				float _MazeRadius;
                float _WallHeight;
				float _WallRadius;
                float4 _TopColor;
				float4 _SideColor;
            CBUFFER_END

            FRAG_IN vert(VERT_IN i)
            {
                float3 positionOS = i.positionOS.xyz;
                FRAG_IN o;
                o.positionCS = TransformObjectToHClip(positionOS);
                o.positionWS = TransformObjectToWorld(positionOS);
                return o;
            }

            float SDF(float3 position)
			{
				const float3 wallU = _WallU;
				const float3 wallV = _WallV;
				const float3 wallW = _WallW;
				const float3 wallExtents = _WallExtents;
				const float3 wallCenter = _WallCenter;

                const float height = lerp(_PrevIsRaised, _CurrIsRaised, saturate(_AnimProgress)) * _WallHeight;

                const float dx = dot(position - wallCenter, wallU);

                // Spheres
                const float innerSphereSD = -(length(position - _MazeCenter) - (_MazeRadius - height));
                const float outerSphereSD = length(position - _MazeCenter) - _MazeRadius;

                // X slope
                const float bufferedWidth = _WallRadius + 0;
                const float xExtentSD = abs(dx) - bufferedWidth - bufferedWidth * (outerSphereSD / _MazeRadius);

                // Quadratic formula (from cicle centered on (0, _MazeRadius) and we want to find y at x=_WallExtents.z). Finds intersection of Y axis on z extents.
            	const float a = 1;
                const float b = -2 * _MazeRadius;
                const float c = wallExtents.z * wallExtents.z;
                const float discriminant = max(0, b * b - 4 * a * c);
                const float root1 = (-b + sqrt(discriminant)) / (2 * a);
                const float root2 = (-b - sqrt(discriminant)) / (2 * a);
                const float root = max(root1, root2);
				const float deltaY = root;

                // The tangents and normals of the two planes intersecting the cylinders.
                const float3 tangent1 = normalize(+wallExtents.z * wallW + (_MazeRadius - deltaY) * -wallV);
                const float3 tangent2 = normalize(-wallExtents.z * wallW + (_MazeRadius - deltaY) * -wallV);
                const float3 normal1 = +normalize(cross(tangent1, wallU));
                const float3 normal2 = -normalize(cross(tangent2, wallU));

                // Moves left/right away from z extents by minimum angle to fit _WallRadius
                const float theta = asin(_WallRadius / _MazeRadius);
                const float3 coneForward1 = normal1 * sin(theta) + tangent1 * cos(theta);
                const float3 coneForward2 = normal2 * sin(theta) + tangent2 * cos(theta);

            	const float3 fromTo = position - _MazeCenter;

                // Double-Cone 1
                const float prllDelta1 = dot(fromTo, coneForward1);
                const float3 prllComponent1 = coneForward1 * prllDelta1;
                const float3 perpComponent1 = fromTo - prllComponent1;

                const float perpRadius1 = abs(prllDelta1) / _MazeRadius * _WallRadius;
                const float3 newTangent1 = normalize(normalize(perpComponent1) * perpRadius1 + prllComponent1); // The tangents and normals of plane through closest point on cylinder.
                const float3 newRight1 = cross(newTangent1, normalize(perpComponent1));
                const float3 newNormal1 = -cross(newTangent1, newRight1);
                const float doubleConeDist1 = dot(fromTo, newNormal1);

                // Double-Cone 2
                const float prllDelta2 = dot(fromTo, coneForward2);
                const float3 prllComponent2 = coneForward2 * prllDelta2;
                const float3 perpComponent2 = fromTo - prllComponent2;

                const float perpRadius2 = abs(prllDelta2) / _MazeRadius * _WallRadius;
                const float3 newTangent2 = normalize(normalize(perpComponent2) * perpRadius2 + prllComponent2); // The tangents and normals of plane through closest point on cylinder.
                const float3 newRight2 = cross(newTangent2, normalize(perpComponent2));
                const float3 newNormal2 = -cross(newTangent2, newRight2);
                const float doubleConeDist2 = dot(fromTo, newNormal2);

                // Actual slants (the other half of cones + everything else)
                const float3 anotherNormal1 = normalize(cross(coneForward1, wallU));
                const float3 anotherNormal2 = normalize(cross(coneForward2, wallU));
                const float proj1 = +dot(position - _MazeCenter, anotherNormal1);
                const float proj2 = -dot(position - _MazeCenter, anotherNormal2);

                return max(max(xExtentSD, max(min(doubleConeDist1, proj1), min(doubleConeDist2, proj2))), max(innerSphereSD, outerSphereSD));
			}

            float3 GetNormal(float3 p)
			{
                const float height = lerp(_PrevIsRaised, _CurrIsRaised, saturate(_AnimProgress)) * _WallHeight;
                if (length(p) < _MazeRadius - height + 0.01)
                {
                    // Top
	                return -normalize(p);
                }

                // COPIED FROM SDF
                // Quadratic formula (from cicle centered on (0, _MazeRadius) and we want to find y at x=_WallExtents.z). Finds intersection of Y axis on z extents.
            	const float a = 1;
                const float b = -2 * _MazeRadius;
                const float c = _WallExtents.z * _WallExtents.z;
                const float discriminant = max(0, b * b - 4 * a * c);
                const float root1 = (-b + sqrt(discriminant)) / (2 * a);
                const float root2 = (-b - sqrt(discriminant)) / (2 * a);
                const float root = max(root1, root2);
				const float deltaY = root;
                
                // The tangents and normals of the two planes intersecting the cylinders.
                const float3 zCrossTangent = normalize(+_WallExtents.z * _WallW + (_MazeRadius - deltaY) * -_WallV);
                const float3 zCrossNormal = +normalize(cross(zCrossTangent, _WallU));
                
                // Moves left/right away from z extents by minimum angle to fit _WallRadius
                const float theta = asin(_WallRadius / _MazeRadius);
                const float3 coneForward = zCrossNormal * sin(theta) + zCrossTangent * cos(theta);
                
                const float y = dot(coneForward, _WallV);
                const float z = dot(coneForward, _WallW);
                const float theta2 = abs(atan2(abs(z), abs(y)));

                const float3 fromToWallCenter = p - _WallCenter;
            	const float3 fromToMazeCenter = p - _MazeCenter;

                const float dy = dot(fromToMazeCenter, _WallV);
                const float dz = dot(fromToMazeCenter, _WallW);
                const float theta3 = abs(atan2(abs(dz), abs(dy)));

                const float buffer = 0;//0.005; // Fights artefacts at borders (radians).
                if (theta3 <= theta2 + buffer)
                {
                    // Sides
                    const float dx = dot(fromToMazeCenter, _WallU);
                    //const float3 tangent =  -normalize(+(sign(dx) * _WallRadius) * _WallU + _WallCenter);
                    //const float3 normal = cross(tangent, _WallW);
                
                    //return normal * sign(dot(fromToWallCenter, normal));

                    float3 forward = _WallV * dy + _WallW * dz;
                    const float3 right = normalize(cross(fromToMazeCenter, forward));
					const float3 normal = normalize(cross(right, fromToMazeCenter));

                    return normal * sign(dot(fromToWallCenter, normal));

                    //const float3 normal = normalize(cross(fromToMazeCenter, _WallW));
                	//return normal * sign(dot(fromToWallCenter, normal));
                }

            	const float2 e = float2(0.01, 0.0);
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

            FRAG_OUT frag(FRAG_IN i, out float depth : SV_Depth) : SV_Target
            {   
                const float3 lookOriginWS = _WorldSpaceCameraPos;
                const float3 lookDirectionWS = normalize(i.positionWS - lookOriginWS);
                const Hit hit = RayMarch(lookOriginWS, lookDirectionWS);
            	clip(MAX_RAYMARCH_STEPS - hit.numSteps - 1);

                float3 ndc = ComputeNormalizedDeviceCoordinatesWithZ(hit.wsPoint, UNITY_MATRIX_VP);
                float2 screenUV = ndc.xy;
                depth = ndc.z;

                const float3 normal = hit.wsNormal;
                const Light mainLight = GetMainLight();
                const float3 lightColor = mainLight.color * saturate(dot(normal, mainLight.direction));
                // Use a simpler ambient fetch
                const float3 ambient = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w); 

                const bool isTowardsCenter = dot(normal, normalize(_MazeCenter - hit.wsPoint)) > 0.99;
                const float3 color = lerp(_SideColor, normal * 0.5 + 0.5, isTowardsCenter);

                const float3 finalColor = color * (isTowardsCenter ? 1 : (lightColor + ambient));



                FRAG_OUT output;
                output.color = float4(finalColor, 1);
                return output;
            }
            
            ENDHLSL
        }
    }
}