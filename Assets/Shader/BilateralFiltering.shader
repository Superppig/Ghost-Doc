Shader "XinY/BilateralFiltering"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "black" { }
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        HLSLINCLUDE
        #pragma shader_feature _ _VOLUME_LIGHT

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #ifdef _VOLUME_LIGHT
            #include "./Include/XinY_VolumeRender_Include.hlsl"
        #endif
        struct appdata
        {
            float4 positionOS : POSITION;
            float2 texcoord : TEXCOORD0;
        };
        struct v2f
        {
            float2 texcoord : TEXCOORD0;
            float4 positionCS : SV_POSITION;
        };


        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_TexelSize;
        CBUFFER_END
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_SourTex);
        SAMPLER(sampler_SourTex);

        ENDHLSL

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off
            ZTest Always
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.texcoord = v.texcoord;
                #if UNITY_UV_STARTS_AT_TOP
                    if (_MainTex_TexelSize.y < 0)
                        o.texcoord.y = 1 - o.texcoord.y; //满足上面两个条件时uv会翻转，因此需要转回来
                #endif
                return o;
            }
            #ifdef _VOLUME_LIGHT
                half VL_BilateralFiltering(float2 uv)
                {
                    float weight_sum = 0;
                    float color_sum = 0;
                    float color_origin = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                    for (int i = -_VL_BlurSize; i < _VL_BlurSize; i++)
                    {
                        for (int j = -_VL_BlurSize; j < _VL_BlurSize; j++)
                        {
                            //空域高斯
                            float2 varible = uv + float2(i * _MainTex_TexelSize.x, j * _MainTex_TexelSize.y);
                            float space_factor = i * i + j * j;
                            space_factor = (-space_factor) / (2 * _VL_BF_SpaceSigma * _VL_BF_SpaceSigma);
                            float space_weight = 1 / (_VL_BF_SpaceSigma * _VL_BF_SpaceSigma * 2 * XinY_PI) * exp(space_factor);

                            //值域高斯
                            float color_neighbor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, varible);
                            float color_distance = (color_neighbor - color_origin);
                            float value_factor = color_distance * color_distance;
                            value_factor = (-value_factor) / (2 * _VL_BF_RangeSigma * _VL_BF_RangeSigma);
                            float value_weight = (1 / (_VL_BF_RangeSigma * _VL_BF_RangeSigma * 2 * XinY_PI)) * exp(value_factor);

                            weight_sum += space_weight * value_weight;
                            color_sum += color_neighbor * space_weight * value_weight;
                        }
                    }
                    half output = color_sum / weight_sum;

                    return output;
                }
            #endif

            half4 frag(v2f v) : SV_TARGET
            {
                half4 output = 0;
                #ifdef _VOLUME_LIGHT
                    output = VL_BilateralFiltering(v.texcoord);
                #endif
                return output;
            }
            ENDHLSL
        }
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off
            ZTest Always
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.texcoord = v.texcoord;
                #if UNITY_UV_STARTS_AT_TOP
                    if (_MainTex_TexelSize.y < 0)
                        o.texcoord.y = 1 - o.texcoord.y; //满足上面两个条件时uv会翻转，因此需要转回来
                #endif
                return o;
            }

            half4 frag(v2f v) : SV_TARGET
            {
                half4 output = 1;
                #ifdef _VOLUME_LIGHT
                    half3 sourTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, v.texcoord);
                    half3 vlTex = SAMPLE_TEXTURE2D(_SourTex, sampler_SourTex, v.texcoord) * _MainLightColor;
                    output.xyz = vlTex + sourTex;
                #endif
                return output;
            }
            ENDHLSL
        }
    }
}
