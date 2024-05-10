Shader "XinY/PBR_Base_AlphaClip"
{
    Properties
    {
        _BaseMap ("BaseMap", 2D) = "white" { }
        _BaseColor ("BaseColor", color) = (1, 1, 1, 1)
        _MRA ("M(Metallic)R(Roughness)A(AO)", 2D) = "white" { }
        _MetallicAd ("MetallicAd", Range(0, 2)) = 1
        _RoughnessAd ("RoughnessAd", Range(0, 2)) = 1
        _AOAd ("AOAd", Range(0, 2)) = 1
        _NormalMap ("NormalMap", 2D) = "bump" { }
        _NormalScale ("NormalScale", Range(0, 5)) = 1
        _EmissionMap ("EmissionMap", 2D) = "black" { }
        [HDR]_EmissionColor ("Emission", color) = (0, 0, 0, 1)
        _GIIntensity ("GIIntensity", Range(0, 1)) = 1
        [Toggle(FOG_ON)]_FOG_ON ("Enable Fog", int) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "AlphaTest" }
        Cull Off
        HLSLINCLUDE
        // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
        #pragma multi_compile _ LIGHTMAP_ON
        //聚光灯阴影
        #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
        //先别用实时GI
        //#pragma multi_compile _ DYNAMICLIGHTMAP_ON

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
            float4 tangentOS : TANGENT;
            float2 texcoord : TEXCOORD0;
            float2 texcoord2 : TEXCOORD1;
            float2 staticLightmapUV : TEXCOORD1;
        };
        struct v2f
        {
            float2 uv : TEXCOORD0;
            float3 positionWS : TEXCOORD1;
            float3 normalWS : TEXCOORD2;
            half3 tangentWS : TEXCOORD3;
            #ifdef LIGHTMAP_ON
                float2 staticLightmapUV : TEXCOORD5;
            #endif
            float4 positionCS : SV_POSITION;
            float4 positionNDC : TEXCOORD6;
            float3 positionVS : TEXCOORD7;
            half3 binormalWS : TEXCOORD8;
            half3 normalOS : TEXCOORD9;
        };


        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _MetallicAd;
            half _AOAd;
            half _RoughnessAd;
            half4 _EmissionColor;
            half _NormalScale;
            half _FlowSpeed;
            half _GIIntensity;
        CBUFFER_END
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_MRA);
        SAMPLER(sampler_MRA);
        TEXTURE2D(_NormalMap);
        SAMPLER(sampler_NormalMap);
        TEXTURE2D(_EmissionMap);
        SAMPLER(sampler_EmissionMap);
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
                //法线获取
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalOS = input.normalOS;
                output.normalWS = normalInput.normalWS;;
                output.tangentWS = normalInput.tangentWS;
                output.binormalWS = normalInput.bitangentWS;
                //UV相关
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                //计算采样lightmap的UV
                OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
                return output;
            }

            float4 frag(v2f i) : SV_TARGET
            {
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                half3 MRA = SAMPLE_TEXTURE2D(_MRA, sampler_MRA, i.uv).rgb;
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv), _NormalScale);
                
                half3x3 TBN = half3x3(i.tangentWS, i.binormalWS, i.normalWS);
                half3 N = mul(normalTS, TBN);
                float2 screenUV = GetNormalizedScreenSpaceUV(i.positionCS);
                
                SurfaceAttrib attrib;
                attrib.baseColor = baseMap;
                attrib.metallic = lerp(0, MRA.x, _MetallicAd);
                attrib.roughness = lerp(1, MRA.y, _RoughnessAd);
                attrib.alpha = baseMap.a;
                clip(attrib.alpha - 0.5);
                half occlusion = lerp(1, MRA.z, _AOAd);

                AOPara aoFactor = GetAOPara(screenUV, occlusion);

                half4 shadowMask = unity_ProbesOcclusion;
                
                float4 shadowCoord = XinY_GetShadowCoord(i.positionCS, i.positionWS);
                Light light = GetMainLight(shadowCoord, i.positionWS, shadowMask, aoFactor.directAO);

                DataNeeded data;
                #ifdef LIGHTMAP_ON
                    data = CalculateDataNeeded(N, i.positionWS, i.staticLightmapUV, light, attrib);
                #else
                    data = CalculateDataNeeded(N, i.positionWS, 0, light, attrib);
                #endif

                half3 mainLightColor = 0;
                half3 additionLightColor = 0;
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, i.uv) * _EmissionColor;
                half3 emissionColor = emission;
                
                mainLightColor = GetOneLightPBRColor(data, attrib, aoFactor);

                //额外灯
                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                Light addLight = GetAdditionalLight(lightIndex, i.positionWS, shadowMask);
                SetAddLightData(data, addLight, i.positionWS);
                additionLightColor += GetOneLightPBRColor(data, attrib, aoFactor);
                LIGHT_LOOP_END

                half4 finalColor = 0;
                finalColor.a = attrib.alpha;
                finalColor.rgb = mainLightColor + additionLightColor + emissionColor;
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

                output.uv = TRANSFORM_TEX(v.texcoord, _BaseMap);
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
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                clip(baseMap.a - 0.5);
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
                output.uv = TRANSFORM_TEX(v.texcoord, _BaseMap);
                output.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                return output;
            }
            half4 frag(v2f i) : SV_TARGET
            {
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                clip(baseMap.a - 0.5);
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
                output.uv = TRANSFORM_TEX(v.uv0, _BaseMap);
                #ifdef EDITOR_VISUALIZATION
                    UnityEditorVizData(v.positionOS.xyz, v.uv0, v.uv1, v.uv2, output.VizUV, output.LightCoord);
                #endif
                return output;
            }
            half4 frag(Varyings input) : SV_Target
            {
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                clip(baseMap.a - 0.5);
                half4 MRA = SAMPLE_TEXTURE2D(_MRA, sampler_MRA, input.uv);
                
                //BrdfData
                half metallic = max(lerp(0.04, MRA.r, _MetallicAd), 0);
                half oneMinusReflectivity = OneMinusReflectivityMetallic(metallic);
                half3 albedo = baseMap.rgb;
                half3 diffuse = albedo * oneMinusReflectivity;;
                half3 specular = lerp(kDieletricSpec.rgb, albedo, metallic);
                half perceptualRoughness = lerp(0, MRA.g, _RoughnessAd);
                half roughness = max(perceptualRoughness * perceptualRoughness, HALF_MIN_SQRT);
                half3 emission = _EmissionColor * SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv);

                UnityMetaInput metaInput;
                metaInput.Albedo = diffuse + specular * roughness * 0.5;
                metaInput.Emission = emission;
                return UnityMetaFragment(metaInput);
            }
            ENDHLSL
        }
    }
    CustomEditor "TestShaderGUI"

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
