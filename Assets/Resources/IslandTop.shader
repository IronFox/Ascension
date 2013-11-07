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

			float VertexHeight(float h0, float h1, float h2)
			{
				return h0;
			}
			
			sampler2D GrassColor, RockColor, SandColor, ForestColor;
			
			void FinishFragment(float2 world, float delta, float3 normal, inout SurfaceOutput o)
			{
				float g = min(1.0,delta*100.0);
				float r = 1.0;//fmod(s.x,0.5) * 2.0;
				float4 rock = (tex2D(RockColor,world) + tex2D(RockColor,world/3.14159)) * 0.5;
				float4 sand = (tex2D(SandColor,world) + tex2D(SandColor,world/3.14159)) * 0.5;
				float4 grass = (tex2D(GrassColor,world) + tex2D(GrassColor,world/3.14159)) * 0.5;
				float4 forest = (tex2D(ForestColor,world) + tex2D(ForestColor,world/3.14159)) * 0.5;
				
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
				//o.Albedo.rg = sin(world) * 0.5 + 0.5;
			}
			
			#include "islandTerrain.cginc"
			ENDCG
	 
	}
	//FallBack "Diffuse"
}
