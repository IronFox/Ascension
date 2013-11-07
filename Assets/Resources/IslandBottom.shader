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
		
			Cull Front
			
			CGPROGRAM
			#pragma surface surf BlinnPhong addshadow fullforwardshadows vertex:vert
			#pragma glsl
			#pragma target 3.0

			float VertexHeight(float h0, float h1, float h2)
			{
				return h1;
			}
			
			sampler2D RockColor;
			
			void FinishFragment(float2 world, float delta, float3 normal, inout SurfaceOutput o)
			{
				float4 rock = (tex2D(RockColor,world) + tex2D(RockColor,world/3.14159)) * 0.5;
				o.Albedo = rock.rgb;
			}
			
			#include "islandTerrain.cginc"


			ENDCG
	 
	}
	//FallBack "Diffuse"
}
