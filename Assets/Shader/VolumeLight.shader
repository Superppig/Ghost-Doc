Shader "XinY/VolumeLight"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        HLSLINCLUDE
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
        #pragma shader_feature _ _VOLUME_LIGHT
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "../Shader/Include/XinY_Include_URP.hlsl"
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
            float3 direction : TEXCOORD1;
        };


        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_TexelSize;
        CBUFFER_END
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        

        TEXTURE2D(_CameraDepthTexture);
        SAMPLER(sampler_CameraDepthTexture);
        float4x4 _TransformM;

        ENDHLSL

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
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

                int t = 0;
                if (v.texcoord.x < 0.5 && v.texcoord.y < 0.5)
                {
                    t = 0;
                }
                else if (v.texcoord.x > 0.5 && v.texcoord.y < 0.5)
                {
                    t = 1;
                }
                else if (v.texcoord.x > 0.5 && v.texcoord.y > 0.5)
                {
                    t = 2;
                }
                else
                {
                    t = 3;
                }
                o.direction = _TransformM[t].xyz;
                return o;
            }



            half4 frag(v2f v) : SV_TARGET
            {
                half4 output = 0;

                #ifdef _VOLUME_LIGHT
                    float depth = LinearEyeDepth(SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, v.texcoord).x, _ZBufferParams);
                    float3 posWS = _WorldSpaceCameraPos + depth * v.direction;
                    float3 startPos = _WorldSpaceCameraPos;
                    half3 ray = normalize(posWS - startPos);
                    half feather = VL_DepthFeather(depth);
                    half lum = VL_GetVolumeLightIntensity(startPos, ray, depth);
                    output = half4(lum.xxx, 1);
                    //output=VL_GetShadowFactor(posWS);
                    //output=frac(posWS.xyzz);
                    //output=feather;
                #endif
                //output.xyz+=mainTex;

                return clamp(output, 0, 1);
            }
            ENDHLSL
        }

    }
}
