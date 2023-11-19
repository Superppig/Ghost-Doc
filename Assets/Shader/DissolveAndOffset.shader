Shader "XinY/Par/DissolveAndOffset"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        [HDR]_BaseColor ("BaseColor", Color) = (1, 1, 1, 1)
        _HardDis ("HardDis", Range(0, 1)) = 1
        _DissolveTex ("DissolveTex", 2D) = "white" { }
        [NoScaleOffset]_Mask ("Mask", 2D) = "white" { }
        _MaskOffset ("MaskOffset", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
        struct appdata
        {
            float4 posOS : POSITION;
            float4 uv0 : TEXCOORD0;
            float4 color : COLOR;
        };

        struct v2f
        {
            float4 uv0 : TEXCOORD0;
            float4 posCS : SV_POSITION;
            half4 color : TEXCOORD1;
            float2 dissolveUV : TEXCOORD2;
            float2 mainTexUV : TEXCOORD3;
        };
        TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
        TEXTURE2D(_DissolveTex);    SAMPLER(sampler_DissolveTex);
        TEXTURE2D(_Mask);    SAMPLER(sampler_Mask);
        float4 _MainTex_ST, _DissolveTex_ST;
        float4 _BaseColor;
        float _HardDis, _MaskOffset;


        v2f vert(appdata v)
        {
            v2f o;
            o.posCS = TransformObjectToHClip(v.posOS);
            o.uv0.xy = v.uv0;
            o.dissolveUV = TRANSFORM_TEX(v.uv0.xy, _DissolveTex);
            o.color = v.color;
            float2 customData = v.uv0.zw;
            //maskoffet为1，控制溶解移动，反之控制主贴图移动
            o.mainTexUV = TRANSFORM_TEX(v.uv0.xy, _MainTex)
            + lerp(customData, 0, _MaskOffset);
            o.dissolveUV = TRANSFORM_TEX(v.uv0.xy, _DissolveTex)
            + lerp(0, customData, _MaskOffset);
            return o;
        }

        half4 frag(v2f i) : SV_Target
        {
            float3 finalcolor = float3(1, 1, 1);
            float alpha = 1;


            //贴图数据
            float4 maintex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.mainTexUV);
            half4 dissolveTex = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, i.dissolveUV);
            
            half4 mainWithBase = maintex * _BaseColor;
            finalcolor = mainWithBase.rgb * i.color.rgb;

            //Dissolve
            half disValue = clamp(dissolveTex.r * dissolveTex.a,0.01,0.99);
            half softDis = clamp(disValue + (i.color.a * 2 - 1), 0, 1);
            half hardDis = step(1 - i.color.a , disValue);

            //溶解时顶点色不控制透明度
            half finalDis = lerp(softDis, hardDis, _HardDis) * mainWithBase.a;

            //Mask
            half4 maskMap = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.uv0);
            half mask = maskMap.r * maskMap.a;

            alpha = mask * finalDis;
            //透明也可代替溶解，不一定要用clip
            return float4(finalcolor, alpha);
        }
        
        ENDHLSL
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}

