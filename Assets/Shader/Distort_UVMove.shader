Shader "XinY/Par/DistortUVMove"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        [HDR]_BaseColor ("BaseColor", Color) = (1, 1, 1, 1)
        _MaskTex ("MaskTex", 2D) = "white" { }
        _DistortTex("DistortTex",2D)="black"{}
        _DistortIntensity("DistortIntensity",float)=0
        [Enum(UnityEngine.Rendering.BlendMode)]_Blend1("Sorce",int)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_Blend2("Dest",int)=1
        [Enum(UnityEngine.Rendering.CullMode)]_Cull ("Cull", int) = 0
        [Enum(Off, 0, On, 1)]_ZWrite ("ZWrite", int) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest ("ZTest", int) = 4
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
            half4 _MainTex_ST;
            half4 _BaseColor;
            half4 _MaskTex_ST;
            half4 _DistortTex_ST;
            half _DistortIntensity;
        CBUFFER_END
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_MaskTex);
        SAMPLER(sampler_MaskTex);
        TEXTURE2D(_DistortTex);
        SAMPLER(sampler_DistortTex);
        struct appdata
        {
            half4 posOS : POSITION;
            float4 uv0 : TEXCOORD0;
            float4 uv1 : TEXCOORD1;
            half4 color : COLOR;
            
        };

        struct v2f
        {
            float2 uv0 : TEXCOORD0;
            half4 posCS : SV_POSITION;
            half4 color : TEXCOORD1;
            float2 customData1 : TEXCOORD2;
            float4 customData2 : TEXCOORD3;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.posCS = TransformObjectToHClip(v.posOS);
            o.uv0 = v.uv0.xy;
            o.color = v.color;
            o.customData1 = v.uv0.zw;
            o.customData2=v.uv1;
            return o;
        }

        half4 frag(v2f i) : SV_Target
        {
            half3 finalcolor = _BaseColor.rgb;
            half alpha = _BaseColor.a;

            float2 distortUV=i.uv0*_DistortTex_ST.xy+_DistortTex_ST.zw+i.customData2.zw;
            half distort=SAMPLE_TEXTURE2D(_DistortTex,sampler_DistortTex,distortUV).r*_DistortIntensity;

            float2 uv = i.uv0*_MainTex_ST.xy+_MainTex_ST.zw+i.customData1.xy+distort;
            half4 maintex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            
            finalcolor = finalcolor * maintex.rgb * i.color.rgb;

            float2 maskUV=i.uv0*_MaskTex_ST.xy+_MaskTex_ST.zw+i.customData2.xy-distort;
            half mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, maskUV).r;

            alpha = alpha * maintex.a * i.color.a*mask;
            return half4(finalcolor, alpha);
        }
        
        ENDHLSL
        Pass
        {
            ZWrite [_ZWrite]
            Blend [_Blend1] [_Blend2]
            Cull [_Cull]
            ZTest [_ZTest]
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}

