Shader "XinY/BuildingWindow_LineScan"
{
    Properties
    {
        _PositionAdjust ("PositionAdjust", float) = 1
        _HeightFlowSpeed ("HeightFlowSpeed", Range(0, 1)) = 0.5
        _HeightMaskPow("HeightMaskPow",Range(0.01,6))=5
        _MetallicAd ("MetallicAd", Range(0, 1)) = 1
        _RoughnessAd ("RoughnessAd", Range(0, 1)) = 1
        _AOAd ("AOAd", Range(0, 1)) = 1
        _BackGroundColor ("BackGroundColor", color) = (0, 0, 0, 1)
        _ScanNum ("ScanNum", Range(0, 500)) = 240
        _ScanSpeed ("ScanSpeed", Range(-2, 2)) = 0.5
        _ScanWidth ("ScanWidth", Range(0, 1)) = 0.5
        [HDR]_ScanColor0 ("ScanColor0", color) = (1, 1, 1, 1)
        [HDR]_ScanColor1 ("ScanColor1", color) = (1, 1, 1, 1)
        [Toggle(FOG_ON)]_FOG_ON ("Enable Fog", int) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        HLSLINCLUDE

        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS


        #pragma shader_feature _ FOG_ON
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "../Shader/Include/XinY_Include_URP.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
        #include "../Shader/Include/XinY_PBR_Include.hlsl"

        #include "../Shader/Include/XinY_Fog_Include.hlsl"

        struct appdata
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 texcoord : TEXCOORD0;
            float2 staticLightmapUV : TEXCOORD1;
        };
        struct v2f
        {
            float2 uv : TEXCOORD0;
            float3 positionWS : TEXCOORD1;
            float3 normalWS : TEXCOORD2;
            DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 5);
            float4 positionCS : SV_POSITION;
            float4 positionNDC : TEXCOORD6;
            float3 positionVS : TEXCOORD7;
            float3 positionOS : TEXCOORD8;
        };


        CBUFFER_START(UnityPerMaterial)
            half _MetallicAd;
            half _AOAd;
            half _RoughnessAd;
            half _ScanNum;
            half _ScanSpeed;
            half _ScanWidth;
            float4 _ScanColor0;
            float4 _ScanColor1;
            float4 _BackGroundColor;
            float _PositionAdjust;
            float _HeightFlowSpeed;
            float _HeightMaskPow;
        CBUFFER_END

        ENDHLSL

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            v2f vert(appdata input)
            {
                v2f output = (v2f)0;;
                //坐标获取
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionWS = vertexInput.positionWS;
                output.positionVS = vertexInput.positionVS;
                output.positionCS = vertexInput.positionCS;
                output.positionNDC = vertexInput.positionNDC;
                output.positionOS = input.positionOS;
                //法线获取
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                //UV相关
                output.uv = input.texcoord;
                //计算采样lightmap的UV
                OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
                return output;
            }

            float4 frag(v2f i) : SV_TARGET
            {
                float scanBaseValue = i.uv.x * _ScanNum;
                scanBaseValue = floor(scanBaseValue);
                scanBaseValue = RandomRange(float2(scanBaseValue, 1), 0, 1);
                float scanBase = smoothstep(scanBaseValue, scanBaseValue + _ScanWidth, frac(i.uv.y - _ScanSpeed * _Time.y * RandomRange(float2(scanBaseValue, 1), 0.5, 2)));
                scanBase = scanBase * step(scanBase, 0.999) ;
                float heightMask = i.positionOS.y / _PositionAdjust;
                float seqTime = abs((frac(_Time.y * _HeightFlowSpeed) - 0.5) * 2);
                float heightFlow = saturate(distance(heightMask, seqTime));
                heightFlow = 1 - heightFlow;
                heightFlow = pow(heightFlow, _HeightMaskPow);

                float3 flowColor = lerp(_ScanColor0, _ScanColor1, heightFlow);

                half3 scanColor = lerp(0, flowColor, scanBase);
                float3 emissionColor = scanColor;

                float2 screenUV = GetNormalizedScreenSpaceUV(i.positionCS);
                
                SurfaceAttrib attrib;
                attrib.baseColor = _BackGroundColor;
                attrib.metallic = _MetallicAd;
                attrib.roughness = _RoughnessAd;
                attrib.alpha = _BackGroundColor.a;
                half occlusion = _AOAd;
                float3 N = normalize(i.normalWS);


                AOPara aoFactor = GetAOPara(screenUV, occlusion);

                half4 shadowMask = unity_ProbesOcclusion;
                
                float4 shadowCoord = XinY_GetShadowCoord(i.positionCS, i.positionWS);
                Light light = GetMainLight(shadowCoord, i.positionWS, shadowMask, aoFactor.directAO);

                DataNeeded data;
                #ifdef LIGHTMAP_ON
                    data = CalculateDataNeeded(N, i.positionWS, i.staticLightmapUV, light, attrib, true);
                #else
                    data = CalculateDataNeeded(N, i.positionWS, 0, light, attrib, true);
                #endif

                half3 mainLightColor = 0;
                half3 additionLightColor = 0;
                
                mainLightColor = GetOneLightPBRColor(data, attrib, aoFactor);

                //额外灯
                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                Light addLight = GetAdditionalLight(lightIndex, i.positionWS, shadowMask);
                SetAddLightData(data, addLight, i.positionWS);
                additionLightColor += GetDirectLightPBRColor(data, attrib, aoFactor);
                LIGHT_LOOP_END

                half4 finalColor = 0;
                finalColor.a = attrib.alpha;
                finalColor.rgb = mainLightColor + additionLightColor + emissionColor ;
                #ifdef FOG_ON
                    finalColor.rgb = XinY_MixFog(finalColor.rgb, i.positionWS);
                #endif

                return finalColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            HLSLPROGRAM
            #pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma vertex vert
            #pragma fragment frag
            float3 _LightDirection;
            float3 _LightPosition;
            v2f vert(appdata v)
            {
                v2f output;

                output.uv = v.texcoord;
                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);

                //pragma这个变体，然后聚光灯开启阴影这个变体就会自动启动
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return output;
            }
            half4 frag(v2f i) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert(appdata v)
            {
                v2f output = (v2f)0;
                output.uv = v.texcoord;
                output.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                return output;
            }
            half4 frag(v2f i) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }

            Cull Off

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/MetaPass.hlsl"
            #pragma vertex vert
            #pragma fragment frag
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                #ifdef EDITOR_VISUALIZATION
                    float2 VizUV : TEXCOORD1;
                    float4 LightCoord : TEXCOORD2;
                #endif
            };
            Varyings vert(Attributes v)
            {
                Varyings output = (Varyings)0;
                output.positionCS = UnityMetaVertexPosition(v.positionOS.xyz, v.uv1, v.uv2);
                output.uv = v.uv0;
                #ifdef EDITOR_VISUALIZATION
                    UnityEditorVizData(v.positionOS.xyz, v.uv0, v.uv1, v.uv2, output.VizUV, output.LightCoord);
                #endif
                return output;
            }
            half4 frag(Varyings input) : SV_Target
            {
                //BrdfData
                half metallic = _MetallicAd;
                half oneMinusReflectivity = OneMinusReflectivityMetallic(metallic);
                half3 albedo = _BackGroundColor;
                half3 diffuse = albedo * oneMinusReflectivity;;
                half3 specular = lerp(kDieletricSpec.rgb, albedo, metallic);
                half perceptualRoughness = _RoughnessAd;
                half roughness = max(perceptualRoughness * perceptualRoughness, HALF_MIN_SQRT);

                UnityMetaInput metaInput;
                metaInput.Albedo = diffuse + specular * roughness * 0.5;
                metaInput.Emission = 0;
                return UnityMetaFragment(metaInput);
            }
            ENDHLSL
        }
    }
    CustomEditor "TestShaderGUI"

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}