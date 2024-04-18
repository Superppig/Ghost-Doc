Shader "XinY/HorizontalPlane"
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
        _SpecIntensity ("SpecIntensity", float) = 1
        _ReflectDistort ("ReflectDistort", Range(0, 0.2)) = 0.1
        _ReflectIntensity("ReflectIntensity",float)=1
        _DistortMap ("DistortMap", 2D) = "black" { }
        _EmissionMap ("EmissionMap", 2D) = "black" { }
        [HDR]_EmissionColor ("Emission", color) = (0, 0, 0, 1)
        [Toggle(EMISS_FLOW)]_EMISS_FLOW ("EmissFlow", int) = 0
        [Toggle(EMISS_FLOW_Y)]_EMISS_FLOW_Y ("Y Flow", int) = 0
        _FlowPara ("FlowWidth Contrast SpeedXY", vector) = (0, 1, 0, 0)
        [Toggle(FOG_ON)]_FOG_ON ("Enable Fog", int) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

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
        #pragma shader_feature _ EMISS_FLOW
        #pragma shader_feature _ EMISS_FLOW_Y
        #define _REFLECTPLANE 1
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
            float2 dynamicLightmapUV : TEXCOORD2;
        };
        struct v2f
        {
            float2 uv : TEXCOORD0;
            float3 positionWS : TEXCOORD1;
            float3 normalWS : TEXCOORD2;
            half3 tangentWS : TEXCOORD3;
            DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 5);
            float4 positionCS : SV_POSITION;
            float4 positionNDC : TEXCOORD6;
            float3 positionVS : TEXCOORD7;
            half3 binormalWS : TEXCOORD8;
            half3 normalOS : TEXCOORD9;
            float2 ori_uv : TEXCOORD10;
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
            half _SpecIntensity;
            float4 _DistortMap_ST;
            half _ReflectDistort;
            half4 _FlowPara;
            float _ReflectIntensity;
        CBUFFER_END
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_MRA);
        SAMPLER(sampler_MRA);
        TEXTURE2D(_NormalMap);
        SAMPLER(sampler_NormalMap);
        TEXTURE2D(_EmissionMap);
        SAMPLER(sampler_EmissionMap);
        TEXTURE2D(_DistortMap);
        SAMPLER(sampler_DistortMap);
        ENDHLSL

        Pass
        {
            Name "ReflectPlane"
            Tags { "LightMode" = "ReflectPlane" }
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
                output.ori_uv = input.texcoord;
                OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
                #ifdef DYNAMICLIGHTMAP_ON
                    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
                return output;
            }

            float4 frag(v2f i) : SV_TARGET
            {
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                half3 MRA = SAMPLE_TEXTURE2D(_MRA, sampler_MRA, i.uv).rgb;
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv), _NormalScale);
                
                // half3x3 TBN = half3x3(i.tangentWS, i.binormalWS, i.normalWS);
                // half3 V = normalize(_WorldSpaceCameraPos - i.positionWS);
                // half3 N = mul(normalTS, TBN);
                // half3 viewDirTS = XinY_GetViewDirectionTangentSpace(i.tangentWS, i.binormalWS, i.normalWS, V);
                // half3 R = reflect(-V, N);
                // half NdotV = saturate(dot(N, V));
                // half fresnelTerm = Pow4(1.0 - NdotV);
                // half NdotL = saturate(dot(N, light.direction));
                // half3 H = SafeNormalize(light.direction + V);
                // half NdotH = saturate(dot(N, H));
                // half LdotH = saturate(dot(light.direction, H));

                //float fogCoord = XinY_InitializeFog(float4(i.positionWS, 1.0));

                //GI
                //关于bakedGI，动态物体使用像素级别的sh，静态物体使用lightmap
                // half3 bakedGI = 0;
                // #if defined(DYNAMICLIGHTMAP_ON)
                //     bakedGI = SAMPLE_GI(i.staticLightmapUV, i.dynamicLightmapUV, i.vertexSH, N);
                // #else
                //     bakedGI = SAMPLE_GI(i.staticLightmapUV, i.vertexSH, N);
                // #endif

                float2 screenUV = GetNormalizedScreenSpaceUV(i.positionCS);
                //BrdfData
                BRDFData brdfData;
                half alpha = 1;
                half metallic = max(lerp(0.04, MRA.r, _MetallicAd), 0);
                half roughness = lerp(1, MRA.g, _RoughnessAd);
                half3 emission = _EmissionColor * SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, i.uv);
                half occlusion = max(lerp(0, MRA.z, _AOAd), 0);
                //AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(screenUV, occlusion);
                GetBRDFData(brdfData, alpha, metallic, roughness, baseMap);

                AOPara aoFactor = GetAOPara(screenUV, occlusion);

                //Shadowmask的烘焙模式才会使用
                //half4 shadowMask = SAMPLE_SHADOWMASK(i.staticLightmapUV);
                half4 shadowMask = unity_ProbesOcclusion;
                
                //ShadowMask烘焙模式或者烘焙阴影会用到shadowMask，联级阴影会用到posWS,开启联级阴影一定到在frag中计算shadowcoord
                float4 shadowCoord = XinY_GetShadowCoord(i.positionCS, i.positionWS);
                Light light = GetMainLight(shadowCoord, i.positionWS, shadowMask, aoFactor.directAO);
                //return light.shadowAttenuation;
                Direct_Dot_Data dataNeed;
                GetDirDotData(dataNeed, i.tangentWS, i.binormalWS, i.normalWS, i.positionWS, normalTS, light.direction);
                ////////////////////////////待测试
                //MixRealtimeAndBakedGI(light, i.normalWS, bakedGI);
                ////////////////////////////////////

                //LightingData
                half3 giColor = 0;
                half3 mainLightColor = 0;
                half3 additionLightColor = 0;
                half3 emissionColor = emission;
                #ifdef EMISS_FLOW
                    float2 flowUV = frac(i.ori_uv + _FlowPara.zw * _Time.y/_BaseMap_ST.xy);
                    #ifdef EMISS_FLOW_Y
                        half flowMask = 1 - 2 * abs(flowUV.y - 0.5);
                    #else
                        half flowMask = 1 - 2 * abs(flowUV.x - 0.5);
                    #endif
                    half2 m_para=half2(_FlowPara.x / max(_BaseMap_ST.x,_BaseMap_ST.y),_FlowPara.y);
                    m_para.y=Remap(m_para.y,0,1,0,1-m_para.x);
                    flowMask = smoothstep(m_para.x,1-m_para.y, flowMask);
                #else
                    half flowMask = 1;
                #endif
                
                half3 indirectDiffuse = GET_INDIRECT_DIFF(light, i.staticLightmapUV, i.vertexSH, dataNeed.N, brdfData.diffuse);
                return indirectDiffuse.xyzz*100;
                //return indirectDiffuse.xyzz;
                //间接高光的实现载体是反射探针
                //这里没有支持反射探针混合和盒投影
                //half3 indirectSpecular = GetIndirectSpecLight(brdfData, dataNeed);
                half2 distortDir = normalize(dataNeed.V.xz) - 0.5;
                half distort = SAMPLE_TEXTURE2D(_DistortMap, sampler_DistortMap, screenUV * _DistortMap_ST.xy + _DistortMap_ST.zw * _Time.y) * _ReflectDistort * (1 - dataNeed.fresnelTerm);
                float2 reflectUV = screenUV + distortDir * distort;
                half3 indirectSpecular = GetIndirectSpecLight(brdfData, 1, dataNeed.fresnelTerm, reflectUV)*_ReflectIntensity+GetIndirectSpecLight(brdfData,dataNeed.R,dataNeed.fresnelTerm)*0.3;
                //return indirectSpecular.xyzz;
                // half mip = perceptualRoughness * (1.7 - 0.7 * perceptualRoughness) * 6;
                // indirectSpecular = DecodeHDREnvironment(half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, R, mip)), unity_SpecCube0_HDR);
                // float surfaceReduction = 1.0 / (roughness2 + 1.0);
                // indirectSpecular *= half3(surfaceReduction * lerp(specular, grazingTerm, fresnelTerm));

                //混入indirectAO
                giColor = GetIndirectCol(indirectSpecular, indirectDiffuse, aoFactor.indirectAO);
                
                //return dataNeed.fresnelTerm;
                // half lightAtten = light.distanceAttenuation * light.shadowAttenuation;
                // half diffuseTerm = light.color * (lightAtten * NdotL);
                // float d = NdotH * NdotH * roughness2MinusOne + 1.00001f;
                // half LdotH2 = LdotH * LdotH;
                // half specularTerm = roughness2 / ((d * d) * max(0.1h, LdotH2) * normalizationTerm);
                
                // mainLightColor = clamp((diffuse + specular * specularTerm * _SpecIntensity) * diffuseTerm, 0, 1.5);
                
                //lightColor已经混合了ao
                mainLightColor = GetOneLightPBRCol(light, dataNeed, brdfData, _SpecIntensity);

                //额外灯
                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                Light addLight = GetAdditionalLight(lightIndex, i.positionWS, shadowMask);
                GetAddDirDotData(dataNeed, addLight.direction);
                additionLightColor += GetOneLightPBRCol(addLight, dataNeed, brdfData, _SpecIntensity);
                LIGHT_LOOP_END

                half4 finalColor = 0;
                finalColor.a = alpha;
                finalColor.rgb = giColor + mainLightColor + additionLightColor + emissionColor*flowMask;
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
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
