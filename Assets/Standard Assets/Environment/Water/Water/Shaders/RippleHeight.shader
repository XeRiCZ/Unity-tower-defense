    Shader "FX/RippleHeight" {
    Properties {
        _WaterLevel ("WaterLevel", Float) = 0
		_heightMultiplier ("Height multiplier", Float) = 3
        _WaterColor ("Ripple color", Color) = (1,1,1,1)
		_MainTex ("Plants texture", 2D) = "white" {}
		_MainColor ("Plants color", Color) = (1,1,1,1)
		_NormalTex ("Plants normal", 2D) = "white" {}
		_PlantsDistribution ("Plants distribution mask", 2D) = "white" {}
    }
    SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	  Pass {
		Blend SrcAlpha OneMinusSrcAlpha
        CGPROGRAM
        #pragma vertex vert 
		#pragma fragment frag 
		#pragma alpha:blend		

		sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		sampler2D _NormalTex;
		uniform float4 _NormalTex_ST;
		sampler2D _PlantsDistribution;
		uniform float4 _PlantsDistribution_ST;

				
        float _WaterLevel;
        float4 _WaterColor;
		float4 _MainColor;
		float _heightMultiplier;
		
		struct vertexInput {
            float4 vertex : POSITION;
			float2 uv_Main : TEXCOORD0;
         };
		
		struct vertexOutput {
            float4 pos : SV_POSITION;
            float4 position_in_world_space : TEXCOORD1;
			float2 uv_MainTex : TEXCOORD0;
			float2 uv_MaskTex : TEXCOORD2;
         };
 
         vertexOutput vert(vertexInput input) 
         {
            vertexOutput output; 
 
            output.pos =  mul(UNITY_MATRIX_MVP, input.vertex);
            output.position_in_world_space = 
               mul(_Object2World, input.vertex);
               // transformation of input.vertex from object 
               // coordinates to world coordinates;
			output.uv_MainTex = input.uv_Main.xy * _MainTex_ST.xy + _MainTex_ST.zw;
			output.uv_MaskTex = input.uv_Main.xy * _PlantsDistribution_ST.xy + _PlantsDistribution_ST.zw;
            return output;
         }
		


		

		
		 float4 frag(vertexOutput input) : COLOR 
         {
			float4 o;
            o = float4(0,0,0,0); 

			// leaf
			float4 leavesTex = tex2D (_MainTex, input.uv_MainTex);
			float4 detailedLeaves2 = tex2D (_MainTex, input.uv_MainTex / 5.0f);
			leavesTex = leavesTex*clamp(detailedLeaves2.g + 0.2,0,1) * _MainColor;
			float4 maskTex = tex2D (_PlantsDistribution, input.uv_MaskTex);
			float4 lerped = float4(0,0,0,0);
			o = float4(lerp(leavesTex.rgb,leavesTex.rgb,maskTex.a),clamp( (o.a + maskTex.a) * leavesTex.a , 0, 1));
			
			if (input.position_in_world_space.y >= _WaterLevel){
                float alphaFactor = (input.position_in_world_space.y - _WaterLevel) * _heightMultiplier;
				if(alphaFactor > 1) alphaFactor = 1;
				else if (alphaFactor < 0) alphaFactor = 0;
				
				//o = float4(_WaterColor.r,_WaterColor.g,_WaterColor.b,alphaFactor);
				o = lerp(o,_WaterColor,alphaFactor);
				
			}

			return o;
         }
		
        ENDCG
    }
	
    }
	Fallback "Legacy Shaders/Transparent/Diffuse"
	}
	
	