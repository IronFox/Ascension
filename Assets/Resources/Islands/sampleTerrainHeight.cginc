#ifndef sampleTerrainHeightCGINC
#define sampleTerrainHeightCGINC

	float SampleHeight(sampler2D map, float2 uv)
	{
		float4 color;
		color = tex2Dlod(map,float4(uv,0.0,0.0));
		color = color * 255.0 / 256.0;
		//float4 color = tex2D(map,uv);
		float height = color.r + color.g * 256.0 + color.b * 256.0*256.0;// + color.a * 16777215.0		
		return height - 1000.0;
	}

	
#endif
