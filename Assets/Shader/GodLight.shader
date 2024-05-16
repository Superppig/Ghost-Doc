Shader "XinY/Fresnel"
{
    Properties
    {
        [HDR]_BaseColor ("BaseColor", Color) = (1, 1, 1, 1)
        _FresnelBias ("FresnelBias", Range(-1, 1)) = 0
        _FresnelScale ("FresnelScale", float) = 1
        _FresnelPower ("FresnelPower", float) = 1
        _JunctionRange ("JunctionRange", Range(0, 1)) = 1
        _HeightMinMax ("HeightMinMax", vector) = (0, 1, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half _FresnelBias;
            half _FresnelPower;
            half _FresnelScale;
            half _JunctionRange;
            float2 _HeightMinMax;
        CBUFFER_END
        TEXTURE2D(_CameraDepthTexture);
        SAMPLER(sampler_CameraDepthTexture);

        struct appdata
        {
            half4 positionOS : POSITION;
            float2 uv0 : TEXCOORD0;
            half4 color : COLOR;
            half3 normal : NORMAL;
        };

        struct v2f
        {
            float2 uv0 : TEXCOORD0;
            half4 color : TEXCOORD1;
            float4 positionCS : SV_POSITION;
            float3 positionVS : TEXCOORD2;
            float3 positionWS : TEXCOORD3;
            half3 normal : TEXCOORD4;
            float3 positionOS : TEXCOORD5;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.positionOS = v.positionOS;
            o.positionWS = TransformObjectToWorld(v.positionOS);
            o.positionVS = TransformWorldToView(o.positionWS);
            o.positionCS = mul(UNITY_MATRIX_P, float4(o.positionVS, 1));
            o.uv0 = v.uv0.xy;
            o.color = v.color;
            o.normal = TransformObjectToWorldNormal(v.normal);
            return o;
        }

        half4 frag(v2f i) : SV_Target
        {
            float2 screenUV = GetNormalizedScreenSpaceUV(i.positionCS);
            float depth = LinearEyeDepth(SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV).x, _ZBufferParams);
            //注意，VS坐标z值为负
            float depthFade = depth + i.positionVS.z;
            half junctionMask = smoothstep(0, _JunctionRange * 5, depthFade);
            half3 V = normalize(_WorldSpaceCameraPos - i.positionWS);
            half3 N = normalize(i.normal);
            half fresnelMask = abs(dot(N, V));
            fresnelMask = clamp(pow(fresnelMask + _FresnelBias, _FresnelPower) * _FresnelScale, 0, 1);
            
            half heightMask = smoothstep(_HeightMinMax.x, _HeightMinMax.y, i.positionOS.z);

            half alpha = fresnelMask * junctionMask * heightMask*_BaseColor.a;
            half3 color=_BaseColor.rgb;
            
            return half4(color.xyz, alpha);
        }
        
        ENDHLSL
        Pass
        {
            ZWrite Off
            Cull Off
            Blend SrcAlpha One
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}

