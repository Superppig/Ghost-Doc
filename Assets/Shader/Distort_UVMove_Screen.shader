Shader "XinY/Par/DistortUVMove_Screen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" { }
        [Toggle(_USE_MAIN_ALPHA)]_USE_MAIN_ALPHA ("use mainAlpha", int) = 0
        [HDR]_BaseColor ("BaseColor", Color) = (1, 1, 1, 1)
        _MaskTex ("MaskTex", 2D) = "white" { }
        _DistortTex ("DistortTex", 2D) = "black" { }
        _DistortMask ("DistortMask", 2D) = "white" { }
        _DistortIntensity ("DistortIntensity", Range(0, 0.2)) = 0
        [Enum(UnityEngine.Rendering.BlendMode)]_Blend1 ("Sorce", int) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_Blend2 ("Dest", int) = 1
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
        TEXTURE2D(_DistortMask);
        SAMPLER(sampler_DistortMask);
        TEXTURE2D(_CameraOpaqueTexture);
        SAMPLER(sampler_CameraOpaqueTexture);

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
            float4 customData : TEXCOORD2;
            float2 uv1 : TEXCOORD3;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.posCS = TransformObjectToHClip(v.posOS);
            o.uv0 = v.uv0.xy;
            o.uv1 = v.uv0.zw;
            o.color = v.color;
            o.customData = v.uv1;
            return o;
        }

        half4 frag(v2f i) : SV_Target
        {
            half3 finalcolor = _BaseColor.rgb;
            half alpha = _BaseColor.a;

            float2 mainuv = i.uv0 * _MainTex_ST.xy + _MainTex_ST.zw;
            half4 maintex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainuv);

            float2 distortUV = i.uv0 ;
            half distortMask=SAMPLE_TEXTURE2D(_DistortMask, sampler_DistortMask, distortUV).r;
            distortUV+= + i.customData.xy;
            half distort = SAMPLE_TEXTURE2D(_DistortTex, sampler_DistortTex, distortUV).r * _DistortIntensity*distortMask ;
            
            float2 screenUV = GetNormalizedScreenSpaceUV(i.posCS);
            half4 opaque = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + distort);
            
            
            finalcolor = finalcolor * maintex.rgb * i.color.rgb;

            float2 maskUV = i.uv0 * _MaskTex_ST.xy + _MaskTex_ST.zw + i.customData.zw;
            half mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, maskUV).r;
            #ifdef _USE_MAIN_ALPHA
                alpha = maintex.a * mask;
            #else
                half mainBrightness = maintex.r * 0.3 + maintex.g * 0.59 + maintex.b * 0.11;
                alpha = mainBrightness * mask;
            #endif

            finalcolor = lerp(opaque, finalcolor, alpha);
            alpha = _BaseColor.a * i.color.a;

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
            #pragma shader_feature _ _USE_MAIN_ALPHA
            ENDHLSL
        }
    }
}

