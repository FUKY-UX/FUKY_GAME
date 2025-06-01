#ifndef SIMPLENPR_SHADOW_PASS_INCLUDED
#define SIMPLENPR_SHADOW_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

// #include "TSCharacterLitInput.hlsl"
// #include "TSCharacterLightingCore.hlsl"

// Shadow Casting Light geometric parameters. These variables are used when applying the shadow Normal Bias and are set by UnityEngine.Rendering.Universal.ShadowUtils.SetupShadowCasterConstantBuffer in com.unity.render-pipelines.universal/Runtime/ShadowUtils.cs
// For Directional lights, _LightDirection is used when applying shadow Normal Bias.
// For Spot lights and Point lights, _LightPosition is used to compute the actual light direction because it is different at each shadow caster geometry vertex.
float3 _LightDirection;
float3 _LightPosition;

float4 _CustomShadowBias;
float4 _CustomShadowLightDir;

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS      : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 uv2          : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv           : TEXCOORD0;
    float4 positionCS   : SV_POSITION;
    #if defined(_ENABLEDISOLVE_ON)
    half3 normalWS     : TEXCOORD1;
    float3 positionWS   : TEXCOORD2;
    float2 uv2         : TEXCOORD3;
    #endif
};

/*	应用自定义阴影偏移	*/
float3 ApplyCustomShadowBias(float3 worldPos, float3 worldNormal)
{
    float invNdotL = 1.0 - saturate(dot(_CustomShadowLightDir.rgb, worldNormal));

    worldPos += _CustomShadowLightDir.rgb * _CustomShadowBias.y;
    worldPos -= worldNormal * invNdotL * _CustomShadowBias.x;
    return worldPos;
}

float4 GetShadowPositionHClip(Attributes input)
{
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

#if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
#else
    float3 lightDirectionWS = _LightDirection;
#endif

    // float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
    float4 positionCS = TransformWorldToHClip(ApplyCustomShadowBias(positionWS, normalWS));
    
#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#endif

    return positionCS;
}

Varyings ShadowPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);

    output.uv = TRANSFORM_TEX(input.texcoord,_MainTex);
    output.positionCS = GetShadowPositionHClip(input);

    #if defined(_ENABLEDISOLVE_ON)
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.positionWS = positionWS;
    output.normalWS = normalWS;
    output.uv2 = input.uv2;
    #endif
    
    return output;
}

half4 ShadowPassFragment(Varyings input) : SV_TARGET
{
    #if _ALPHA_CLIP_ON
    clip(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv).a-_Cutoff);
    #endif
    
    return 0;
}

#endif