Shader "LavaShader/LavaBlend" 
{
	Properties {
		[NoScaleOffset] _MainTex ("Stone Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MetallicTex ("Stone Metallic/Smooth", 2D) = "white" {}
		[NoScaleOffset] _NormalTex ("Stone Normal", 2D) = "white" {}
		[NoScaleOffset] _LavaAlbedo ("Lava Albedo", 2D) = "white" {}
		[NoScaleOffset] _LavaNRM ("Lava Normal", 2D) = "white" {}
		[NoScaleOffset] _LavaEMIS ("Lava Emission", 2D) = "white" {}
		_LavaHeight ("Height Base",Float) = 0
		_LavaNoise ("Height Noise",Float) = 0
		_LavaNoiseFactor ("Height Noise Power",Range(0.01,2)) = 0.15
		_LavaBlend ("Blend",Float) = 1
		_Tiling ("Tiling (X,Y,Z)",Vector) = (0.05,0.05,0.05,0)
	}
	CGINCLUDE
	#pragma target 3.0

	sampler2D _MainTex;
	sampler2D _MetallicTex;
	sampler2D _NormalTex;

	sampler2D _LavaAlbedo;
	sampler2D _LavaNRM;
	sampler2D _LavaEMIS;

	fixed _LavaHeight;
	fixed _LavaBlend;
	fixed _LavaNoise;
	fixed _LavaNoiseFactor;
	fixed4 _Tiling;

	struct Input {
		float3 worldPos;
		float3 worldNormal;
		INTERNAL_DATA
	};

	half4 Triplanar(sampler2D tex, float3 coord, float3 worldNrm, float3 projNrm) {
		half4 x = tex2D(tex, coord.zy) * abs(worldNrm.x);
		half4 y = tex2D(tex, coord.zx) * abs(worldNrm.y);
		half4 z = tex2D(tex, coord.xy) * abs(worldNrm.z);
		return lerp(lerp(z, x, projNrm.x), y, projNrm.y);
	}

	half4 TriplanarMetallic(sampler2D tex, float3 coord, float3 worldNrm, float3 projNrm) {
		return normalize(saturate(Triplanar(tex, coord, worldNrm, projNrm)));
	}

	half3 TriplanarNRM(sampler2D tex, float3 coord, float3 worldNrm, float3 projNrm) {
		half3 x = UnpackNormal(tex2D(tex, coord.zy) * abs(worldNrm.x));
		half3 y = UnpackNormal(tex2D(tex, coord.zx) * abs(worldNrm.y));
		half3 z = UnpackNormal(tex2D(tex, coord.xy) * abs(worldNrm.z));
		return normalize(lerp(lerp(z, x, projNrm.x), y, projNrm.y).rgb);
	}

	float hash(float n)
	{
		return frac(sin(n)*43758.5453);
	}

	float noise(float3 x)
	{
		// The noise function returns a value in the range -1.0f -> 1.0f

		float3 p = floor(x);
		float3 f = frac(x);

		f = f*f*(3.0 - 2.0*f);
		float n = p.x + p.y*57.0 + 113.0*p.z;

		return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
			lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
			lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
				lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
	}

	void surf(Input IN, inout SurfaceOutputStandard o){
		float3 worldNrm = WorldNormalVector(IN, float3(0, 0, 1));
		float3 projNrm = saturate(pow(worldNrm * 1.4, 4));
		float3 worldPos = _Tiling * IN.worldPos;
		float noisef = noise(IN.worldPos * _LavaNoiseFactor);
		float blend = pow(saturate((_LavaHeight - IN.worldPos.y + noisef * _LavaNoise)/_LavaBlend * noisef),2);
		
		o.Alpha = blend;
		o.Albedo = lerp(Triplanar(_MainTex, worldPos, worldNrm, projNrm), Triplanar(_LavaAlbedo, worldPos, worldNrm, projNrm), blend);
		o.Normal = lerp(TriplanarNRM(_NormalTex, worldPos, worldNrm, projNrm), TriplanarNRM(_LavaNRM, worldPos, worldNrm, projNrm), blend);
		half4 metallic = lerp(TriplanarMetallic(_MetallicTex, worldPos, worldNrm, projNrm), half4(0.1, 0.1, 0.1, 0.3), blend);
		o.Metallic = metallic.r;
		o.Smoothness = metallic.a;
		o.Emission = lerp(fixed3(0,0,0),Triplanar(_LavaEMIS, worldPos, worldNrm, projNrm), blend) * 5;
	}
	ENDCG
		SubShader{
			Tags { "RenderType" = "Opaque" "VR" = "Opaque" }
			LOD 200
			
			CGPROGRAM
			#pragma surface surf Standard fullforwardshadows keepalpha
			#include "UnityCG.cginc"
		ENDCG
	} 
	FallBack "Diffuse"
}
