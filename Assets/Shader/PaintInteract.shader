Shader "XinY/PaintInteract"
{
    Properties
    {
        //_PrevRT ("_PrevRT", 2D) = "black" { }
        //_InteractRT ("_InteractRT", 2D) = "black" { }
        _MainTex ("_MainTex", 2D) = "black" { }
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
        CBUFFER_START(UnityPerMaterial)

        CBUFFER_END
        TEXTURE2D(_PrevRT);
        SAMPLER(sampler_PrevRT);
        TEXTURE2D(_InteractRT);
        SAMPLER(sampler_InteractRT);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        float4 _HitPos;
        float4 _MoveDir;
        float _InvEquivalentTexSize;
        float _EquivalentRange;
        float _FadeSpeed;

        struct appdata
        {
            half4 posOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            half4 posCS : SV_POSITION;
            half3 posWS : TEXCOORD0;
            float2 uv : TEXCOORD1;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.posWS = TransformObjectToWorld(v.posOS);
            o.posCS = TransformWorldToHClip(o.posWS);
            o.uv = v.uv;
            return o;
        }

        half4 frag(v2f i) : SV_Target
        {
            half prev = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + _MoveDir.xz * _InvEquivalentTexSize).r;
            half factor = (1 / _InvEquivalentTexSize) / _EquivalentRange;
            half data = clamp(distance((i.uv - 0.5) * factor + 0.5, half2(0.5, 0.5)) * 2, 0, 1);
            half cur = clamp(1 - data, 0, 1);
            cur=smoothstep(0,0.5,cur);
            cur=cur+prev;
            float cond = step(abs(i.uv.x - 0.5), 0.499) * step(abs(i.uv.y - 0.5), 0.499);
            cur = clamp(cur*cond-_FadeSpeed,0,1);

            return half4(cur , 0, 0, 0);
        }
        
        ENDHLSL
        Pass
        {
            ZTest Always
            Cull Off
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            ENDHLSL
        }
    }
}

