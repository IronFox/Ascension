#ifndef islandCGINC
#define islandCGINC

		
		float UnityCalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess)
		{
			float3 wpos = mul(_Object2World,vertex).xyz;
			float dist = distance (wpos, _WorldSpaceCameraPos);
			return tess / (1.0 + dist*0.05);
			//float f = clamp(sqr(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0)) * tess;
			//return f;
		}
		
		float4 UnityCalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

	float4 UnityDistanceBasedTess (float4 v0, float4 v1, float4 v2, float minDist, float maxDist, float tess)
	{
		float3 f;
		f.x = UnityCalcDistanceTessFactor (v0,minDist,maxDist,tess);
		f.y = UnityCalcDistanceTessFactor (v1,minDist,maxDist,tess);
		f.z = UnityCalcDistanceTessFactor (v2,minDist,maxDist,tess);

		return UnityCalcTriEdgeTessFactors (f);
	}
	




	
		struct appdata {
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
		};	
	//	const float _Tess = 8.0;

		
	
		
		float4 tessDistance (appdata v0, appdata v1, appdata v2)
		{
			float minDist = 10.0;
			float maxDist = 100.0;
			return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, 16.0);
		}		
		
		
		
		sampler2D _UpperHeightMap,_LowerHeightMap,_NormalMap;

		struct Input {
			float2 uv_UpperHeightMap;

			float3 worldPos;
			float3 worldNormal;
		};
		


		float SampleHeight(sampler2D map, float2 uv)
		{
			float4 color = tex2Dlod(map,float4(uv,0.0,0.0));
			float height = color.r + color.g * 255.0 + color.b * 65535.0;// + color.a * 16777215.0		
			return height - 1000.0;
		}

		
	
		void PrepareDisplacement(inout appdata v)
		{
			float2 uv = v.texcoord.xy;
			v.texcoord.x = SampleHeight(_UpperHeightMap,uv);
			v.texcoord.y = SampleHeight(_LowerHeightMap,uv);
		}

		void surf (Input IN, inout SurfaceOutput o) {
			float delta = IN.uv_UpperHeightMap.x - IN.uv_UpperHeightMap.y;
			clip(delta+0.1);
			float3 object = mul(_World2Object,float4(IN.worldPos,1.0)).xyz;
			float2 uv = 1.0 - ((object.xz/0.25) / 2048.0 + 0.5);
			uv.y = 1.0 - uv.y;
			float3 n = tex2D(_NormalMap,uv).xyz * 2.0 - 1.0;
			o.Normal = n;
			o.Albedo = float3(1.0,1.0,1.0);
			// o.Albedo = float3(uv,0.0);
			//o.Albedo = float3(abs(uv - IN.uv_UpperHeightMap),0.0);
			o.Alpha = 1.0;
			
			Finalize(object.xz,delta,n,o);
		}


#endif
