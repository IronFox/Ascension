Shader "Custom/Lower Island" {
	Properties {
		_UpperHeightMap ("Encoded Upper Heightmap", 2D) = "white" {}
		_LowerHeightMap ("Encoded Lower Heightmap", 2D) = "white" {}
		_NormalMap ("Lower Normal Map",2D) = "white" {}
		_RockColor ("Rock Colormap",2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 300
		
		CGPROGRAM
		//#pragma surface surf Lambert vertex:vert
            #pragma surface surf BlinnPhong addshadow fullforwardshadows vertex:disp tessellate:tessDistance nolightmap
            #pragma target 5.0

		sampler2D _RockColor;

		void Finalize(float2 world, float delta, float3 normal, inout SurfaceOutput o)
		{
			float4 rock = (tex2D(_RockColor,world) + tex2D(_RockColor,world/3.14159)) * 0.5;
			o.Albedo = rock.rgb;
		}


		#include "island.cginc"

	
		
	    void disp (inout appdata v)
		{
			PrepareDisplacement(v);
			v.vertex.y = v.texcoord.y;
		}	
		

		
		
		ENDCG
	} 
	FallBack "Diffuse"
}
