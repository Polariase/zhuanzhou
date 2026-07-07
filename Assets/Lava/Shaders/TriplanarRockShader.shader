Shader "LavaShader/TriplanarRock" 
{
	Properties {
		[NoScaleOffset] _MainTex ("Stone Albedo (RGB)", 2D) = "gray" {}
		[NoScaleOffset] _MetallicTex ("Stone Metallic/Smooth", 2D) = "black" {}
		[NoScaleOffset] _NormalTex ("Stone Normal", 2D) = "normal" {}
		_Tiling ("Tiling (X,Y,Z)",Vector) = (0.05,0.05,0.05,0)
	}
	CGINCLUDE
	#pragma target 3.0

	sampler2D _MainTex;
	sampler2D _MetallicTex;
	sampler2D _NormalTex;

	fixed3 _Tiling;

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

	void surf(Input IN, inout SurfaceOutputStandard o){
		float3 worldNrm = WorldNormalVector(IN, float3(0, 0, 1));
		float3 projNrm = saturate(pow(worldNrm * 1.4, 4));
		float3 worldPos = _Tiling * IN.worldPos;

		o.Albedo = Triplanar(_MainTex, worldPos, worldNrm, projNrm);
		o.Normal = TriplanarNRM(_NormalTex, worldPos, worldNrm, projNrm);
		half4 metallic = Triplanar(_MetallicTex, worldPos, worldNrm, projNrm);
		o.Metallic = metallic.r;
		o.Smoothness = metallic.a;
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
