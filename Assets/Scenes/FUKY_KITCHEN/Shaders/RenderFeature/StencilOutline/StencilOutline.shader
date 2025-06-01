Shader "Unlit/StencilOutLine"
{
    Properties
    {
        [BitMask(Preset)] _Stencil ("_Stencil", Integer) = 0
        [HDR]_Color("_Color",color) = (1,1,1,1)
        _Width("_Width",range(0,1)) = 0.2
        [Toggle]_UseUV3("_UseUV3",float) = 0
    }
    Subshader
    {
        Pass
        {
            Tags {"LightMode" = "LightweightForward" "RenderType" = "Opaque" "Queue" = "Geometry + 10"}
                    //Tags可不添加，只是为了演示
     
            colormask 0 //不输出颜色
            ZWrite Off
            ZTest Off
     
            Stencil
            {
                Ref [_Stencil]
                Comp Always
                Pass replace
            }
 
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
 

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

 
            struct appdata
            {
                float4 vertex       : POSITION;
            };
 
            struct v2f
            {
                float4 vertex: SV_POSITION;
            };
 
            v2f vert(appdata v)
            {
                v2f o;
                                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }
 
            half4 frag(v2f i) : SV_Target
            {
                return half4 (0.5h, 0.0h, 0.0h, 1.0h);
            }
            ENDHLSL
        }
 
        Pass
        {
            Tags {"RenderType" = "Opaque" "Queue" = "Geometry + 20"}
 
            ZTest off
            Blend SrcAlpha OneMinusSrcAlpha
 
            Stencil {
                Ref [_Stencil]
                Comp notEqual
                Pass keep
            }
 
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct appdata
            {
                float4 vertex: POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float4 uv: TEXCOORD0;
                float4 smoothNormalTS: TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
 
            struct v2f
            {
                float4 vertex: SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
 
            half4 _Color;
            half _Width;
            half _UseUV3;
 
            v2f vert(appdata input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                //v.vertex.xyz += _Width * normalize(v.vertex.xyz);
                float3x3 TBNOS = float3x3(input.tangentOS.xyz,cross(input.normalOS,input.tangentOS.xyz) * input.tangentOS.w,input.normalOS.xyz);
                float3 smoothNormalOS = normalize(mul(input.smoothNormalTS.xyz,TBNOS));
                float3 normalOS = (_UseUV3<0.5) ? input.normalOS : smoothNormalOS;


                VertexPositionInputs vertexPositionInput = GetVertexPositionInputs(input.vertex.xyz);
                float3 viewPosition = vertexPositionInput.positionVS;
                float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normalOS));

                output.vertex = TransformWViewToHClip(viewPosition + viewNormal * -viewPosition.z * _Width / 10.0);

                return output;
            }
 
            half4 frag(v2f input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return _Color;
            }
            ENDHLSL
        }
    }CustomEditor "LWGUI.LWGUI"
}