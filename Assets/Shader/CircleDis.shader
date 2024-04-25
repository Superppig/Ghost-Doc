Shader "XinY/Par/CircleDis"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        _Mask ("Mask", 2D) = "White" { }
        _DissolveTex ("DissolveTex", 2D) = "white" { }
        [HDR] _BaseColor ("BaseColor", Color) = (1, 1, 1, 1)
        [Enum(UnityEngine.Rendering.CullMode)]_Cull ("Cull", int) = 0
        [Enum(Off, 0, On, 1)]_ZWrite ("ZWrite", int) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest ("ZTest", int) = 4
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "../Shader/Include/XinY_Include_URP.hlsl"

        struct appdata
        {
            float4 posOS : POSITION;
            float4 uv0 : TEXCOORD0;//xy:uv zw:custom
            float2 uv1 : TEXCOORD1;//uv
            float4 color : COLOR;
        };

        struct v2f
        {
            float2 uv0 : TEXCOORD0;
            float2 uvDis : TEXCOORD1;
            float4 customData : TEXCOORD2;
            float4 posVS : SV_POSITION;
            float4 color : TEXCOORD3;
        };


        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _MainTex_ST;
            float4 _DissolveTex_ST;
            float4 _Mask_ST;
        CBUFFER_END
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_DissolveTex);
        SAMPLER(sampler_DissolveTex);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);

        ENDHLSL
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            ZWrite [_ZWrite]
            Blend SrcAlpha OneMinusSrcAlpha
            Cull [_Cull]
            ZTest [_ZTest]
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert(appdata v)
            {
                v2f o;
                o.customData = v.uv0;
                o.posVS = TransformObjectToHClip(v.posOS);
                o.uv0 = TRANSFORM_TEX(v.uv1.xy, _MainTex);
                o.uvDis = TRANSFORM_TEX(v.uv1.xy, _DissolveTex);
                o.color = v.color;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 finalcolor = float3(1, 1, 1);
                float alpha = 1;
                //maskUV
                float2 maskUV = i.customData.xy * _Mask_ST.xy + i.customData.zw;
                //贴图数据
                float4 maintex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv0);
                float mask = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, maskUV).r;
                float dissolve = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, i.uvDis).r;

                //溶解
                dissolve = step(1 - i.color.a, dissolve);
                alpha = alpha * mask * dissolve * maintex.a;

                finalcolor = maintex.rgb * _BaseColor.rgb * i.color.rgb;
                //return maintex;
                return float4(finalcolor, alpha);
            }
            ENDHLSL
        }
    }
}

