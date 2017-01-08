 Shader "Grail/Standard_ForGame" {
     Properties {
		 _Color ("Main Color", Color) = (1,1,1,1)

         _MainTex ("Albedo (RGB)", 2D) = "white" {}
         _Glossiness ("Smoothness", Range(0,1)) = 0.5
         _Metallic ("Metallic", Range(0,1)) = 0.0
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_SpecMap ("Specular map", 2D) = "white" {}
	
		_useCC ("Use color correction? (0;1)", Range(0,1)) = 0.0
		_CustomColorMask ("Custom Color Mask", 2D) = "white" { }
		_Desaturation ("Desaturation", Range(0,1)) = 0.0
		_Brightness ("Brightness (_mul)", Range(0,1)) = 1.0
		_AddToClamp ("Linear brightness (_add)", Range(0,1)) = 0.0
		_Contrast ("Final contrast (_con)", Range(0,2)) = 1.0

		_DeathOverlay ("DeathOverlay", 2D) = "white" { }
		_isDead ("Is dead? (0;1)", Range(0,1)) = 0.0
		
		_EyeCloseTex ("ClosedEyesOverlay", 2D) = "white" { }
		_ClosedEyes ("Closed Eyes? (0;1)", Range(0,1)) = 0.0
     }
     SubShader {
         Tags { "RenderType"="Opaque" }
         LOD 200
         
         CGPROGRAM
         #pragma surface surf Standard vertex:vert fullforwardshadows
         #pragma target 3.0
        struct Input {
            float4 vertex : POSITION;
            float2  texcoord : TEXCOORD0;
            float2 texcoord1 : TEXCOORD1;
			float3    viewDir;
			INTERNAL_DATA
        };
 
        struct v2f {
            float4 vertex : POSITION;
            float2  texcoord : TEXCOORD0;
            float2 texcoord1 : TEXCOORD1;
        };
		
		/*inline float3 WorldSpaceViewDir( in float4 v )
		{
			return _WorldSpaceCameraPos.xyz - mul(_Object2World, v).xyz;
		}*/
 
         void vert (inout appdata_full v, out Input o)
         {
             UNITY_INITIALIZE_OUTPUT(Input,o);	
		     o.texcoord = v.texcoord.xy;
			 
         }
 
         sampler2D _MainTex;
		 uniform float4 _MainTex_ST;
         half _Glossiness;
         half _Metallic;
         fixed4 _Color;
		 

		sampler2D _BumpMap;
		sampler2D _SpecMap;
		half _Intensity;
		half _Shininess;


uniform half _Desaturation;
uniform half _Brightness;
uniform half _AddToClamp;
uniform half _Contrast;
		uniform half _useCC;
		uniform half _ClosedEyes;

		sampler2D _CustomColorMask;
uniform half4 _CustomColorMask_ST;

		uniform half _isDead;

sampler2D _DeathOverlay;
sampler2D _EyeCloseTex;
 
 
         void surf (Input IN, inout SurfaceOutputStandard o) 
         {

			
		
			fixed4 tex = tex2D(_MainTex, IN.texcoord);
			//if(_isDead == 1){
		fixed4 dOverl = tex2D(_DeathOverlay, IN.texcoord);
		tex.rgb = lerp(tex.rgb,dOverl.rgb,clamp(dOverl.a*_isDead,0,1));
	//}

	if(_ClosedEyes > 0){
		fixed4 eyeOver = tex2D(_EyeCloseTex, IN.texcoord);
		tex.rgb = lerp(tex.rgb,eyeOver.rgb,clamp(eyeOver.a*_ClosedEyes,0,1));
	}
if(_useCC == 1)
{
			fixed3 m = tex2D(_CustomColorMask, IN.texcoord).rgb;
	float newMax = max(tex.x,max(tex.y,tex.z));
	float newMin = min(tex.x,min(tex.y,tex.z));
	float newCol = (newMax+newMin) / 2.0;
	float3 mixedCol = lerp(tex.rgb,float3(newCol,newCol,newCol),_Desaturation);
	tex.rgb = lerp(tex.rgb,mixedCol,m.z);
	
	tex.rgb *= lerp(_Brightness,1.0,1- m.z);
	tex.rgb += clamp(tex.rgb+_AddToClamp * m.z,0.0,1.0);
	tex.rgb = ((tex.rgb - 0.5f) * max(lerp(_Contrast,1.0,1- m.z), 0)) + 0.5f;

			}
			float4 output = tex * _Color ;
		



			o.Albedo = output.rgb;
             o.Metallic = _Metallic;
             o.Smoothness = _Glossiness;
			o.Alpha = tex.a * _Color.a;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.texcoord));		 
         }
         ENDCG
     } 
     FallBack "Diffuse"
 }