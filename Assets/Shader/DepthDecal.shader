Shader "Unlit/DepthDecal"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "black" { }
        [HDR]_BaseColor ("BaseColor", color) = (1, 1, 1, 1)

        [Enum(UnityEngine.Rendering.CullMode)]_Cull ("Cull", int) = 0
        [Enum(Off, 0, On, 1)]_ZWrite ("ZWrite", int) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest ("ZTest", int) = 4
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        
        HLSLINCLUDE
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
        //聚光灯阴影
        #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "../Shader/Include/XinY_Include_URP.hlsl"

        #define smp sampler_Clamp_Linear
        
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
        CBUFFER_END
        TEXTURE2D(_MainTex);
        SAMPLER(smp);
        TEXTURE2D(_CameraDepthTexture);
        SAMPLER(sampler_CameraDepthTexture);


        struct appdata
        {
            half4 positionOS : POSITION;
        };

        struct v2f
        {
            float4 positionCS : SV_POSITION;
            float3 positionVS : TEXCOORD2;
            float3 positionWS : TEXCOORD3;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.positionWS = TransformObjectToWorld(v.positionOS);
            o.positionVS = TransformWorldToView(o.positionWS);
            o.positionCS = mul(UNITY_MATRIX_P, float4(o.positionVS, 1));
            return o;
        }

        half4 frag(v2f i) : SV_Target
        {
            half3 finalcolor = _BaseColor.rgb;
            half alpha = _BaseColor.a;

            float2 screenUV = GetNormalizedScreenSpaceUV(i.positionCS);
            float depth = LinearEyeDepth(SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV).x, _ZBufferParams);
            //注意，VS坐标z值为负

            float4 rPositionVS = 1;
            rPositionVS.z = depth;
            rPositionVS.xy = -i.positionVS.xy * depth / i.positionVS.z;
            float3 rPositionWS = mul(unity_CameraToWorld, rPositionVS);
            float4 rPositionCS = TransformWorldToHClip(rPositionWS);
            float3 rPositionOS = mul(unity_WorldToObject, float4(rPositionWS, 1));
            float2 uv = rPositionOS.xz+0.5;

            float4 baseColor = SAMPLE_TEXTURE2D(_MainTex, smp, uv);
            finalcolor *= baseColor.xyz;
            alpha *= baseColor.a;
            float4 shadowCoord = XinY_GetShadowCoord(rPositionCS, rPositionWS);
            Light light = GetMainLight(shadowCoord);
            float shadow=max(0.3,light.shadowAttenuation);
            finalcolor*=light.color*light.distanceAttenuation*shadow;
            //return frac(uv.xyxx);
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
