Shader "XinY/Bridge"
{
    Properties
    {
        _NoiseTex ("NoiseTex", 2D) = "black" { }
        _FlowColor ("FlowColor", color) = (0, 0, 0, 0)
        _AlphaFlowSpeed ("AlphaFlowSpeed", vector) = (0, 0, 0, 0)
        _AlphaTilling ("AlphaTilling", vector) = (0, 0, 0, 0)
        _FlowSpeed ("FlowSpeed", vector) = (0, 0, 0, 0)
        _FlowTilling ("FlowTilling", vector) = (0, 0, 0, 0)
        _ShapeTilling ("ShapeTilling", vector) = (0, 0, 0, 0)
        _ShapeRadius ("ShapeRadius", float) = 0
        _TransformSpeedMinAndMax ("TransformSpeedMinAndMax", vector) = (0, 0, 0, 0)
        [HDR]_ShapeColor ("ShapeColor", color) = (0, 0, 0, 0)
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
            float4 positionOS : POSITION;
            float2 texcoord : TEXCOORD0;
        };
        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD1;
        };


        CBUFFER_START(UnityPerMaterial)
            float4 _FlowColor;
            float2 _AlphaFlowSpeed;
            float2 _AlphaTilling;
            float2 _FlowSpeed;
            float2 _FlowTilling;
            float2 _ShapeTilling;
            float _ShapeRadius;
            float2 _TransformSpeedMinAndMax;
            float4 _ShapeColor;
        CBUFFER_END
        TEXTURE2D(_NoiseTex);
        SAMPLER(sampler_NoiseTex);
        float3 _HitPos;
        TEXTURE2D(_InteractRT);
        SAMPLER(sampler_InteractRT);
        float _InvEquivalentTexSize;

        ENDHLSL

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert(appdata v)
            {
                v2f output = (v2f)0;;
                output.positionCS = TransformObjectToHClip(v.positionOS);
                output.positionWS = TransformObjectToWorld(v.positionOS);
                output.uv = v.texcoord;
                return output;
            }

            float sdf(float2 p, float r)
            {
                float3 k = float3(-0.866025404, 0.5, 0.577350269);
                p = abs(p);
                p -= 2.0 * min(dot(k.xy, p), 0.0) * k.xy;
                p -= float2(clamp(p.x, -k.z * r, k.z * r), r);
                return length(p) * sign(p.y);
            }


            float4 frag(v2f i) : SV_TARGET
            {
                float4 finalColor = 0;
               
                float2 sdfUV = i.uv * _ShapeTilling;
                //插缝
                float bias = step(0.5, frac(sdfUV / 2.0).y) * 0.5;
                float2 biasUV = float2(sdfUV.x + bias, sdfUV.y);
                sdfUV = biasUV;
                sdfUV = RotateUV(frac(sdfUV) - 0.5, -0.166667 * 3.1415926) * 2;

                //随机变化
                float randomFactor = 0;
                float2 floorUV = floor(biasUV);
                float factor = RandomRange(floorUV, 0, _ShapeRadius);
                float random = RandomRange(floor(biasUV) + 0.09, _TransformSpeedMinAndMax.x, _TransformSpeedMinAndMax.y);
                random = abs(frac(random * _Time.y) - 0.5) * 4 - 1;
                random = random * _ShapeRadius - factor;
                randomFactor = random + factor;
                float shape = step(sdf(sdfUV, randomFactor), 0);

                //交互
                float2 interactUV=_InvEquivalentTexSize*(i.positionWS.xz-_HitPos.xz)+0.5;
                float interact=SAMPLE_TEXTURE2D(_InteractRT, sampler_InteractRT, interactUV).r;

                //颜色
                float2 flowUV=i.uv*_FlowTilling+_Time.y*_FlowSpeed*0.5;
                float noise=SAMPLE_TEXTURE2D(_NoiseTex,sampler_NoiseTex,flowUV).r;
                finalColor.rgb=noise*_FlowColor+shape*_ShapeColor;

                //alpha
                float2 alphaUV=i.uv*_AlphaTilling+_Time.y*_AlphaFlowSpeed;
                float alphaNoise=SAMPLE_TEXTURE2D(_NoiseTex,sampler_NoiseTex,alphaUV).r*0.3;
                finalColor.a=clamp(shape*0.5*(interact+alphaNoise)+_FlowColor.a*noise,0,1);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}
