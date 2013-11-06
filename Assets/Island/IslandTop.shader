Shader "Custom/IslandTop" {
	Properties {
	}
	SubShader {
		tags {
		//"queue"="alphatest" 
		//"ignoreprojector"="true" 
		"rendertype"="opaque"
		}

		LOD 200
		
			
			CGPROGRAM
			#pragma surface surf BlinnPhong addshadow fullforwardshadows vertex:vert
			#pragma glsl
			#pragma target 3.0
			sampler2D _UpperHeightMap, _LowerHeightMap, _WaterHeightMap, _NormalMap;
			float4	Region;
			
			
			struct Input {
				float3 coords;
				float //thickness,
						waterDepth;
			};
			
			float SampleHeight(sampler2D map, float2 uv)
			{
				float4 color;
			//	#if !defined(SHADER_API_OPENGL)
					color = tex2Dlod(map,float4(uv,0.0,0.0));
			//	#endif
				//float4 color = tex2D(map,uv);
				float height = color.r + color.g * 255.0 + color.b * 65535.0;// + color.a * 16777215.0		
				return height - 1000.0;
			}
			
			
			

			
			void vert (inout appdata_full v, out Input o)
			{
				float4 world = mul(_Object2World,v.vertex);
				float2 uv = (world.xz - Region.xy) / (Region.zw - Region.xy);
				float 	h0 = SampleHeight(_UpperHeightMap,uv),
						h1 = SampleHeight(_LowerHeightMap, uv),
						h2 = 0.0;//SampleHeight(_WaterHeightMap, uv);
				//cos(world.x*0.25) * cos(world.z*0.25) * 2.0;
				// v.normal = mul(_World2Object,float4(0.0,1.0,0.0,0.0)).xyz;
				v.normal = float3(0.0,0.0,1.0) * _Object2World[1].z;
				// float3(0.0,0.0,1.0) / _Object2World[1].z;
				// v.tangent = float4(mul(_World2Object,float4(1.0,0.0,0.0,0.0)).xyz,1.0);
				float3 t = mul(_World2Object,float4(1.0,0.0,0.0,0.0)).xyz;
				float tl = length(t);
				v.tangent = float4(t / tl / tl,1.0);

				
				
				float w = (h2 - h0) / 5.0;
				float water_depth = h2 - h0;
				float3 n = tex2Dlod(_NormalMap,float4(uv,0.0,0.0)).xyz * 2.0 - 1.0;
				float thickness = h0 - h1;
				float edge = clamp(1.0 - thickness/50.0,0.0,1.0);
				world.xz += n.xy * ((1.0 - pow(edge,12.0))-0.7) * edge;
				world.y = h0;
				// if (w > 0.9)
				// {
					// world.y = min(world.z,h2-0.1);
					// thickness = world.z - h1;
				// }
				v.vertex = mul(_World2Object,world);

				UNITY_INITIALIZE_OUTPUT(Input,o);
				o.coords = float3(uv,thickness);
				//o.thickness = thickness;
				o.waterDepth = water_depth;
				
				
				
				
			}

			void surf (Input IN, inout SurfaceOutput o) {
				//half4 c = tex2D (_MainTex, IN.uv_MainTex);
				// o.Albedo = float3(sin(IN.coords*10.0)*0.5 + 0.5,0.0);
				clip(IN.coords.z);
				//float3 up = float3(_Object2World[0].z,_Object2World[1].z,_Object2World[2].z);
				// o.Albedo = _Object2World[2].xzy * 0.5 + 0.5;
				float3 n = tex2D(_NormalMap,IN.coords.xy).xyz * 2.0 - 1.0;
				
				//float3 n2 = mul(_Object2World,
				o.Albedo = (float3)(1.0);
				// o.Albedo = n * 0.5 + 0.5;
				//float3(sin(IN.coords.xy*10.0)*0.5 + 0.5,0.0);
				//c.rgb;
				
				o.Normal = n;
				//float3(1.0,0.0,0.0);
				o.Alpha = 1.0;
				
				
				/*
							float g = min(1.0,delta*100.0);
			float r = 1.0;//fmod(s.x,0.5) * 2.0;
			float4 rock = (tex2D(_RockColor,world) + tex2D(_RockColor,world/3.14159)) * 0.5;
			float4 sand = (tex2D(_SandColor,world) + tex2D(_SandColor,world/3.14159)) * 0.5;
			float4 grass = (tex2D(_GrassColor,world) + tex2D(_GrassColor,world/3.14159)) * 0.5;
			float4 forest = (tex2D(_ForestColor,world) + tex2D(_ForestColor,world/3.14159)) * 0.5;
			
			
			float transfer_to_lower = 1.0 - smoothstep(0.0,5.0,delta);
			//float water = tex2D(water_map,gl_TexCoord[0].xy).x * (1.0 - transfer_to_lower)  / 5.0;
			float water = 0.0;

			float sand_a = smoothstep(-0.2,0.2,-0.2 + 1.4*(smoothstep(0.0,0.2,water) * smoothstep(0.3,0.6,normal.z)- sand.a));
			float plant = (1.0 - smoothstep(0.0,0.2,water));
			
			float tree_coverage = 0.0;//tex2D(tree_cover_map,gl_TexCoord[0].xy).x;
			float grass_a =  smoothstep(-0.2,0.2,-0.2 + 1.4*(smoothstep(0.2,0.8,normal.z) * plant - grass.a));
			float forest_a =  smoothstep(-0.5,0.5,-0.2 + 1.4*(tree_coverage - forest.a));
			
			float3 color = rock.rgb;
			color = lerp(color,sand.rgb,sand_a);
			color = lerp(color,grass.rgb,grass_a);
			color = lerp(color,forest.rgb,forest_a);
			
			float3 diffuse = (float3)(1.0);
			
			diffuse *= 0.25 + 0.75 * (1.0 - tree_coverage*tree_coverage);
			
			float water_depth = 0.0;
			
			float3 waterColor = pow(float3(0.4,0.55,0.75),max(0.0,water_depth)* (1.0 - transfer_to_lower) * 0.5);
			diffuse *= waterColor;

			o.Albedo = diffuse * color;
*/
			}
			ENDCG
	 
	}
	//FallBack "Diffuse"
}
