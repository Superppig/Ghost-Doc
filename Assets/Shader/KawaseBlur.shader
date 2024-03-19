Shader "XinY/KawaseBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" }
        LOD 100
        Cull Off
        ZWrite Off
        ZTest Always
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_TexelSize;
        CBUFFER_END
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        float _Size;

        struct appdata
        {
            float4 positionOS : POSITION;
            float2 texcoord : TEXCOORD;
        };
        struct v2f
        {
            float4 positionCS : SV_POSITION;
            float2 texcoord : TEXCOORD;
        };
        v2f vertex(appdata i)//水平方向的采样

        {
            v2f o;
            o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
            o.texcoord = i.texcoord;
            return o;
        }
        half4 fragment(v2f i) : SV_TARGET
        {
            half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            tex += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(-1, -1) * _MainTex_TexelSize.xy * _Size);
            tex += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(1, -1) * _MainTex_TexelSize.xy * _Size);
            tex += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(-1, 1) * _MainTex_TexelSize.xy * _Size);
            tex += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(1, 1) * _MainTex_TexelSize.xy * _Size);
            return tex / 5.0;
        }
        ENDHLSL
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vertex
            #pragma fragment fragment
            ENDHLSL
        }
    }
}