Shader "Reflective/Bumped Specular Custom with AO" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
	_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
	_MainTex ("Base (RGB) RefStrGloss (A)", 2D) = "white" {}
	_AOTex ("AO", 2D) = "white" {}
	_Cube ("Reflection Cubemap", Cube) = "" { TexGen CubeReflect }
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_SpecMap ("Specular map", 2D) = "white" {}

	_Intensity1 ("Intensity of diffuse", Float ) = 1
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 400
CGPROGRAM
#pragma surface surf BlinnPhong
#pragma target 3.0
#pragma glsl
//input limit (8) exceeded, shader uses 9
#pragma exclude_renderers d3d11_9x

sampler2D _MainTex;
sampler2D _AOTex;
sampler2D _BumpMap;
sampler2D _SpecMap;

samplerCUBE _Cube;

half _Intensity1;

fixed4 _Color;
fixed4 _ReflectColor;
half _Shininess;

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
	float2 uv_SpecMap;

	float3 worldRefl;
	INTERNAL_DATA
};

float3 overlayColor(float3 colorA, float4 colorB){
            return (colorA * (1 - colorB.a)) + (colorB.rgb * colorB.a);
        }

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 aotex = tex2D(_AOTex, IN.uv_MainTex);
	fixed4 c = tex * _Color;
	fixed4 specTex = tex2D(_SpecMap, IN.uv_SpecMap);

	

	
	o.Albedo = (c.rgb * _Intensity1) * aotex.rgb;
	
	o.Gloss = tex.a * specTex.g;
	o.Specular = _Shininess;
	
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
	
	float3 worldRefl = WorldReflectionVector (IN, o.Normal);
	tex.a = 0.4;
	float3 lerped = lerp(worldRefl.rgb ,tex.rgb,tex.a) ;
	

	fixed4 reflcol = texCUBE (_Cube, lerped);
	//reflcol *= tex.a;
	//reflcol *= tex.a;
	o.Emission = reflcol.rgb * _ReflectColor.rgb * specTex.g * aotex.rgb;
	o.Alpha = reflcol.a * _ReflectColor.a;
}
ENDCG
}

FallBack "Reflective/Bumped Diffuse"
}
