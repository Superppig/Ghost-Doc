
Shader "XinY/Scene_Standard"
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
        _EmissionMap ("EmissionMap", 2D) = "black" { }
        [HDR]_EmissionColor ("Emission", color) = (0, 0, 0, 1)
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 100
        AlphaTest Off

        HLSLINCLUDE
        // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE

        #pragma multi_compile _ LIGHTMAP_ON
        //先别用实时GI
        //#pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "../Shader/Include/XinY_Include_URP.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"

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
            float4 shadowCoord : TEXCOORD4;
            DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 5);
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
            half _SpecIntensity;
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

            v2f vert(appdata v)
            {
                v2f output = (v2f)0;;
                //坐标获取
                output.positionWS = TransformObjectToWorld(v.positionOS);
                output.positionVS = TransformWorldToView(output.positionWS);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                float4 ndc = output.positionCS * 0.5f;
                output.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
                output.positionNDC.zw = output.positionCS.zw;
                //法线获取
                output.normalOS = v.normalOS;
                real sign = real(v.tangentOS.w) * GetOddNegativeScale();
                output.normalWS = TransformObjectToWorldNormal(v.normalOS);
                output.tangentWS = real3(TransformObjectToWorldDir(v.tangentOS.xyz));
                output.binormalWS = real3(cross(output.normalWS, float3(output.tangentWS))) * sign;
                //UV相关
                output.uv = TRANSFORM_TEX(v.texcoord, _BaseMap);
                OUTPUT_LIGHTMAP_UV(v.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
                #ifdef DYNAMICLIGHTMAP_ON
                    output.dynamicLightmapUV = v.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
                //阴影
                //上面的方法可以算屏幕阴影
                output.shadowCoord = XinY_GetShadowCoord(output.positionCS, output.positionWS);
                return output;
            }

            float4 frag(v2f i) : SV_TARGET
            {
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                half3 MRA = SAMPLE_TEXTURE2D(_MRA, sampler_MRA, i.uv).rgb;
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv), _NormalScale);
                
                //Shadowmask的烘焙模式才会使用
                //half4 shadowMask = SAMPLE_SHADOWMASK(i.staticLightmapUV);
                half4 shadowMask = unity_ProbesOcclusion;
                
                //ShadowMask烘焙模式或者烘焙阴影会用到shadowMask，联级阴影会用到posWS,开启联级阴影一定到在frag中计算shadowcoord
                Light light = GetMainLight(XinY_GetShadowCoord(i.positionCS, i.positionWS), i.positionWS, 0);


                half3x3 TBN = half3x3(i.tangentWS, i.binormalWS, i.normalWS);
                half3 V = normalize(_WorldSpaceCameraPos - i.positionWS);
                half3 N = mul(normalTS, TBN);
                half3 viewDirTS = XinY_GetViewDirectionTangentSpace(i.tangentWS, i.binormalWS, i.normalWS, V);
                half3 R = reflect(-V, N);
                half NdotV = saturate(dot(N, V));
                half fresnelTerm = Pow4(1.0 - NdotV);
                half NdotL = saturate(dot(N, light.direction));
                half3 H = SafeNormalize(light.direction + V);
                half NdotH = saturate(dot(N, H));
                half LdotH = saturate(dot(light.direction, H));

                float fogCoord = XinY_InitializeFog(float4(i.positionWS, 1.0));

                //GI
                //关于bakedGI，动态物体使用sh，静态物体使用lightmap
                half3 bakedGI = 0;
                #if defined(DYNAMICLIGHTMAP_ON)
                    bakedGI = SAMPLE_GI(i.staticLightmapUV, i.dynamicLightmapUV, i.vertexSH, N);
                #else
                    bakedGI = SAMPLE_GI(i.staticLightmapUV, i.vertexSH, N);
                #endif

                half2 screenUV = GetNormalizedScreenSpaceUV(i.positionCS);
                
                //BrdfData
                half metallic = max(lerp(0.04, MRA.r, _MetallicAd), 0);
                half oneMinusReflectivity = OneMinusReflectivityMetallic(metallic);
                half3 albedo = baseMap.rgb;
                half3 diffuse = albedo * oneMinusReflectivity;;
                half3 specular = lerp(kDieletricSpec.rgb, albedo, metallic);;
                half reflectivity = half(1.0) - oneMinusReflectivity;
                half perceptualRoughness = lerp(0, MRA.g, _RoughnessAd);
                half smoothness = 1 - perceptualRoughness;
                half roughness = max(perceptualRoughness * perceptualRoughness, HALF_MIN_SQRT);
                half roughness2 = max(roughness * roughness, HALF_MIN);;
                half grazingTerm = saturate(smoothness + reflectivity);;
                half normalizationTerm = roughness * half(4.0) + half(2.0);;     // roughness * 4.0 + 2.0
                half roughness2MinusOne = roughness2 - half(1.0);;    // roughness^2 - 1.0
                half alpha = baseMap.a * oneMinusReflectivity + reflectivity;
                half3 emission = _EmissionColor*SAMPLE_TEXTURE2D(_EmissionMap,sampler_EmissionMap,i.uv);
                half occlusion = max(lerp(0, MRA.z, _AOAd), 0);
                //AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(screenUV, occlusion);


                MixRealtimeAndBakedGI(light, i.normalWS, bakedGI);

                //LightingData
                half3 giColor = bakedGI;
                half3 mainLightColor = 0;
                half3 additionLightColor = 0;
                half3 emissionColor = emission;
                
                half3 indirectDiffuse = bakedGI * diffuse;
                //间接高光的实现载体是反射探针
                //这里没有支持反射探针混合和盒投影
                half3 indirectSpecular = 0;
                half mip = perceptualRoughness * (1.7 - 0.7 * perceptualRoughness) * 6;
                indirectSpecular = DecodeHDREnvironment(half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, R, mip)), unity_SpecCube0_HDR);
                float surfaceReduction = 1.0 / (roughness2 + 1.0);
                indirectSpecular *= half3(surfaceReduction * lerp(specular, grazingTerm, fresnelTerm));
                giColor = (indirectDiffuse + indirectSpecular) * occlusion;

                half lightAtten = light.distanceAttenuation * light.shadowAttenuation;
                half diffuseTerm = light.color * (lightAtten * NdotL);
                float d = NdotH * NdotH * roughness2MinusOne + 1.00001f;
                half LdotH2 = LdotH * LdotH;
                half specularTerm = roughness2 / ((d * d) * max(0.1h, LdotH2) * normalizationTerm);
                
                mainLightColor = (diffuse + specular * specularTerm * _SpecIntensity) * diffuseTerm;

                //额外灯
                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)GetMainLight(i.shadowCoord, i.positionWS, shadowMask);
                Light addLight = GetAdditionalLight(lightIndex, i.positionWS, shadowMask);
                half addHalfLambert = dot(N, addLight.direction) * 0.5 + 0.5;
                additionLightColor += addHalfLambert * diffuse * addLight.color * addLight.shadowAttenuation * addLight.distanceAttenuation;
                LIGHT_LOOP_END

                half4 finalColor = 0;
                finalColor.a = alpha;
                finalColor.rgb = giColor + mainLightColor + additionLightColor + emissionColor;

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
                half3 emission = _EmissionColor*SAMPLE_TEXTURE2D(_EmissionMap,sampler_EmissionMap,input.uv);

                UnityMetaInput metaInput;
                metaInput.Albedo = diffuse + specular * roughness * 0.5;
                metaInput.Emission = emission;
                return UnityMetaFragment(metaInput);
            }
            ENDHLSL
        }
    }
}