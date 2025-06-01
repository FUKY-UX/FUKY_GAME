#ifndef SIMPLENPR_INPUT
#define SIMPLENPR_INPUT
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

CBUFFER_START(UnityPerMaterial)
// float4 _BaseMap_ST;
// half4 _BaseColor;
half _Cutoff;

half4 _MainColor;
float4 _MainTex_ST;
// half4 _ShadowColor;
// half4 _TilingAndOffset;
//half _RampIntensity;
//half _RampIndex;
//float2 _ShadowSmooth;
half _ShadowGain;

half _NormalScale;
float4 _NormalMap_ST;
half3 _SpecularColor;
half _Specular1,_AddSpecular,_SpecularInShadow,_SpecularStep,_SpecularDiscolor;
half3 _ReflectionCol;
float4 _ReflectionSetting;
float4 _RimCol;
float _Discolor,_RimWidth,_Threshold;
half _RimLightInShadow;
//half _ReflectionIntensity;
float4 _TransmissionCol;
float _TransmissionSetting,_TransmissionUseShadow;

float4 _EmissiveTex_ST;
half3 _EmissiveColor;
//float2 _LambertSmooth;

float4 _OcclusionTex_ST;
half _OcclusionMult;

half _AOInBaseColor;
half4 _AOColor;
            
half _HalftoneEffect;
half _HalftoneStep;
half3 _HalftoneColor;
half _HalftoneScale;
half _HalftoneUVZScale;
half4 _EdgePowScale;

half _UseVertexColorRGB;
half _UseVertexColorA;

//outline
int _IsTangentSpace;
half4 _OutlineCol;
half _OutlineColInBaseColor;
half _OutlineWidth;
half _ViewSpaceZOffset;

CBUFFER_END

TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
TEXTURE2D(_ShadowColorTex); SAMPLER(sampler_ShadowColorTex);
TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
TEXTURE2D(_EmissiveTex); SAMPLER(sampler_EmissiveTex);
TEXTURE2D(_OcclusionTex); SAMPLER(sampler_OcclusionTex);
TEXTURE2D(_DiffuseRampMap);
//SAMPLER(sampler_LinearClamp);
#endif