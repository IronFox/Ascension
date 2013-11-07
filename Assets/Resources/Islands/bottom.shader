Shader "Island/Bottom" {
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

			#include "sampleTerrainHeight.cginc"
			
			sampler2D RockColor, _LowerHeightMap;

			float VertexHeight(float h0, float h1, float h2)
			{
				return h1;
			}
			float GetWaterHeight(float2 uv)
			{
				return -1000.0;
			}
			float GetBottomHeight(float2 uv)
			{
				return SampleHeight(_LowerHeightMap, uv);
			}
			
			
			void FinishFragment(float2 world, float delta, float waterDepth,float3 normal,  inout SurfaceOutput o)
			{
				float4 rock = (tex2D(RockColor,world) + tex2D(RockColor,world/3.14159)) * 0.5;
				o.Albedo = rock.rgb;
			}
			
			#include "terrain.cginc"


			ENDCG
	 
	}
	//FallBack "Diffuse"
}
