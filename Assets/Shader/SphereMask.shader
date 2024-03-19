Shader "XinY/SphereMask"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" { }
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" }
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "../Shader/Include/XinY_Include_URP.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_TexelSize;
            real4 _ColorX;
            real4 _ColorY;
            real4 _ColorZ;
            float _Width;
            float _Spread;
            float4 _Speed;
            float _EdgeSample;
            float _DepthSensitivity;
            float _NormalSensitivity;
            float4 _OutLineColor;
        CBUFFER_END
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_CameraDepthTexture);
        SAMPLER(sampler_CameraDepthTexture);
        TEXTURE2D(_BlurTex);
        SAMPLER(sampler_BlurTex);
        float4x4 _TransformM;
        float3 _TargetPos;
        float _MaskStart;
        float _MaskEnd;

        struct a2v
        {
            float4 positionOS : POSITION;
            float2 texcoord : TEXCOORD;
        };
        struct v2f
        {
            float4 positionCS : SV_POSITION;
            float2 texcoord : TEXCOORD;
            float3 direction : TEXCOORD1;
        };

        ENDHLSL
        pass
        {
            HLSLPROGRAM
            #pragma vertex VERT
            #pragma fragment FRAG

            #pragma shader_feature _ OUTMASK_ON

            
            v2f VERT(a2v i)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.texcoord = i.texcoord;
                #if UNITY_UV_STARTS_AT_TOP
                    if (_MainTex_TexelSize.y < 0)
                        o.texcoord.y = 1 - o.texcoord.y; //满足上面两个条件时uv会翻转，因此需要转回来
                #endif

                int t = 0;
                if (i.texcoord.x < 0.5 && i.texcoord.y < 0.5)
                {
                    t = 0;
                }
                else if (i.texcoord.x > 0.5 && i.texcoord.y < 0.5)
                {
                    t = 1;
                }
                else if (i.texcoord.x > 0.5 && i.texcoord.y > 0.5)
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
            half4 FRAG(v2f i) : SV_TARGET
            {
                float4 source = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
                float depth = LinearEyeDepth(SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord).x, _ZBufferParams);
                float3 posWS = _WorldSpaceCameraPos + depth * i.direction;
                half dist=distance(posWS,_TargetPos);
                dist=smoothstep(_MaskStart,_MaskEnd,dist);
                float4 blur = SAMPLE_TEXTURE2D(_BlurTex, sampler_BlurTex, i.texcoord);
                float4 c=lerp(source,blur,dist);
                #if OUTMASK_ON
                c=dist;
                #endif
                return c;
            }
            ENDHLSL
        }
    }
}