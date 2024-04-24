Shader "XinY/Par/Fresnel"
{
    Properties
    {
        _MainTex("MainTex",2D)="black"{}
        _FlowSpeed("FlowSpeed",vector)=(1,1,0,0)
        [HDR]_BaseColor ("BaseColor", Color) = (1, 1, 1, 1)
        _FresnelBias("FresnelBias",Range(-1,1))=0
        _FresnelScale("FresnelScale",float)=1
        _FresnelPower("FresnelPower",float)=1
        _JunctionRange("JunctionRange",Range(0,1))=1     
        [HDR]_RimColor ("RimColor", Color) = (1, 1, 1, 1)

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
            half4 _RimColor;
            half _FresnelBias;
            half _FresnelPower;
            half _FresnelScale;
            half _JunctionRange;
            half2 _FlowSpeed;
        CBUFFER_END
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);        
        TEXTURE2D(_CameraDepthTexture);
        SAMPLER(sampler_CameraDepthTexture);

        struct appdata
        {
            half4 positionOS : POSITION;
            float2 uv0 : TEXCOORD0;
            half4 color : COLOR;
            half3 normal:NORMAL;
        };

        struct v2f
        {
            float2 uv0 : TEXCOORD0;
            half4 color : TEXCOORD1;
            float4 positionCS : SV_POSITION;
            float3 positionVS:TEXCOORD2;
            float3 positionWS:TEXCOORD3;
            half3 normal:TEXCOORD4;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.positionWS=TransformObjectToWorld(v.positionOS);
            o.positionVS=TransformWorldToView(o.positionWS);
            o.positionCS=mul(UNITY_MATRIX_P,float4(o.positionVS,1));
            o.uv0 = TRANSFORM_TEX(v.uv0.xy, _MainTex);
            o.color = v.color;
            o.normal=TransformObjectToWorldNormal(v.normal);
            return o;
        }

        half4 frag(v2f i) : SV_Target
        {
            half3 finalcolor = _BaseColor.rgb;
            half alpha = _BaseColor.a;

            float2 screenUV = GetNormalizedScreenSpaceUV(i.positionCS);
            float depth = LinearEyeDepth(SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV).x, _ZBufferParams);
            //注意，VS坐标z值为负
            float depthFade=depth+i.positionVS.z;
            half junctionMask=1-smoothstep(0,_JunctionRange*5,depthFade);
            //return junctionMask;
            half3 V = normalize(_WorldSpaceCameraPos - i.positionWS);
            half3 N=normalize(i.normal);
            half fresnelMask=1-abs(dot(N,V));
            fresnelMask=clamp(pow(fresnelMask+_FresnelBias,_FresnelPower)*_FresnelScale,0,1);
            half nosie=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv0+_Time.y*_FlowSpeed);
            half rimMask=clamp(fresnelMask+junctionMask-nosie,0,1);

            
            finalcolor = finalcolor *  i.color.rgb;
            finalcolor=lerp(finalcolor,_RimColor,rimMask);
            alpha = lerp(alpha,1,rimMask)*i.color.a;
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

