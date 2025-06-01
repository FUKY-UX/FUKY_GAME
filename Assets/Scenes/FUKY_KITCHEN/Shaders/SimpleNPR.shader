Shader "ToonLit/SceneToonLit"
{
    Properties
    {
//    	[Main(Group20, _, off, off)] 
//    	_group20 ("自定义方向光(TODO)", float) = 0
//    	[SubToggle(Group20)]   _ViewSpace ("跟随视角",float) = 0
//    	[Sub(Group20)] _CustomDir("主光方向",Vector) = (0,0,0,0)
    	
        [Main(Group1, _, off, off)] 
		_group1 ("主帖图设置", float) = 0
        [Sub(Group1)]   [HDR]_MainColor ("主颜色",Color) = (1,1,1,1)
        [Sub(Group1)]   _MainTex ("主贴图", 2D) = "white" {}
        [SubToggle(Group1,_ALPHA_CLIP_ON)] _AlphaClip("AlphaClip",float) = 0
        [Sub(Group1)]   _Cutoff("裁切",Range(-0.01,1.001)) = 0
        
        [Main(Group5, _, off, off)] 
		_group5 ("阴影设置", float) = 0
        [Sub(Group5)]   [NoScaleOffset]_DiffuseRampMap("RampTex",2d) = "white" {}
        [Sub(Group5)]   _ShadowGain("阴影增益",Range(-1,1)) = 0
        
        [Main(Group2, _NORMALMAP,off , on)] 
		_group2 ("法线贴图", float) = 0
        [Sub(Group2)]   _NormalScale("法线强度",float) = 1
        [Sub(Group2)]   _NormalMap("法线贴图",2D) = "bump"{}
        
        [Main(Group3, _, off, off)] 
		_group3 ("高光设置", float) = 0
        [Sub(Group3)]   [HDR]_SpecularColor("高光颜色",Color) = (1,1,1,1)
    	[Sub(Group3)]   _SpecularDiscolor("去色",Range(0,1)) = 0
        [Sub(Group3)]   _Specular1("高光1",Range(0,1)) = 0
        [Sub(Group3)]   _AddSpecular("附加光高光",Range(0,1)) = 0
        [Sub(Group3)]   _SpecularInShadow("暗面高光强度",Range(0,1)) = 0.1
        [SubToggle(Group3)]   _SpecularStep("硬高光",float) = 0
        
        [Main(RimLight, _, off, off)] 
        _rimLight ("深度边缘光设置", float) = 0
        [Sub(RimLight)]    [HDR]_RimCol("边缘光颜色",Color) = (0,0,0,0)
    	[Sub(RimLight)]    _Discolor("去色", Range(0,1)) = 0.0
        [Sub(RimLight)]    _RimWidth("宽度", float) = 0.01
        [Sub(RimLight)]    _Threshold("阈值", Range(0,1)) = 0.8
    	[Sub(RimLight)]    _RimLightInShadow("在阴影中的强度", Range(0,1)) = 0.2
        
//        [Main(ReflectionCol, _, off, off)] 
//        _reflectionCol ("反射光设置", float) = 0
//        [Sub(ReflectionCol)]   _ReflectionCol("反射颜色",Color) = (0,0,0,0)
//        [Sub(ReflectionCol)]   _ReflectionSetting("XY:边缘过渡 Z:反射粗糙度 W:反射颜色(F0)",Vector) = (5,1,0,0)
        
        [Main(Transmission, _, off, off)] 
        _transmission ("透射光设置(适合法线均匀物体)", float) = 0
        [Sub(Transmission)]   [HDR]_TransmissionCol("透射颜色",Color) = (0,0,0,0)
        [Sub(Transmission)]   _TransmissionSetting("TransmissionSetting",float) = 0
    	[Sub(Transmission)]   _TransmissionUseShadow("TransmissionUseShadow",Range(0,1)) = 0
    	
        
        [Main(Group4, _EMISSIVE, off, on)] 
		_group4 ("自发光", float) = 0
        [Sub(Group4)]   [HDR]_EmissiveColor("自发光颜色",Color) = (0,0,0,0)
        [Sub(Group4)]   _EmissiveTex("自发光贴图",2D) = "white"{}
    	
		[Main(StaticAO, _STATIC_AO, off, on)] 
		_staticAO ("AO贴图", float) = 0
		[Sub(StaticAO)]   _OcclusionMult("AO贴图强度",Range(0,1)) = 0.5
		[Sub(StaticAO)]   _OcclusionTex("AO贴图",2D) = "white"{}
    	
    	[Main(AOSetting, _, off, off)]
    	_aoSetting ("AOSetting", float) = 0
    	[Sub(AOSetting)]   _AOInBaseColor("2AO影响主贴图",Range(0,1)) = 0.5
    	[Sub(AOSetting)]   _AOColor("2AO颜色",Color) = (0,0,0,0)
        
        [Main(Group8, _, off, off)] 
		_group8 ("FX-半色调边缘光", float) = 0
        [Sub(Group8)]   _HalftoneEffect("整体强度",Range(0,1)) = 0
        [Sub(Group8)]   _HalftoneStep ("淡入淡出",Range(0.0001,1)) = 0.5
        [Sub(Group8)]   [HDR]_HalftoneColor("边缘颜色",Color) = (1,1,1,1)
        [Sub(Group8)]   _HalftoneScale("网格密度",float) = 10
        [SubToggle(Group8)]   _HalftoneUVZScale("网格密度不受远近影响",float) = 0
        [Sub(Group8)]   _EdgePowScale("XY:边缘过渡PowScale Z:Bias W:Active",Vector) = (1,1,0,1)
        
        [Main(Group9, _, off, off)] 
        _group9 ("描边", float) = 0
        [KWEnum(Group9, Null,_, VertexCol, _SMOOTHNORMAL_VERTEX_COLOR,Tangent, _SMOOTHNORMAL_TANGENT, UV3, _SMOOTHNORMAL_TEXCOORD3)] _SmoothNormalData ("平滑法线数据", float) = 0
        [SubToggle(Group9,_)] _IsTangentSpace("平滑法线在切线空间",Int) = 1
        [Sub(Group9)]  [HDR]_OutlineCol ("描边颜色",Color) = (0,0,0,1)
    	[Sub(Group9)]  _OutlineColInBaseColor ("描边纯度",Range(0,1)) = 0
        [Sub(Group9)]   _OutlineWidth ("描边宽度",float) = -0.1
        [SubToggle(Group9,_UNIFORM_OUTLINE_WIDTH)] _UniformOutline("描边宽度始终不变",float) = 0
        //[Sub(Group9)]   _ViewSpaceZOffset ("深度偏移",Range(0,20)) = 0

		[Main(FoodCooked, _FOOD_COOKED, off, on)] 
		_foodCooked ("食物煮熟", float) = 0
        [Sub(FoodCooked)]   _FoodRawTex("生食贴图",2D) = "white"{}
		[Sub(FoodCooked)]   _FoodCookedTex("熟食贴图",2D) = "white"{}
        [Sub(FoodCooked)]   _FoodBurnedTex("烤焦贴图",2D) = "white"{}

        [Header(Custom Outline)]
        [SubToggle(Group9)] _UseVertexColorRGB ("使用顶点色:RGB颜色",float) = 0
        [SubToggle(Group9)] _UseVertexColorA ("使用顶点色:A描边宽度",float) = 0
        
        
        [Main(Preset, _, on, off)] _PresetGroup ("渲染设置", float) = 0
        [SubToggle(Preset,_USE_SHADOW)] _UseShadow ("接收投影", Float) = 1
    	[SubToggle(Preset,_USE_SSAO)] _UseSSAO ("接收SSAO", Float) = 1
		[Preset(Preset, LWGUI_Preset_BlendMode)] _BlendMode ("Blend Mode Preset", float) = 0
		[SubEnum(Preset, UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
		[SubEnum(Preset, UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 1
		[SubEnum(Preset, UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 0
		[SubToggle(Preset)] _ZWrite1 ("ZWrite ", Float) = 1
		[SubEnum(Preset, UnityEngine.Rendering.CompareFunction)] _ZTest1 ("ZTest", Float) = 4 // 4 is LEqual
		[SubToggle(Preset)] _ZWrite2 ("ZWrite2 ", Float) = 1
		[SubEnum(Preset, UnityEngine.Rendering.CompareFunction)] _ZTest2 ("ZTest2", Float) = 4 // 4 is LEqual
		//[SubEnum(Preset, RGBA, 15, RGB, 14)] _ColorMask ("ColorMask", Float) = 15 // 15 is RGBA (binary 1111)
    	
    	[SubEnum(Preset, OFF (Disabled), 0, ON (NotEqual), 6)] _StencilTest ("Stencil Comp", Float) = 0
		[BitMask(Preset)] _Stencil ("Stencil", Integer) = 0
		//[BitMask(Preset, Left, Bit6, Bit5, Bit4, Description, Bit2, Bit1, Right)] _StencilWithDescription ("Stencil With Description", Integer) = 0
    	[BitMask(Preset)] _Stencil2 ("Stencil2", Integer) = 0
		//[BitMask(Preset, Left, Bit6, Bit5, Bit4, Description, Bit2, Bit1, Right)] _StencilWithDescription2 ("Stencil With Description2", Integer) = 0
        
    }
    SubShader
    {
		Tags { "RenderType" = "Opaque"
			"Queue" = "Geometry"
			"RenderPipeline" = "UniversalPipeline"  }
        Pass
        {
			
            Name "SimpleNPR" 
            Tags{"LightMode" = "UniversalForward"}
		    Cull [_Cull]
		    ZWrite [_ZWrite1]
		    ZTest [_ZTest1]
		    Blend [_SrcBlend] [_DstBlend]
		    //Blend Off
		    //ColorMask [_ColorMask]
            
            Stencil
            {
                Ref [_Stencil]
                Pass Replace
            }
            
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHA_CLIP_ON
            #pragma shader_feature_local_fragment _USE_SHADOW
            #pragma shader_feature_local_fragment _USE_SSAO
            
            #pragma shader_feature_local_fragment _EMISSIVE
            #pragma shader_feature_local_fragment _STATIC_AO
            #pragma shader_feature_local_fragment _NORMALMAP
            #pragma shader_feature_local_fragment _FOOD_COOKED
            
            
            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            
            #include "../Shaders/SimpleNPRInput.hlsl"

            struct Attributes
            {
                float4 color        :COLOR ;
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 texcoord     : TEXCOORD0;
                float2 staticLightmapUV   : TEXCOORD1;
                //float2 dynamicLightmapUV  : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 color                    : TEXCOORD10;
                float2 uv                       : TEXCOORD0;
                float3 positionWS               : TEXCOORD1;
                float3 positionVS               : TEXCOORD6;
                float3 positionOS               : TEXCOORD11;
                float3 normalWS                 : TEXCOORD2;
                float3 normalVS                 : TEXCOORD12;
                float4 tangentWS                : TEXCOORD3;
                float3 viewDirWS                : TEXCOORD4;
                float3 bitangent                : TEXCOORD5;
                float4 shadowCoord              : TEXCOORD7;
                float3 staticLightmapUV         : TEXCOORD8;
                half  fogFactor                 : TEXCOORD9;
                float4 positionNDC              : TEXCOORD13;
                float4 positionCS : SV_POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float3 _bound_center;
            float3 _bound_size;
            float4 _heat1;
            float4 _heat2;
            TEXTURE2D(_FoodRawTex); SAMPLER(sampler_FoodRawTex);
            TEXTURE2D(_FoodCookedTex); SAMPLER(sampler_FoodCookedTex);
            TEXTURE2D(_FoodBurnedTex); SAMPLER(sampler_FoodBurnedTex);
            float4 _FoodRawTex_ST;
            float4 _FoodCookedTex_ST;
            float4 _FoodBurnedTex_ST;

            Varyings vert (Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.uv = TRANSFORM_TEX(input.texcoord,_MainTex);
                output.positionWS = vertexInput.positionWS;
                output.positionVS = vertexInput.positionVS;
                output.positionNDC = vertexInput.positionNDC;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.positionOS = input.positionOS;
            	
                output.viewDirWS = GetCameraPositionWS() - output.positionWS;
                output.normalWS = normalInput.normalWS;
                output.normalVS = TransformWorldToViewDir(output.normalWS, true);
                real sign = input.tangentOS.w * GetOddNegativeScale();
                output.tangentWS = half4(normalInput.tangentWS.xyz, sign);
                output.bitangent = normalInput.bitangentWS;
                output.shadowCoord = GetShadowCoord(vertexInput);
                #if defined(_FOG_FRAGMENT)
                    output.fogFactor  = ComputeFogFactor(vertexInput.positionCS.z);
                #endif
                OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
                output.color = input.color;
                return output;
            }
            
            half MySpecularTerm (float3 roughness , float3 normalWS,float3 lightDir,float3 viewDirWS)
            {
                half specularColor = 0;
                half r2 = roughness*roughness;
                float3 halfDir = SafeNormalize(normalize(lightDir) + float3(normalize(viewDirWS)));
                float NoH = saturate(dot(normalWS,halfDir));
                half a2 = r2*r2;
                specularColor = a2 / (PI*pow((pow(NoH,2)*(a2-1)+1),2)) ;
                specularColor = saturate(specularColor);

                specularColor = lerp(specularColor, step(0.5, specularColor), _SpecularStep);
                
                return specularColor;
            }

            half3 MyTransmission (Light light, float3 normalWS, float3 viewDirWS)
            {
				half3 mainAtten = _TransmissionCol * light.color * light.distanceAttenuation * lerp(1,light.shadowAttenuation,_TransmissionUseShadow);
				half3 hDir = normalize(light.direction) + normalWS;
				half mainVdotL = saturate( dot( normalize(viewDirWS), -hDir ) );
				mainVdotL = saturate(dot(pow(mainVdotL, 1.6), _TransmissionSetting.x));
				half3 TransmissionCol = mainAtten * mainVdotL;
            	return TransmissionCol;
            }



            float4 TransformHClipToViewPortPos(float4 positionCS)
             {
                 float4 o = positionCS * 0.5f;
                 o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
                 o.zw = positionCS.zw;
                 return o / o.w;
             }

            void ApplyAdditionalLight(inout float3  mainColor, half3 mainTex ,float3 worldPos,float3 normalWS,float3 viewDirWS)
            {
                //MultipleLight
                half3 mixColor = 0;
                #if _ADDITIONAL_LIGHTS
                int pixelLightCount = GetAdditionalLightsCount();
                for (int lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    //GetAdditionalLight带上shadowmask的重载有LightCookie的支持
                    Light AdditionalLight = GetAdditionalLight(lightIndex, worldPos,1);
                    half lightAttenuation = AdditionalLight.distanceAttenuation * AdditionalLight.shadowAttenuation;
                    half3 addLightDiffuse = max(0,dot(AdditionalLight.direction,normalWS)*lightAttenuation).rrr * AdditionalLight.color.rgb;
                	addLightDiffuse += MyTransmission(AdditionalLight, normalWS, viewDirWS);
                    half3 addLightSpecular = MySpecularTerm(_Specular1,normalWS,AdditionalLight.direction,viewDirWS) * lightAttenuation * AdditionalLight.color.rgb ;
                    mixColor += lerp(addLightDiffuse,addLightSpecular,_AddSpecular);
                }
                #endif
                mainColor += mixColor * mainTex.rgb;
            }

            float3 RimLighting(float3 positionCS,float3 normalWS,half rimlightWidth,half rimlightThreshold,half rimlightFadeout,half3 rimlightBrightness)
            {
                float3 rimLightColor = (half3)0;

                // float SceneDepth = LoadSceneDepth(positionCS.xy);
                //
                // float linearEyeDepth = LinearEyeDepth(SceneDepth, _ZBufferParams);
                
                float linearEyeDepth = LinearEyeDepth(positionCS.z, _ZBufferParams);
                float3 normalVS = mul((float3x3)UNITY_MATRIX_V, normalWS);

                //problem.2

                //114.59f ·= 2.0 * (180 / 3.1415926)
                //
                float fixedfovValue = atan(1.0f / unity_CameraProjection._m11) * 114.59f * 0.05;
                rimlightWidth *= (1/fixedfovValue);
                //
                // return fixedValue * _fixedValueScaleFactor;//_fixedValueScaleFactor is some small number
                
                float2 uvOffset = float2(sign(normalVS.x), 0) * rimlightWidth / (1 + linearEyeDepth) / 100;
                int2 loadTexPos = positionCS.xy + uvOffset * _ScaledScreenParams.xy;
                loadTexPos = min(max(loadTexPos, 0), _ScaledScreenParams.xy - 1);
                float offsetSceneDepth = LoadSceneDepth(loadTexPos);
                float offsetLinearEyeDepth = LinearEyeDepth(offsetSceneDepth, _ZBufferParams);
                float rimLight = saturate(offsetLinearEyeDepth - linearEyeDepth) * 10 /rimlightFadeout;
                rimLight = clamp(rimLight,0,1);
                rimLight = step(rimlightThreshold,rimLight);

                return rimLight * rimlightBrightness;
            }

            // float3 GetNormizedPos(float3 world_pos, float2 uv, float3 loacl_pos){
            //     float3 half_size = _bound_size / 2;
            //     float3 p0 = float3(_bound_center.x - half_size.x, _bound_center.y - half_size.y, _bound_center.z - half_size.z);
            //     float3 p1 = float3(_bound_center.x + half_size.x, _bound_center.y - half_size.y, _bound_center.z - half_size.z);
            //     float3 p3 = float3(_bound_center.x - half_size.x, _bound_center.y - half_size.y, _bound_center.z + half_size.z);
            //     float3 p4 = float3(_bound_center.x - half_size.x, _bound_center.y + half_size.y, _bound_center.z - half_size.z);
            //     p0 = mul(unity_ObjectToWorld, float4(p0, 1));
            //     p1 = mul(unity_ObjectToWorld, float4(p1, 1));
            //     p3 = mul(unity_ObjectToWorld, float4(p3, 1));
            //     p4 = mul(unity_ObjectToWorld, float4(p4, 1));

            //     float3 vx = p1 - p0;
            //     float3 vy = p4 - p0;
            //     float3 vz = p3 - p0;

            //     float3 size = float3(length(vx),length(vy),length(vz));
                
            //     vx = normalize(vx);
            //     vy = normalize(vy);
            //     vz = normalize(vz);
            //     float3 p02w = world_pos - p0;
            //     // return half4(p02w, 1);
            //     return float3(dot(p02w, vx) / (size.x), dot(p02w, vy)/ (size.y), dot(p02w, vz)/ (size.z));
            // }

            float3 GetNormizedPos(float3 loacl_pos){
                return (((loacl_pos - _bound_center) / _bound_size) + 0.5);
            }

            float GetHeat(float3 v_alpha){
                float4 mid_surface = lerp(_heat1, _heat2, v_alpha.y);
                float2 mid_line = lerp(mid_surface.xy, mid_surface.wz, v_alpha.z);
                return lerp(mid_line.x, mid_line.y, v_alpha.x);
            }

            half4 GetCookedColor(float heat, float2 uv){
                half4 rawColor = SAMPLE_TEXTURE2D(_FoodRawTex,sampler_FoodRawTex,TRANSFORM_TEX(uv,_FoodRawTex));//half4(1,0,0,1)
                half4 cookedColor = SAMPLE_TEXTURE2D(_FoodCookedTex,sampler_FoodCookedTex,TRANSFORM_TEX(uv,_FoodCookedTex));//half4(0, 1, 0, 1)
                half4 burnedColor = SAMPLE_TEXTURE2D(_FoodBurnedTex,sampler_FoodBurnedTex,TRANSFORM_TEX(uv,_FoodBurnedTex));//half4(0,0,0,1)

                heat /= 5;
                // 1 <0.5
                // (1.5- heated)     0.5< <1.5
                // 0 >1.5
                float raw_weight = saturate(1.5 - heat);
                // 0  <0.5
                // heated - 0.5   0.5< <1.5
                // 1  1.5< <2
                // 3- heats 2< < 3
                // 0  >3
                int well_cooked = step(heat, 1.5) * step(2, heat);
                float cooked_weight = min(saturate(3 - heat), saturate(heat - 0.5)) * (1-well_cooked) + well_cooked;
                // 0 <2
                // heated - 2   2< <3
                // 1 0 <3
                float burned_weight = saturate(heat - 2);
                return rawColor * raw_weight + cookedColor * cooked_weight + burnedColor * burned_weight;
            }

            // half4 GetCookedColor(float heat, float2 uv){
            //     if (heat < 2.5)
            //     {
            //         return SAMPLE_TEXTURE2D(_FoodRawTex,sampler_FoodRawTex,TRANSFORM_TEX(uv,_FoodRawTex));//half4(1,0,0,1)
            //     }
            //     else if (heat < 7.5)
            //     {
            //         half4 rawColor = SAMPLE_TEXTURE2D(_FoodRawTex,sampler_FoodRawTex,TRANSFORM_TEX(uv,_FoodRawTex));//half4(1,0,0,1)
            //         half4 cookedColor = SAMPLE_TEXTURE2D(_FoodCookedTex,sampler_FoodCookedTex,TRANSFORM_TEX(uv,_FoodCookedTex));//half4(0, 1, 0, 1)
            //         float alpha = (heat - 2.5) / 5;
            //         return lerp(rawColor, cookedColor, alpha);
            //     }
            //     else if (heat < 10)
            //     {
            //         return SAMPLE_TEXTURE2D(_FoodCookedTex,sampler_FoodCookedTex,TRANSFORM_TEX(uv,_FoodCookedTex));//half4(0, 1, 0, 1)
            //     }
            //     else if (heat < 15)
            //     {
            //         half4 cookedColor = SAMPLE_TEXTURE2D(_FoodCookedTex,sampler_FoodCookedTex,TRANSFORM_TEX(uv,_FoodCookedTex));//half4(0, 1, 0, 1)
            //         half4 burnedColor = SAMPLE_TEXTURE2D(_FoodBurnedTex,sampler_FoodBurnedTex,TRANSFORM_TEX(uv,_FoodBurnedTex));//half4(0,0,0,1)
            //         float alpha = (heat - 10) / 5;
            //         return lerp(cookedColor, burnedColor, alpha);
            //     }
            //     else
            //     {
            //         return SAMPLE_TEXTURE2D(_FoodBurnedTex,sampler_FoodBurnedTex,TRANSFORM_TEX(uv,_FoodBurnedTex));//half4(0,0,0,1)
            //     }
            // }

        // half4 GetCookedColor(float3 world_pos, float2 uv, float3 loacl_pos){
        //         float3 half_size = _bound_size / 2;
        //         float3 p0 = float3(_bound_center.x - half_size.x, _bound_center.y - half_size.y, _bound_center.z - half_size.z);
        //         float3 p1 = float3(_bound_center.x + half_size.x, _bound_center.y - half_size.y, _bound_center.z - half_size.z);
        //         float3 p3 = float3(_bound_center.x - half_size.x, _bound_center.y - half_size.y, _bound_center.z + half_size.z);
        //         float3 p4 = float3(_bound_center.x - half_size.x, _bound_center.y + half_size.y, _bound_center.z - half_size.z);
        //         p0 = mul(unity_ObjectToWorld, float4(p0, 1));
        //         p1 = mul(unity_ObjectToWorld, float4(p1, 1));
        //         p3 = mul(unity_ObjectToWorld, float4(p3, 1));
        //         p4 = mul(unity_ObjectToWorld, float4(p4, 1));

        //         float3 vx = p1 - p0;
        //         float3 vy = p4 - p0;
        //         float3 vz = p3 - p0;

        //         float3 size = float3(length(vx),length(vy),length(vz));
                
        //         vx = normalize(vx);
        //         vy = normalize(vy);
        //         vz = normalize(vz);
        //         float3 p02w = world_pos - p0;
        //         // return half4(p02w, 1);
        //         float3 v_alpha = float3(dot(p02w, vx) / (size.x), dot(p02w, vy)/ (size.y), dot(p02w, vz)/ (size.z));
                
        //         float3 v_alpha2 = (((loacl_pos - _bound_center) / _bound_size) + 0.5);
        //         return half4(v_alpha, 1);
        //         float4 mid_surface = lerp(_heat1, _heat2, v_alpha.y);
        //         float2 mid_line = lerp(mid_surface.xy, mid_surface.wz, v_alpha.z);
        //         float heat = lerp(mid_line.x, mid_line.y, v_alpha.x);
        //         // heat /= 5;

        //         // 1 <0.5
        //         // (1.5- heated)     0.5< <1.5
        //         // 0 >1.5
        //         float raw_weight = saturate(1.5 - heat);
        //         // 0  <0.5
        //         // heated - 0.5   0.5< <1.5
        //         // 1  1.5< <2
        //         // 3- heats 2< < 3
        //         // 0  >3
        //         int well_cooked = step(heat, 1.5) * step(2, heat);
        //         float cooked_weight = min(saturate(3 - heat), saturate(heat - 0.5)) * (1-well_cooked) + well_cooked;
        //         // 0 <2
        //         // heated - 2   2< <3
        //         // 1 0 <3
        //         float burned_weight = saturate(heat - 2);

        //         if (heat < 2.5)
        //         {
        //             return half4(1,0,0,1);
        //         }
        //         else if (heat < 7.5)
        //         {
        //             float alpha = (heat - 2.5) / 5;
        //             return lerp(half4(1,0,0,1), half4(0, 1, 0, 1), alpha);
        //         }
        //         else if (heat < 10)
        //         {
        //             return half4(0,1,0,1);
        //         }
        //         else if (heat < 15)
        //         {
        //             float alpha = (heat - 10) / 5;
        //             return lerp(half4(0,1,0,1), half4(0, 0, 0, 1), alpha);
        //         }
        //         else
        //         {
        //             return half4(0,0,0,1);
        //         }

        //         return half4(1,0,0,1) * raw_weight + half4(0, 1, 0, 1) * cooked_weight + half4(0, 0, 0, 1) * burned_weight;
        //     }

            half4 frag (Varyings input) : SV_Target
            {

                
                UNITY_SETUP_INSTANCE_ID(input);
                //Make ObjectZeroWS work in Particle 
                Light mainLight =  GetMainLight();
                //Shadow
                #if ((_MAIN_LIGHT_SHADOWS) || (_MAIN_LIGHT_SHADOWS_CASCADE) )&& (_USE_SHADOW)
                // Light lighShadow = GetMainLight(shadowCoord);
                // half ShadowAtten = lighShadow.shadowAttenuation;
                half4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
                half4 shadowParams = GetMainLightShadowParams();
                half ShadowAtten = SampleShadowmap(TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture), shadowCoord, shadowSamplingData, shadowParams, false);
            	mainLight.shadowAttenuation = ShadowAtten;
                #endif
                
                //NormalMap
                #ifdef  _NORMALMAP
                float3 normalTS =UnpackNormalScale( SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,input.uv*_NormalMap_ST.xy+_NormalMap_ST.zw),_NormalScale);
                half3x3 TBN = half3x3(input.tangentWS.xyz, input.bitangent, input.normalWS.xyz);
                float3 normalWS = TransformTangentToWorld(normalTS, TBN);
                normalWS = NormalizeNormalPerPixel(normalWS);
                #else
                float3 normalWS = normalize(input.normalWS);
                #endif

            	// TODO R:Specular; G:Gloss; B:Reflect; A:RampID;
            	half4 maskSGRI = 1;

                #if _FOOD_COOKED
            	half4 mainTex = GetCookedColor(GetHeat(GetNormizedPos(input.positionOS)), input.uv) * _MainColor;
                #else
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,TRANSFORM_TEX(input.uv,_MainTex))*_MainColor;
                #endif

				//ssao
            	float2 sceneUV = input.positionNDC.xy/input.positionNDC.w;
            	half ssao = 1;
            	#if (_SCREEN_SPACE_OCCLUSION) && (_USE_SSAO)
            	ssao = lerp(1, SAMPLE_TEXTURE2D_X(_ScreenSpaceOcclusionTexture, sampler_ScreenSpaceOcclusionTexture,sceneUV), _AmbientOcclusionParam.a);
            	#endif

            	//ao
            	half ao = 1;
                #if _STATIC_AO
            	ao = lerp(1, SAMPLE_TEXTURE2D(_OcclusionTex,sampler_OcclusionTex,TRANSFORM_TEX(input.uv,_OcclusionTex)), _OcclusionMult);
            	#endif

            	//ssao和ao影响基础色 增强对比度用
            	mainTex.rgb = mainTex.rgb * lerp(1, lerp(_AOColor, 1, min(ssao,ao)), _AOInBaseColor);
				
                half lambert = saturate(dot(normalize(mainLight.direction),normalWS));
            	half halfLambert = dot(normalize(mainLight.direction),normalWS) * 0.5 + 0.5;
            	
            	//Diffuse
				float2 rampUV = float2(min(min(halfLambert, saturate(mainLight.shadowAttenuation + _ShadowGain)), min(ssao,ao)) , maskSGRI.a);
            	half3 diffuseLight = SAMPLE_TEXTURE2D(_DiffuseRampMap,sampler_LinearClamp,rampUV);
            	diffuseLight *= mainLight.color * mainLight.distanceAttenuation;
                diffuseLight = mainTex.rgb * diffuseLight.rgb ;
            	//Transmission
            	diffuseLight += MyTransmission(mainLight, normalWS, input.viewDirWS);

            	//Specular
                half specular1 = MySpecularTerm(_Specular1,normalWS,mainLight.direction,input.viewDirWS);
                half3 specularCol = _SpecularColor * specular1 * (saturate(lambert * mainLight.shadowAttenuation +_SpecularInShadow)*lambert) * _MainLightColor * lerp(mainTex.rgb,1,_SpecularDiscolor);

                // //RimLight & reflectCol
                // half3 edgeLight = saturate(pow(max(0,1-dot(normalize(input.viewDirWS),normalize(input.normalWS))),_ReflectionSetting.x) * _ReflectionSetting.y) * _ReflectionCol;
                // half3 reflectCol = GlossyEnvironmentReflection(reflect(-normalize(input.viewDirWS),normalWS),input.positionWS,_ReflectionSetting.z,1);
                // reflectCol = lerp(reflectCol,(reflectCol.r+reflectCol.g+reflectCol.b)/3,saturate(_ReflectionSetting.w));
                // // half3 reflectCol = CalculateIrradianceFromReflectionProbes(reflect(-normalize(input.viewDirWS),normalWS),input.positionWS,_RimLightPowScale.z);
                // edgeLight *= reflectCol;

                //屏幕空间深度边缘光 依赖SSAO提供的_CameraDepthTexture
            	half3 rimLight = 0;
            	#if (_SCREEN_SPACE_OCCLUSION)
                float3 samplePositionVS = float3(input.positionVS.xy + input.normalVS.xy * _RimWidth, input.positionVS.z); // 保持z不变（CS.w = -VS.z）
                float4 samplePositionCS = TransformWViewToHClip(samplePositionVS); // input.positionCS不是真正的CS 而是SV_Position屏幕坐标
                float4 samplePositionVP = TransformHClipToViewPortPos(samplePositionCS);
                float depth = input.positionNDC.z / input.positionNDC.w;
                //float depth = LoadSceneDepth(input.positionCS.xy);
                float linearEyeDepth = LinearEyeDepth(depth, _ZBufferParams); // 离相机越近越小
                float offsetDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, samplePositionVP).r; // _CameraDepthTexture.r = input.positionNDC.z / input.positionNDC.w
                float linearEyeOffsetDepth = LinearEyeDepth(offsetDepth, _ZBufferParams);
                float depthDiff = linearEyeOffsetDepth - linearEyeDepth;
                float edgeFactor = step(_Threshold, depthDiff);
            	edgeFactor *= lerp(lambert * mainLight.shadowAttenuation, 1, _RimLightInShadow);
            	rimLight = _RimCol * edgeFactor * lerp(mainTex.rgb,1,_Discolor);
            	#endif
            	
                //emissiveCol
                #ifdef _EMISSIVE
                half3 emissiveCol = _EmissiveColor * SAMPLE_TEXTURE2D(_EmissiveTex,sampler_EmissiveTex,input.uv*_EmissiveTex_ST.xy+_EmissiveTex_ST.zw);
                #else
                half3 emissiveCol = 0;
                #endif
                
                //effects
                half3 effects = 0;
            	
                //edgeHalftone
                float2 halftoneUV = -input.positionVS.xy/input.positionVS.z;
                halftoneUV = lerp(halftoneUV*distance(TransformObjectToWorld(float3(0,0,0)),GetCameraPositionWS()),halftoneUV*10,_HalftoneUVZScale);//stabilize UV tilingScale
                half halftone = length(frac(halftoneUV* _HalftoneScale)-0.5);
                half edgeMask = saturate(pow(max(0,1-dot(normalize(input.viewDirWS),normalize(input.normalWS))),_EdgePowScale.x) * _EdgePowScale.y + _EdgePowScale.z);
            	edgeMask = lerp(1,edgeMask,_EdgePowScale.w);
                halftone = saturate(step(pow(halftone,edgeMask),_HalftoneStep) * edgeMask);
                half3 edgeHalftoneCol = halftone * _HalftoneColor *  _HalftoneEffect;
            	
                effects+=edgeHalftoneCol;

                //globeIllumination
                half3 globeIllumination=0;
                #if defined(LIGHTMAP_ON)
                    globeIllumination = SampleLightmap(input.staticLightmapUV,normalWS);
                #else
                    globeIllumination = SampleSH(normalWS);
                #endif
            		globeIllumination *= mainTex.rgb * ssao;

                //final
                half3 finalCol = 0;
                finalCol += diffuseLight+specularCol + emissiveCol + rimLight  + globeIllumination + effects;
                ApplyAdditionalLight(finalCol,mainTex.rgb,input.positionWS,normalWS,input.viewDirWS);
                finalCol = MixFog(finalCol,input.fogFactor);
                #if _ALPHA_CLIP_ON
                clip(mainTex.a-_Cutoff);
                #endif
                
                return half4(finalCol,mainTex.a);
            }
            ENDHLSL
        }

        Pass 
        {
            Name "OutLinePass"
            Tags { "LightMode" = "FukyOutline"
//            	"RenderType" = "Transparent"
//            	"Queue" = "Transparent + 10"
            }
            //Blend SrcAlpha OneMinusSrcAlpha
            Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite2]
			ZTest [_ZTest2]
	        Cull Front
	        
//            Stencil
//            {
//                Ref [_Stencil]
//                Comp [_StencilComp]
//                Pass [_StencilOp]
//                ReadMask [_StencilReadMask]
//                WriteMask [_StencilWriteMask]
//            }
	        
	        Stencil
            {
                Ref [_Stencil2]
                Comp [_StencilTest]
                Pass Keep
            }

	        
	        HLSLPROGRAM
	        #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
	        
	        #pragma shader_feature_local_vertex _UNIFORM_OUTLINE_WIDTH

	        //SmoothNormalData
	        //#pragma shader_feature_local_vertex _SMOOTHNORMAL_NULL
	        #pragma shader_feature_local_fragment _ALPHA_CLIP_ON
	        #pragma shader_feature_local_vertex _SMOOTHNORMAL_VERTEX_COLOR
	        #pragma shader_feature_local_vertex _SMOOTHNORMAL_TANGENT
	        #pragma shader_feature_local_vertex _SMOOTHNORMAL_TEXCOORD3
	        
	        
	        #pragma multi_compile_fog
	        
	        #pragma vertex vert  
	        #pragma fragment frag

	        #include "../Shaders/SimpleNPRInput.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        :COLOR;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 texcoord     : TEXCOORD0;
                float4 smoothNormalTS     : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                half  fogFactor                 : TEXCOORD9;
                float4 positionCS               : SV_POSITION;
                float3 normalWS                 : TEXCOORD4;
                float2 uv                       : TEXCOORD2;
                float4 color                    : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            float GetCameraFOV()
            {
                //https://answers.unity.com/questions/770838/how-can-i-extract-the-fov-information-from-the-pro.html
                float t = unity_CameraProjection._m11;
                float Rad2Deg = 180 / 3.1415;
                float fov = atan(1.0f / t) * 2.0 * Rad2Deg;
                return fov;
            }
            float ApplyOutlineDistanceFadeOut(float inputMulFix)
            {
                //make outline "fadeout" if character is too small in camera's view
                return saturate(inputMulFix);
            }
            float GetOutlineCameraFovAndDistanceFixMultiplier(float positionVS_Z)
            {
                float cameraMulFix;
                if(unity_OrthoParams.w == 0)
                {
                    ////////////////////////////////
                    // Perspective camera case
                    ////////////////////////////////

                    // keep outline similar width on screen accoss all camera distance       
                    cameraMulFix = abs(positionVS_Z);

                    // can replace to a tonemap function if a smooth stop is needed
                    cameraMulFix = ApplyOutlineDistanceFadeOut(cameraMulFix);

                    // keep outline similar width on screen accoss all camera fov
            //                    cameraMulFix *= GetCameraFOV();       
                }
                else
                {
                    ////////////////////////////////
                    // Orthographic camera case
                    ////////////////////////////////
                    float orthoSize = abs(unity_OrthoParams.y);
                    orthoSize = ApplyOutlineDistanceFadeOut(orthoSize);
                    cameraMulFix = orthoSize * 50; // 50 is a magic number to match perspective camera's outline width
                }

                return cameraMulFix * 0.5; // mul a const to make return result = default normal expand amount WS
            }
            float4 ClipPosZOffset(float4 positionCS, float viewSpaceZOffset)
            {
                float modifiedPositionVS_Z = - positionCS.w - viewSpaceZOffset;

                float modifiedPositionCS_Z = modifiedPositionVS_Z * UNITY_MATRIX_P[2].z + UNITY_MATRIX_P[2].w;

                positionCS.z = modifiedPositionCS_Z * positionCS.w / (-modifiedPositionVS_Z);

                return positionCS;
            }

            float4 ClipPosZOffset_Correct(float3 worldPos, float viewSpaceZOffset)
            {
                float3 viewPos = mul(UNITY_MATRIX_V, float4(worldPos, 1.0)).xyz;
                viewPos.z -= viewSpaceZOffset; // 正常视空间偏移

                float4 clipPos = mul(UNITY_MATRIX_P, float4(viewPos, 1.0)); // 正常投影
                return clipPos;
            }
	        
	        Varyings vert(Attributes v)
	        {
                Varyings o = (Varyings)0;
                o.uv.xy = TRANSFORM_TEX(v.texcoord,_MainTex);
                o.normalWS = normalize(GetVertexNormalInputs(v.normalOS).normalWS);
                o.color = v.color;
                VertexPositionInputs vertexPositionInput = GetVertexPositionInputs(v.positionOS.xyz);
                _OutlineWidth = max(-0.1,_OutlineWidth);
                _OutlineWidth *= GetOutlineCameraFovAndDistanceFixMultiplier(vertexPositionInput.positionVS.z);
                
                #ifdef _UNIFORM_OUTLINE_WIDTH
                _OutlineWidth *= vertexPositionInput.positionCS.w * 0.1;
                #endif

                // _OutlineWidth *= lerp(1,o.color.a,_UseVertexColorA);
                _OutlineWidth = lerp(_OutlineWidth,_OutlineWidth * o.color.a,_UseVertexColorA);
                
	            float3x3 TBNOS = float3x3(v.tangentOS.xyz,cross(v.normalOS,v.tangentOS.xyz) * v.tangentOS.w,v.normalOS.xyz);
                TBNOS = lerp(float3x3(float3(1,0,0),float3(0,1,0),float3(0,0,1)),TBNOS,_IsTangentSpace);
                
                #if _SMOOTHNORMAL_VERTEX_COLOR
	            o.positionCS = TransformObjectToHClip(v.positionOS.xyz + normalize(mul(v.color.rgb*2-1,TBNOS).xyz) * 0.01 * _OutlineWidth );
	            #elif  _SMOOTHNORMAL_TANGENT
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz + normalize(v.tangentOS.xyz) * 0.01 * _OutlineWidth );
	            #elif _SMOOTHNORMAL_TEXCOORD3
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz + normalize(mul(v.smoothNormalTS.xyz,TBNOS)) * 0.01 * _OutlineWidth );
	            #else
	            o.positionCS = TransformObjectToHClip(v.positionOS.xyz + v.normalOS.xyz * 0.01 * _OutlineWidth );
	            #endif

                
                #if defined(_FOG_FRAGMENT)
                    o.fogFactor  = ComputeFogFactor(o.positionCS.z);
                #endif
                //禁用深度偏移
                //o.positionCS = ClipPosZOffset(o.positionCS,_ViewSpaceZOffset);
                return o;
	        }
	        
	         half4 frag(Varyings input) : SV_Target
	        {
	            UNITY_SETUP_INSTANCE_ID(input);
				half4 mainTex = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv);
	            #if _ALPHA_CLIP_ON
	            clip(mainTex.a - _Cutoff);
	            #endif
	            //half3 globeIllumination = SampleSH(input.normalWS)*0.5;
	            half3 outlineCol = lerp(_OutlineCol,input.color,_UseVertexColorRGB).rgb ;
	        	outlineCol = lerp(outlineCol,mainTex.rgb,_OutlineColInBaseColor);
	        	
	            
                return half4(MixFog(outlineCol,input.fogFactor).rgb, _OutlineCol.a);
	        }
	     ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            Cull [_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            //#pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature_local_fragment  _ALPHA_CLIP_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "../Shaders/SimpleNPRInput.hlsl"
            #include "../Shaders/SimpleNPRShadowPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull off

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            //#pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature_local_fragment  _ALPHA_CLIP_ON
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "../Shaders/SimpleNPRInput.hlsl"
            #include "../Shaders/SimpleNPRDepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}

            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 2.0

            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHA_CLIP_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            ENDHLSL
        }
        
    }CustomEditor "LWGUI.LWGUI"
}
