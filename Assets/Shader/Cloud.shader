
Shader "XinY/Cloud"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _CloudNoise ("CloudNoise", 2D) = "black" { }
        _CloudDistort ("CloudDistort", 2D) = "black" { }
        _CloudDetail ("CloudDetail", 2D) = "gray" { }
        _CloudStart ("CloudStart", Range(0, 1)) = 0.1
        _CloudEnd ("CloudEnd", Range(0, 1)) = 0.8
        _CloudStart2 ("CloudStart_2", Range(0, 1)) = 0.1
        _CloudEnd2 ("CloudEnd_2", Range(0, 1)) = 0.8
        _DistortIntensity ("DistortIntensity", Range(0, 0.1)) = 0.05
        _FresnelBias ("FresnelBias", Range(-1, 1)) = 0
        _FresnelScale ("FresnelScale", float) = 1
        _FresnelPower ("FresnelPower", float) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline" "IgnoreProjector" = "True" "Queue" = "Transparent" }
        LOD 100
        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "../Shader/Include/XinY_Include_URP.hlsl"

        struct appdata
        {
            float4 positionOS : POSITION;
            float4 texcoord : TEXCOORD0;
            half3 normal : NORMAL;
        };
        struct v2f
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD1;
            half3 normalWS : TEXCOORD2;
            float4 uv : TEXCOORD0;
        };


        CBUFFER_START(UnityPerMaterial)
            half4 _CloudNoise_ST;
            half _CloudStart;
            half _CloudEnd;
            half _CloudStart2;
            half _CloudEnd2;
            half4 _CloudDistort_ST;
            half _DistortIntensity;
            half4 _CloudDetail_ST;
            half _FresnelBias;
            half _FresnelPower;
            half _FresnelScale;
            half4 _Color;
        CBUFFER_END
        TEXTURE2D(_CloudNoise);SAMPLER(sampler_CloudNoise);
        TEXTURE2D(_CloudDistort);SAMPLER(sampler_CloudDistort);
        TEXTURE2D(_CloudDetail);SAMPLER(sampler_CloudDetail);


        ENDHLSL

        Pass
        {

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert(appdata v)
            {
                v2f output = (v2f)0;;
                output.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(v.positionOS);
                output.normalWS = TransformObjectToWorldNormal(v.normal);
                output.uv = v.texcoord;
                return output;
            }

            half4 frag(v2f i) : SV_TARGET
            {
                float2 uv = i.uv;
                float2 distort_uv = i.uv * _CloudDistort_ST.xy + _CloudDistort_ST.zw * _Time.y;
                float distort = SAMPLE_TEXTURE2D(_CloudDistort, sampler_CloudDistort, distort_uv) * _DistortIntensity;

                float2 cloud_uv_1 = i.uv * _CloudNoise_ST.xy + _CloudNoise_ST.zw * _Time.y + distort;
                float cloud_1 = SAMPLE_TEXTURE2D(_CloudNoise, sampler_CloudNoise, cloud_uv_1);
                cloud_1 = smoothstep(_CloudStart, _CloudEnd, cloud_1);

                float2 cloud_uv_2 = i.uv * _CloudDetail_ST.xy + _CloudDetail_ST.zw * _Time.y + distort;
                float cloud_2 = SAMPLE_TEXTURE2D(_CloudDetail, sampler_CloudDetail, cloud_uv_2);
                cloud_2 = smoothstep(_CloudStart2, _CloudEnd2, cloud_2);
                float cloudBlend = lerp(cloud_1 + cloud_2, cloud_1, step(0.1, cloud_1));
                half3 N = normalize(i.normalWS);
                half3 V = normalize(_WorldSpaceCameraPos - i.positionWS);
                half fresnelMask = 1 - max(0, dot(N, V));
                fresnelMask = clamp(pow(fresnelMask + _FresnelBias, _FresnelPower) * _FresnelScale, 0, 1);

                half boxMask1 = smoothstep(0, 0.1, 1 - distance(0.5, i.uv.x) * 2);
                half boxMask2 = smoothstep(0, 0.1, 1 - distance(0.5, i.uv.y) * 2);
                half boxMask = boxMask1 * boxMask2;
                //return fresnelMask;
                half cloud = lerp(cloud_2 * 0.5, cloudBlend, fresnelMask);

                float distanceMask = distance(i.positionWS, _WorldSpaceCameraPos);
                distanceMask = smoothstep(0, 200, distanceMask);

                return cloud * _Color * boxMask * distanceMask;
            }
            ENDHLSL
        }
    }
}