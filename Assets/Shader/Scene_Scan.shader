Shader "XinY/Scene_Scan"
{
    Properties
    {
        _BaseMap ("BaseMap", 2D) = "white" { }
        _BaseColor ("BaseColor", color) = (1, 1, 1, 1)
        _MRHA ("M(Metallic)R(Roughness)H(Hight)A(AO)", 2D) = "white" { }
        _MetallicAd ("MetallicAd", Range(-2, 2)) = 1
        _RoughnessAd ("RoughnessAd", Range(-2, 2)) = 1
        _AOAd ("AOAd", Range(-2, 2)) = 1
        _NormalMap ("NormalMap", 2D) = "bump" { }
        _NormalScale ("NormalScale", Range(0, 5)) = 1
        _EmissionMap ("EmissionMap", 2D) = "black" { }
        _EmissionMask ("EmissionMask", 2D) = "white" { }
        [HDR]_Emission ("Emission", color) = (0, 0, 0, 1)
        _ScanNum("ScanNum",Range(0,500))=240
        _ScanSpeed ("ScanSpeed", Range(-2, 2)) = 0.5
        _ScanWidth("ScanWidth",Range(0,1))=0.5
        [HDR]_ScanColor("ScanColor",color)=(1,1,1,1)
        [HDR]_OutlineColor ("OutlineColor", color) = (1, 1, 1, 1)
        _OutLineWdith ("OutLineWdith", float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 100
        AlphaTest Off

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                float4 uv : TEXCOORD0;
                
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
                float2 uv_2 : TEXCOORD10;
            };


            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _MetallicAd;
                half _AOAd;
                half _RoughnessAd;
                half4 _Emission;
                half _NormalScale;
                half _ScanNum;
                half _ScanSpeed;
                half _ScanWidth;
                half4 _ScanColor;
            CBUFFER_END
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_MRHA);
            SAMPLER(sampler_MRHA);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_EmissionMask);
            SAMPLER(sampler_EmissionMask);
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
                output.uv = float4(TRANSFORM_TEX(v.texcoord, _BaseMap), v.texcoord);
                output.uv_2 = v.texcoord2;
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
                half4 MRHA = SAMPLE_TEXTURE2D(_MRHA, sampler_MRHA, i.uv);
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv), _NormalScale);

                half emissMask = SAMPLE_TEXTURE2D(_EmissionMask, sampler_EmissionMask, i.uv_2);

                float scanBaseValue=i.uv.x*_ScanNum;
                scanBaseValue=floor(scanBaseValue);
                scanBaseValue=RandomRange(float2(scanBaseValue,1),0,1);
                float scanBase=smoothstep(scanBaseValue,scanBaseValue+_ScanWidth,frac(i.uv.y-_ScanSpeed*_Time.y*RandomRange(float2(scanBaseValue,1),0.5,2)));
                scanBase=scanBase*step(scanBase,0.999)*emissMask;
                half3 scanColor=lerp(0,_ScanColor,scanBase);

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
                half metallic = max(lerp(0.04, MRHA.r, _MetallicAd), 0);
                half oneMinusReflectivity = OneMinusReflectivityMetallic(metallic);
                half3 albedo = baseMap.rgb*scanColor;
                half3 diffuse = albedo * oneMinusReflectivity;;
                half3 specular = lerp(kDieletricSpec.rgb, albedo, metallic);;
                half reflectivity = half(1.0) - oneMinusReflectivity;
                half perceptualRoughness = lerp(0, MRHA.g, _RoughnessAd);
                half smoothness = 1 - perceptualRoughness;
                half roughness = max(perceptualRoughness * perceptualRoughness, HALF_MIN_SQRT);
                half roughness2 = max(roughness * roughness, HALF_MIN);;
                half grazingTerm = saturate(smoothness + reflectivity);;
                half normalizationTerm = roughness * half(4.0) + half(2.0);;     // roughness * 4.0 + 2.0
                half roughness2MinusOne = roughness2 - half(1.0);;    // roughness^2 - 1.0
                half alpha = baseMap.a * oneMinusReflectivity + reflectivity;
                half3 emission = _Emission * emissMask;
                half occlusion = max(lerp(1, MRHA.a, _AOAd), 0);
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
                
                mainLightColor = (diffuse + specular * specularTerm) * diffuseTerm;

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

        //外描线
        Pass
        {
            Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
            Cull Front
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                half4 tangentOS : TANGENT;
                float4 color : COLOR;//顶点色

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 color : TEXCOORD1;
            };

            float _OutLineWdith;
            float4 _OutlineColor;

            v2f vert(appdata v)
            {
                //模型顶点沿法线外拓
                v2f o;
                float3 posWS = TransformObjectToWorld(v.vertex);
                float3 posVS = TransformWorldToView(posWS);
                float3 normalTS = v.color * 2 - 1;
                real sign = real(v.tangentOS.w) * (unity_WorldTransformParams.w >= 0.0 ? 1.0 : - 1.0);
                half3 normalWS = TransformObjectToWorldNormal(v.normalOS);
                half3 tangentWS = real3(TransformObjectToWorldDir(v.tangentOS.xyz));
                half3 binormalWS = real3(cross(normalWS, float3(tangentWS))) * sign;
                half3x3 TBN = half3x3(tangentWS, binormalWS, normalWS);
                float3 normal = mul(normalTS, TBN);
                float3 normalVS = mul((float3x3)UNITY_MATRIX_V, normal);

                // float3 normalWS=TransformObjectToWorldNormal(v.normalOS);
                // float3 normalVS=TransformWorldToViewDir(normalWS);

                posVS = (posVS + normalVS * _OutLineWdith * 0.001) * max(1, posVS.z);
                o.pos = TransformWViewToHClip(posVS);
                o.uv = v.uv;
                // o.posWS=UnityObjectToWorldDir(v.vertex);
                // o.normalWS=UnityObjectToWorldNormal(v.normal);
                // o.vertexColor=v.color;
                o.color = v.color;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return float4(_OutlineColor.rgb, 1.0);
            }
            ENDHLSL
        }
        
        //ShadowCaster
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
        //DepthOnly
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
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
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

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaLit

            #pragma shader_feature EDITOR_VISUALIZATION
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

            #pragma shader_feature_local_fragment _SPECGLOSSMAP

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitMetaPass.hlsl"

            ENDHLSL
        }
    }
}