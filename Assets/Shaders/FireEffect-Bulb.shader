// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/EffectShaderRemap-Bulb"
{
	Properties
	{
		_ScrollPrimary ("Primary Scroll Texture", 2D) = "white" {}
		_ScrollPrimaryColorMult ("Primary Scroll Color Strength", Range(0, 1)) = 1
		_ScrollPrimaryAlphaMult ("Primary Scroll Alpha Strength", Range(0, 1)) = 1

		_PalTex ("Palette Texture", 2D) = "white" {}
		_HDRMultR ("HDR Compensation R", Float) = 1.0
		_HDRMultG ("HDR Compensation G", Float) = 1.0
		_HDRMultB ("HDR Compensation B", Float) = 1.0
	}

	Category
	{
		Tags
		{
			"Queue"="Transparent+3"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off Lighting Off ZWrite Off

		SubShader
		{
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
			
				#include "UnityCG.cginc"

				sampler2D _ScrollPrimary;
				float _ScrollPrimaryColorMult;
				float _ScrollPrimaryAlphaMult;

				sampler2D _PalTex;

				float4 _ScrollPrimary_ST;
				float4 _PalTex_ST;

				float _HDRMultR;
				float _HDRMultG;
				float _HDRMultB;

				struct appdata_t
				{
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
					fixed4 color : COLOR;
				};

				struct v2f
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};
			

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord,_ScrollPrimary);
					o.color = v.color;
					return o;
				}

				fixed4 frag (v2f i) : COLOR
				{
					// texture sample
					float4 primary = tex2D(_ScrollPrimary, i.texcoord);
					primary.rgb *= _ScrollPrimaryColorMult;

					// Store the original texture gray value as the uv to lookup into the remap texture
					float2 remapColor = float2 (primary.r, i.color.r);

					// Lookup final pixel color in shader
					float3 resultColor = tex2D(_PalTex, remapColor).rgb;
					float4 ret = float4(resultColor * float3(_HDRMultR, _HDRMultG, _HDRMultB), primary.a * i.color.a);

					return ret;
				}
				ENDCG 
			}
		}	
	}
}
