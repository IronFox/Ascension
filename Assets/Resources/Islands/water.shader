Shader "Island/Water" {
	Properties {
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 200
		
			
			CGPROGRAM
			#pragma surface surf BlinnPhong alpha vertex:vert
			#pragma glsl
			#pragma target 3.0


			#include "common.cginc"
			#include "sampleTerrainHeight.cginc"

			//addshadow fullforwardshadows 

			sampler2D _UpperHeightMap, _WaterHeightMap, _NormalMap, WaterCoverageMap;
			samplerCUBE SkyMap;
			float4 Region;
			
	
			float sampleHeight(float2 at)
			{
				return SampleHeight(_WaterHeightMap,at);
			}
			
			float3 sampleNormal(float2 at, float h0)
			{
				float 	hx = sampleHeight(at + float2(1.0 / 2048.0,0.0)) - h0,
						hy = sampleHeight(at + float2(0.0,1.0 / 2048.0)) - h0;
				float3 n = float3(-hx*4.0,-hy*4.0,1.0);
				return normalize(n);
			}

			
			struct Input
			{
				float4 world2;
				float4 world;
				float3 tx;
				float3 ty;
				float3 tz;
			};


	
	
	
	
			void vert (inout appdata_full v, out Input o)
			{
				float4 world = mul(_Object2World,v.vertex);
				float2 uv = (world.xz - Region.xy) / (Region.zw - Region.xy);
				float 	h0 = SampleHeight(_UpperHeightMap,uv),
						//h1 = SampleHeight(_LowerHeightMap,uv),
						h2 = SampleHeight(_WaterHeightMap, uv);
						
				v.normal = float3(0.0,0.0,1.0) * _Object2World[1].z;
				float3 t = mul(_World2Object,float4(1.0,0.0,0.0,0.0)).xyz;
				float tl = length(t);
				v.tangent = float4(t / tl /tl,1.0);

				float3 binormal = //mul(_Object2World,float4(cross(v.tangent.xyz,v.normal),0.0)).xyz;
								// cross(mul(_Object2World,float4(v.tangent.xyz,0.0)).xyz,mul(_Object2World,float4(v.normal,0.0)));
								mul(_Object2World,float4(cross(t,v.normal),0.0)).xyz;
				float binormalLen = length(binormal);

				
				world.y = h2;
				v.vertex = mul(_World2Object,world);

				//o = (Input)0.0;
				
				UNITY_INITIALIZE_OUTPUT(Input,o);
				o.tz = sampleNormal(uv,h2);
				o.tx = normalize(cross(o.tz,float3(0.0,1.0,0.0)));
				o.ty = normalize(cross(o.tx,o.tz));
				//if (h1 > h0)
					//h0 = -1000.0;
				o.world = float4(world.xzy,h2 - h0);
				o.world2 = float4(uv.x,uv.y,1.0 / binormalLen,0.0);

			}



			void surf (Input IN, inout SurfaceOutput o) {
				//clip(IN.world.w);

				//float h = IN.world.w *5.0;
				
				float h = tex2D(WaterCoverageMap,IN.world2.xy).x;
				float height = IN.world.w;
				float time = _Time.y;
				float3 world = IN.world.xyz;
				if (h < 0.2 || height < -100.0)
					clip(-1.0);

				float4 result = (float4)(0.0);
				float3 nx = (tex2D(_NormalMap,world.yz*0.1 + float2(0.0,time)*0.1).xyz*2.0 - 1.0)
								+ (tex2D(_NormalMap,world.yz*0.03 + float2(0.0,time)*0.1).xyz*2.0 - 1.0);
				if (abs(IN.tz.x) > 0.1)
				{
					float w = smoothstep(0.1,0.7,abs(IN.tz.x));
					result.xyz += nx*w;
					result.w += w;
				}
				float3 ny = (tex2D(_NormalMap,world.xz*0.1 + float2(0.0,time)*0.1).xyz*2.0 - 1.0)
								+ (tex2D(_NormalMap,world.xz*0.03 + float2(0.0,time)*0.1).xyz*2.0 - 1.0);
				if (abs(IN.tz.y) > 0.1)
				{
					float w = smoothstep(0.1,0.7,abs(IN.tz.y));
					result.xyz += ny*w;
					result.w += w;
				}
				float3 nz = ((tex2D(_NormalMap,world.xy*0.03 + float2(-time,-time)*0.005).xyz*2.0 - 1.0)
							+ (tex2D(_NormalMap,world.xy*0.1 + float2(time,-time)*0.005).xyz*2.0 - 1.0)) * 0.5;
				if (result.w < 1.0)
				{
					float w = 1.0 - result.w;
					result.xyz += nz*w;
					result.w += w;
				
				}
						
				result.xyz /= result.w;
					
				result.xy *= 3.0;
				
				//result = float4(0.0,0.0,1.0,1.0);
				float3 normal = normalize(IN.tx * result.x + IN.ty * result.y + IN.tz * result.z);
				
				float3 view = normalize(IN.world.xzy - _WorldSpaceCameraPos);
				
				float variance = min(dot(normal.xy,normal.xy)*1000.0,10.0);
				float3 reflected = reflect(view,normal.xzy);
				
				float fresnel = 0.05 + 0.95 * (sqr(1.0-abs(dot(normal.xzy,view))));
				float4 reflection_sample = float4(0.0);
				float reflection_strength = 0.0;
								
				{
					float3 dir = reflected;
					float4 s = texCUBE(SkyMap,dir);
					float strength = fresnel;
					reflection_sample += s*strength;
					reflection_strength += strength;
				}
				
				// float	specular_base = max(dot(reflected,gl_LightSource[0].position.xyz),0.0);
				// float	specular = pow(specular_base,80.0+40.0*variance)*(0.7+0.5*variance)*2.0 * fresnel * 10.0;
				o.Specular = (80.0+40.0*variance) / 128.0;



				o.Gloss = (0.7+0.5*variance)*2.0 * fresnel * 10.0;

				o.Albedo = float3(0.35,0.37,0.4)*1.5*(1.0-reflection_strength);
				o.Alpha = sqr(min(1.0,(0.3+0.9*reflection_strength)*1.5));
				o.Emission = reflection_sample.xyz /*+ sqr(sqr(fresnel))*/;
				o.Alpha *= smoothstep(-100.0,-50.0,height);
				//o.Alpha = 1.0;
				o.Normal = normal;
				o.Normal.y *= IN.world2.z;
				
	
			}
			
	
	
	
			ENDCG
	 
	}
	//FallBack "Diffuse"
}
