Shader "XinY/Par/AlphaBlendUVMove"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        [HDR]_BaseColor ("BaseColor", Color) = (1, 1, 1, 1)
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=0
        [Enum(Off, 0, On, 1)]_ZWrite("ZWrite",int)=0
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest",int)=4
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
            half4 _MainTex_ST;
            half4 _BaseColor;
        CBUFFER_END
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        struct appdata
        {
            half4 posOS : POSITION;
            float4 uv0 : TEXCOORD0;
            float2 uv1:TEXCOORD1;
            half4 color : COLOR;
        };

        struct v2f
        {
            float2 uv0 : TEXCOORD0;
            half4 posCS : SV_POSITION;
            half4 color : TEXCOORD1;
            float4 customData :TEXCOORD2;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.posCS = TransformObjectToHClip(v.posOS);
            o.uv0 = TRANSFORM_TEX(v.uv0.xy, _MainTex);
            o.color = v.color;
            o.customData=float4(v.uv0.zw,v.uv1.xy);
            return o;
        }

        half4 frag(v2f i) : SV_Target
        {
            half3 finalcolor = _BaseColor.rgb;
            half alpha = _BaseColor.a;
            float2 uv=i.uv0*i.customData.xy+i.customData.zw;
            half4 maintex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            
            finalcolor = finalcolor * maintex.rgb * i.color.rgb;
            alpha = alpha * maintex.a * i.color.a;
            return half4(finalcolor, alpha);
        }
        
        ENDHLSL
        Pass
        {
            ZWrite [_ZWrite]
            Blend SrcAlpha OneMinusSrcAlpha
            Cull [_Cull]
            ZTest [_ZTest]
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}

