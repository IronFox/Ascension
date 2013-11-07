#ifndef islandTerrainCGINC
#define islandTerrainCGINC

			
	sampler2D _UpperHeightMap, _LowerHeightMap, _WaterHeightMap, _NormalMap;
	float4	Region;
	
	
	struct Input {
		float2 world;
		float3 coords;
		float //thickness,
				waterDepth;
	};
	
	float SampleHeight(sampler2D map, float2 uv)
	{
		float4 color;
		color = tex2Dlod(map,float4(uv,0.0,0.0));
		color = color * 255.0 / 256.0;
		//float4 color = tex2D(map,uv);
		float height = color.r + color.g * 256.0 + color.b * 256.0*256.0;// + color.a * 16777215.0		
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
		world.y = VertexHeight(h0,h1,h2);
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
		o.world = world.xz;
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
		
		FinishFragment(IN.world,IN.coords.z,n, o);
	}
	
	
	
#endif