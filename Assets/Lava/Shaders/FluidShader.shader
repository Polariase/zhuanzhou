Shader "Fluid/FluidShader" {
	Properties {
		_FluidLiquidTex ("Fluid Liquid(RGB)", 2D) = "grey" {}
		_FluidSolidTex ("Fluid Foam(RGB)", 2D) = "grey" {}
		_FluidNrm ("Fluid Normal(RGB)", 2D) = "normal" {}
		//_FluidMaskTex ("Fluid Mask(R)",2D) = "grey" {}
		_Tiling ("Tiling", Vector) = (0.25,0.5,1,0)
		_Intensity("Emission Intensity", Range(0,10)) = 1
		_Speed("Speed",Float) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows keepalpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _FluidLiquidTex;
		sampler2D _FluidSolidTex;
		sampler2D _FluidNrm;
		//sampler2D _FluidMaskTex;
		fixed _Intensity;
		fixed4 _Tiling;
		fixed _Speed;

		struct Input
		{
			float3 worldPos;
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			half2 timer = frac(half2(_Time.y * _Speed,0.5f + _Time.y * _Speed));
			half interp = 2 * abs(0.5 - timer.x);
			half4 flow = half2(0.5f - IN.color.r, 0.5f - IN.color.g).xyxy * timer.xxyy * IN.color.b;

			half2 tiling_lava = IN.worldPos.xz * _Tiling.x;
			half2 tiling_solid = IN.worldPos.xz * _Tiling.y;
			half2 tiling_nrm = IN.worldPos.xz * _Tiling.z;
			//half2 tiling_mask = IN.worldPos.xz * _Tiling.w;
			// Albedo comes from a texture tinted by color
			half4 liquid = lerp(tex2D(_FluidLiquidTex,tiling_lava + flow.xy),tex2D(_FluidLiquidTex,tiling_lava + flow.zw),interp) * IN.color.b * IN.color.a;
			half4 solid = lerp(tex2D(_FluidSolidTex,tiling_solid + flow.xy),tex2D(_FluidSolidTex,tiling_solid + flow.zw),interp);
			half4 nrm = lerp(tex2D(_FluidNrm,tiling_nrm + flow.xy),tex2D(_FluidNrm,tiling_nrm + flow.zw),interp);
			//half mask = lerp(tex2D(_FluidMaskTex,tiling_mask +flow.xy).r,tex2D(_FluidMaskTex,tiling_mask +flow.zw).r,interp);

			o.Albedo = solid + liquid;
			o.Emission = (pow(o.Albedo,1.4) + liquid * 0.5f) * _Intensity;
			o.Metallic = 0;
			o.Normal = UnpackNormal(nrm);
			o.Smoothness = 0;
			o.Alpha = IN.color.r;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
