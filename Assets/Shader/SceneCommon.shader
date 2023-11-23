
Shader "XinY/SceneCommon"
{
    Properties
    {
        _SpecExpon ("SpecExpon", float) = 1
        _BaseColor ("BaseColor", color) = (0.3, 0.3, 0.3, 1)
        _NormalMap ("NormalMap", 2D) = "bump" { }
        [HDR]_RimColor ("RimColor", Color) = (1, 1, 1, 1)
        _RimOffset ("RimOffset", float) = 6
        _RimThreshold ("RimThreshold", Range(0,1)) = 0.03
        _RimStrength ("RimStrength", float) = 0.6
        _RimMax ("RimMax", float) = 0.3
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 100
        AlphaTest Off

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            
            Blend One Zero
            

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 posOS : POSITION;
                float2 uv : TEXCOORD0;
                half3 normalOS : NORMAL;
                half4 tangentOS : TANGENT;
            };

            struct v2f
            {
                float4 posCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 posWS : TEXCOORD1;
                float3 posVS : TEXCOORD2;
                float4 posNDC : TEXCOORD3;
                float3 normalWS : TEXCOORD4;
                float3 tangentWS : TEXCOORD5;
                float3 bitangentWS : TEXCOORD6;
                float shadowCoord : TEXCOORD7;
            };


            CBUFFER_START(UnityPerMaterial)
                half4 _RimColor;
                half4 _BaseColor;
                half _SpecExpon;
                float _RimMax;
                float _RimOffset;
                float _RimStrength;
                float _RimThreshold;
            CBUFFER_END
            TEXTURE2D(_NormalMap);    SAMPLER(sampler_NormalMap);
            TEXTURE2D(_CameraDepthTexture);SAMPLER(sampler_CameraDepthTexture);
            
            v2f vert(appdata v)
            {
                v2f o;
                //各类空间坐标
                o.posWS = TransformObjectToWorld(v.posOS.xyz);
                o.posVS = TransformWorldToView(o.posWS);
                o.posCS = TransformObjectToHClip(v.posOS);
                float4 ndc = o.posCS * 0.5f;
                o.posNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
                o.posNDC.zw = o.posCS.zw;
                //法线内容
                real sign = real(v.tangentOS.w) * (unity_WorldTransformParams.w >= 0.0 ? 1.0 : - 1.0);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.tangentWS = real3(TransformObjectToWorldDir(v.tangentOS.xyz));
                o.bitangentWS = real3(cross(o.normalWS, float3(o.tangentWS))) * sign;

                o.uv = v.uv;
                o.shadowCoord = TransformWorldToShadowCoord(o.posWS);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                Light light = GetMainLight(i.shadowCoord);

                float4 normalMap = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv);
                float3 normalTS = UnpackNormal(normalMap);
                
                float3x3 TBN = float3x3(i.tangentWS, i.bitangentWS, i.normalWS);
                //float3 N=normalize(mul(normalTS,TBN));
                float3 N = i.normalWS;
                //NEW视角方向，将视空间的坐标转为世界空间，OLD相机位置-模型位置
                float3 V = normalize(mul((float3x3)UNITY_MATRIX_I_V, i.posVS * (-1)));
                float3 L = normalize(light.direction);
                float3 H = normalize(L + V);

                float NdotL = dot(N, L);//lambert
                float NdotH = dot(N, H);//blinphong
                float NdotV = dot(N, V);//fresnel

                float3 normalVS = normalize(mul((float3x3)UNITY_MATRIX_V, N));
                float2 matcapUV = normalVS.xy * 0.5 + 0.5;

                //固有色
                float lambert = max(0, NdotL);

                //高光,亮部才有高光
                float blinnPhong = step(0, NdotL) * pow(max(0, NdotH), _SpecExpon);
                //总混合
                float3 albedo = (_BaseColor * max(lambert,light.shadowAttenuation) + blinnPhong) * light.color;

                //屏幕空间边缘光
                float rimOffset = _RimOffset;
                float rimThreshold = _RimThreshold;
                float rimStrength = _RimStrength;
                float rimMax = _RimMax;

                float2 screenUV = i.posNDC.xy / i.posNDC.w;
                float rawDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV);//获取深度图
                float linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                
                float2 screenOffset=float2(lerp(-1,1,step(0,normalVS.x))
                                            *rimOffset/ max(1, pow(linearDepth, 2))/ _ScreenParams.x
                                            ,lerp(-1,1,step(0,normalVS.y))
                                            *rimOffset/ max(1, pow(linearDepth, 2))/ _ScreenParams.y);
                
                //获得偏移后的深度图
                float offsetDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV + screenOffset);
                float offsetLinearDepth = LinearEyeDepth(offsetDepth, _ZBufferParams);
                //return offsetLinearDepth * 0.01;
                //确定发光范围
                float rim = saturate(offsetLinearDepth - linearDepth);
                rim = step(rimThreshold, rim) * clamp(rim * rimStrength, 0, rimMax);
                //return float4(screenOffset+screenUV,0,1);
                //return saturate(400*offsetLinearDepth - 400*linearDepth);
                float alpha = 1;
                albedo=lerp(albedo,_RimColor,rim);
                float4 col = float4(albedo, alpha);

                return col;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            // -------------------------------------
            // Universal Pipeline keywords

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
}