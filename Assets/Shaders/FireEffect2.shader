// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/FireEffectShader2"
{
	Properties
	{
		_ScrollTex ("Scroll Texture", 2D) = "white" {}
		_HotSpotTex ("HotSpot Texture", 2D) = "black" {}
		_MaskTex("Mask Texture", 2D) = "white" {}
		_PalTex ("Palette Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0

		_TemperatureCoarse ("-- Temperature (Coarse)", Range(0, 1)) = 1
		_TemperatureDetail ("-- Temperature (Detail)", Range(0, 1)) = 1
		_OpacityCoarse ("-- Opacity (Coarse)", Range(0, 1)) = 1
		_OpacityDetail ("-- Opacity (Detail)", Range(0, 1)) = 1

	}

	Category
	{
		Tags
		{
			"Queue"="Transparent"
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
				#pragma multi_compile_particles
			
				#include "UnityCG.cginc"

				sampler2D _ScrollTex;
				float _TemperatureCoarse;
				float _OpacityCoarse;

				sampler2D _HotSpotTex;

				sampler2D _MaskTex;
				float _TemperatureDetail;
				float _OpacityDetail;

				sampler2D _PalTex;

				float4 _ScrollTex_ST;
				float4 _HotSpotTex_ST;
				float4 _MaskTex_ST;
				float4 _PalTex_ST;

				sampler2D _CameraDepthTexture;
				float _InvFade;

				struct appdata_t
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					float2 texcoord1 : TEXCOORD1;
				};

				struct v2f
				{
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
					float2 texcoord1 : TEXCOORD1;
					float2 texcoord2 : TEXCOORD2;
					fixed4 color : COLOR;
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD3;
					#endif
				};
			

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					float4 scroll_ST = _ScrollTex_ST;
					o.texcoord = TRANSFORM_TEX(v.texcoord, scroll);
					o.texcoord1 = TRANSFORM_TEX(v.texcoord1, _HotSpotTex);
					o.texcoord2 = TRANSFORM_TEX(v.texcoord, _MaskTex);
					#ifdef SOFTPARTICLES_ON
					o.projPos = ComputeScreenPos (o.vertex);
					COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					return o;
				}

				fixed4 frag (v2f i) : COLOR
				{
					// texture sample
					float4 primary = tex2D(_ScrollTex, i.texcoord) + tex2D(_HotSpotTex, i.texcoord1);
					primary.rgb *= _TemperatureCoarse;
					primary.a *= _OpacityCoarse;

					float4 mask = tex2D(_MaskTex, i.texcoord2);
					mask.rgb *= _TemperatureDetail;
					mask.a *= _OpacityDetail;

					// Store the original texture gray value as the uv to lookup into the remap texture
					float2 remapColor = float2 (primary.r, mask.r);
					float2 remapAlpha = float2 (primary.a, mask.a);

					// Lookup final pixel color in shader
					float3 resultColor = tex2D(_PalTex, remapColor).rgb * i.color.rgb;
					float resultAlpha = tex2D(_PalTex, remapAlpha).a * i.color.a;

					#ifdef SOFTPARTICLES_ON
					float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
					float partZ = i.projPos.z;
					float fade = saturate (_InvFade * (sceneZ-partZ));
					resultAlpha *= fade;
					#endif

					float4 ret = float4(resultColor, resultAlpha);

					return ret;
				}
				ENDCG 
			}
		}	
	}
	CustomEditor "FireEffectInspector"
}
