// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "LavaShader/Lava" {
	Properties
	{
		_LavaLiquidTex ("Fluid Liquid(RGB)", 2D) = "grey" {}
		_LavaSolidTex ("Fluid Foam(RGB)", 2D) = "grey" {}
		_LavaNrm("Lava Normal(RGB)", 2D) = "normal" {}
		_Tiling ("Tiling", Vector) = (0.25,0.5,1,0)
		//_Distort ("Border Distortion", Range(0,10)) = 1
		//_Incandescence("Incandescence", Color) = (1,0.2,0,0)
		//_Intensity("Emission Intensity", Range(0,10)) = 1
		//_Depth("Depth",Float) = 0.05
		_Speed("Speed",Float) = 0.5

		//_GerstnerIntensity("Per vertex displacement", Float) = 1.0
		//_GAmplitude("Wave Amplitude", Vector) = (-0.1 ,-0.15,0.05,0.5)
		//_GFrequency("Wave Frequency", Vector) = (0.1, 0.5, 0.25, 0.2)
		//_GSteepness("Wave Steepness", Vector) = (1.0, 1.0, 1.0, 1.0)
		//_GSpeed("Wave Speed", Vector) = (0.5, 1.375, 1.1, 3)
		//_GDirectionAB("Wave Direction", Vector) = (0.62, 0.85,-0.85, 0.25)
		//_GDirectionCD("Wave Direction", Vector) = (0.1, 0.9, 0.5, 0.5)
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard vertex:vert fullforwardshadows
		#pragma target 3.0

		uniform half _GerstnerIntensity;
		uniform float4 _GAmplitude;
		uniform float4 _GFrequency;
		uniform float4 _GSteepness;
		uniform float4 _GSpeed;
		uniform float4 _GDirectionAB;
		uniform float4 _GDirectionCD;

		half3 GerstnerOffset4(half2 xzVtx, half4 steepness, half4 amp, half4 freq, half4 speed, half4 dirAB, half4 dirCD)
		{
			half3 offsets;

			half4 AB = steepness.xxyy * amp.xxyy * dirAB.xyzw;
			half4 CD = steepness.zzww * amp.zzww * dirCD.xyzw;

			half4 dotABCD = freq.xyzw * half4(dot(dirAB.xy, xzVtx), dot(dirAB.zw, xzVtx), dot(dirCD.xy, xzVtx), dot(dirCD.zw, xzVtx));
			half4 TIME = _Time.yyyy * speed;

			half4 COS = cos(dotABCD + TIME);
			half4 SIN = sin(dotABCD + TIME);

			offsets.x = dot(COS, half4(AB.xz, CD.xz));
			offsets.z = dot(COS, half4(AB.yw, CD.yw));
			offsets.y = dot(SIN, amp);

			return offsets;
		}

		half3 GerstnerNormal4(half2 xzVtx, half4 amp, half4 freq, half4 speed, half4 dirAB, half4 dirCD)
		{
			half3 nrml = half3(0, 2.0, 0);

			half4 AB = freq.xxyy * amp.xxyy * dirAB.xyzw;
			half4 CD = freq.zzww * amp.zzww * dirCD.xyzw;

			half4 dotABCD = freq.xyzw * half4(dot(dirAB.xy, xzVtx), dot(dirAB.zw, xzVtx), dot(dirCD.xy, xzVtx), dot(dirCD.zw, xzVtx));
			half4 TIME = _Time.yyyy * speed;

			half4 COS = cos(dotABCD + TIME);

			nrml.x -= dot(COS, half4(AB.xz, CD.xz));
			nrml.z -= dot(COS, half4(AB.yw, CD.yw));

			nrml.xz *= _GerstnerIntensity;
			nrml = normalize(nrml);

			return nrml;
		}

		void Gerstner(out half3 offs, out half3 nrml,
			half3 vtx, half3 tileableVtx,
			half4 amplitude, half4 frequency, half4 steepness,
			half4 speed, half4 directionAB, half4 directionCD)
		{
			offs = GerstnerOffset4(tileableVtx.xz, steepness, amplitude, frequency, speed, directionAB, directionCD);
			nrml = GerstnerNormal4(tileableVtx.xz + offs.xz, amplitude, frequency, speed, directionAB, directionCD);
		}

		//fixed _Depth;
		//sampler2D_float _CameraDepthTexture;
		sampler2D _LavaLiquidTex;
		sampler2D _LavaSolidTex;
		sampler2D _LavaNrm;
		half3 _Tiling;
		//fixed _Distort;
		//fixed _Intensity;
		fixed _Speed;
		//half4 _Incandescence;
	
		struct Input
		{
			float3 worldPos;
			float4 depth;
			float4 color : COLOR;
		};

		void vert(inout appdata_full v, out Input o)
		{
			half3 vtxForAni = mul(unity_ObjectToWorld,(v.vertex)).xzz;

			half3 nrml;
			half3 offsets;
			Gerstner(
				offsets, nrml, v.vertex.xyz, vtxForAni,						// offsets, nrml will be written
				_GAmplitude,												// amplitude
				_GFrequency,												// frequency
				_GSteepness,												// steepness
				_GSpeed,													// speed
				_GDirectionAB,												// direction # 1, 2
				_GDirectionCD												// direction # 3, 4
			);
			//v.vertex.xyz += offsets;
			//v.normal = nrml;
			UNITY_INITIALIZE_OUTPUT(Input, o);
			//o.depth = ComputeScreenPos(mul(UNITY_MATRIX_MVP, v.vertex)); //DepthBase
		}

		half _Flow;
		half _Interp;
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			//half depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.depth));
			//depth = LinearEyeDepth(depth);
			//depth -= IN.depth.z;
			
			//half depthDistort = saturate(1 - _Depth * depth);

			half2 timer = frac(half2(_Time.y * _Speed,0.5f + _Time.y * _Speed));
			half interp = 2 * abs(0.5 - timer.x);
			half4 flow = half2((0.5f - IN.color.r),(0.5f - IN.color.g)).xyxy * timer.xxyy * IN.color.b;

			half2 tiling_lava = IN.worldPos.xz * _Tiling.x;
			half2 tiling_solid = IN.worldPos.xz * _Tiling.y;
			half2 tiling_nrm = IN.worldPos.xz * _Tiling.z;

			half4 liquid = lerp(tex2D(_LavaLiquidTex,tiling_lava + flow.xy),tex2D(_LavaLiquidTex,tiling_lava + flow.zw),interp);
			half4 solid = lerp(tex2D(_LavaSolidTex,tiling_solid + flow.xy),tex2D(_LavaSolidTex,tiling_solid + flow.zw),interp);
			half4 nrm = lerp(tex2D(_LavaNrm,tiling_nrm + flow.xy),tex2D(_LavaNrm,tiling_nrm + flow.zw),interp);

			//o.Alpha = depthDistort;
			//o.Specular = depthDistort;
			//o.Albedo = col_n.rgb * col_n.a + col_d * (1 - col_n.a);
			//o.Emission = pow(o.Albedo, 7) * _Intensity + _Incandescence * max(0.0, depthDistort - 0.95) * 3000;

			//o.Emission = float4((0.5f - IN.color.g)*2,(0.5f - IN.color.r)*2,0,1);
			o.Albedo = lerp(liquid,solid,IN.color.a).rgb;
			//o.Normal = UnpackNormal(nrm);
			o.Metallic = 1;
			o.Smoothness = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
