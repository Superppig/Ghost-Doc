Shader "XinY/FlowLED"
{
    Properties
    {
        _FlowSpeed("FlowSpeed",float)=1.0
        _FlowCycleNum("FlowCycleNum",float)=1.0
        _FlowSmooth("FlowSmooth Min Max",vector)=(0,1,0,0)
        [HDR]_FlowColor("LEDColor",color)=(1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        HLSLINCLUDE
        #include "../Shader/Include/XinY_Include_URP.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        CBUFFER_START(UnityPerMaterial)
        half _FlowSpeed;
        half _FlowCycleNum;
        half2 _FlowSmooth;
        half4 _FlowColor;
        CBUFFER_END
    
        struct appdata
        {
            float3 positionOS:POSITION;
            float2 texcoord : TEXCOORD0;
            half3 normalOS:NORMAL;
        };
        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 positionCS : SV_POSITION;

        };

        ENDHLSL

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            v2f vert(appdata v)
            {
                v2f output = (v2f)0;;
                //坐标获取
                output.positionCS=TransformObjectToHClip(v.positionOS);
                output.uv=v.texcoord;
                return output;
            }
            float4 frag(v2f i) : SV_TARGET
            {
                float stepValue=frac(i.uv.x*_FlowCycleNum+_Time.y*_FlowSpeed);
                float flow=1-2*distance(0.5,stepValue);
                flow=smoothstep(_FlowSmooth.x,_FlowSmooth.y,flow);
                float4 color=flow*_FlowColor;
                return color;
            };
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            HLSLPROGRAM
            #pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma vertex vert
            #pragma fragment frag
            float3 _LightDirection;
            float3 _LightPosition;
            v2f vert(appdata v)
            {
                v2f output;

                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);

                //pragma这个变体，然后聚光灯开启阴影这个变体就会自动启动
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return output;
            }
            half4 frag(v2f i) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert(appdata v)
            {
                v2f output = (v2f)0;
                output.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                return output;
            }
            half4 frag(v2f i) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }

    }
}