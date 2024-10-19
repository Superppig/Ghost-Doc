Shader "XinY/PlayerHand"
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
        _DetailMap ("DetailMap", 2D) = "black" { }
        _EmissionMask ("EmissionMask", 2D) = "black" { }
        [HDR]_EmissionColor_Layer1 ("_EmissionColor_Layer1", color) = (0, 0, 0, 1)
        [HDR]_EmissionColor_Layer2 ("_EmissionColor_Layer2", color) = (0, 0, 0, 1)
        [HDR]_EmissionColor_Layer3 ("_EmissionColor_Layer3", color) = (0, 0, 0, 1)
        [HDR]_EmissionColor_Layer4 ("_EmissionColor_Layer4", color) = (0, 0, 0, 1)
        [Toggle(FOG_ON)]_FOG_ON ("Enable Fog", int) = 0
    }

    SubShader
    {

        HLSLINCLUDE
        #pragma multi_compile _ LIGHTMAP_ON

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
            float4 tangentOS : TANGENT;
            float2 texcoord : TEXCOORD0;
            float2 staticLightmapUV : TEXCOORD1;
            float3 normalOS : NORMAL;
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
        };


        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half _MetallicAd;
            half _AOAd;
            half _RoughnessAd;
            float4 _EmissionColor_Layer1;
            float4 _EmissionColor_Layer2;
            float4 _EmissionColor_Layer3;
            float4 _EmissionColor_Layer4;
            half _NormalScale;
        CBUFFER_END
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_MRA);
        SAMPLER(sampler_MRA);
        TEXTURE2D(_NormalMap);
        SAMPLER(sampler_NormalMap);
        TEXTURE2D(_EmissionMask);
        SAMPLER(sampler_EmissionMask);
        TEXTURE2D(_DetailMap);
        SAMPLER(sampler_DetailMap);
        float _BloodDirty;

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
                output.normalWS = normalInput.normalWS;;
                output.tangentWS = normalInput.tangentWS;
                output.binormalWS = normalInput.bitangentWS;
                //UV相关
                output.uv = input.texcoord;
                //计算采样lightmap的UV
                OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
                return output;
            }

            float4 frag(v2f i) : SV_TARGET
            {
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                half3 MRA = SAMPLE_TEXTURE2D(_MRA, sampler_MRA, i.uv).rgb;
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv), _NormalScale);
                float4 detail = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, i.uv);
                float detailMask = step(Remap(detail.a, 0, 1, 0.05, 0.95), _BloodDirty);
                
                half3x3 TBN = half3x3(i.tangentWS, i.binormalWS, i.normalWS);
                half3 N = mul(normalTS, TBN);
                float2 screenUV = GetNormalizedScreenSpaceUV(i.positionCS);
                
                SurfaceAttrib attrib;
                attrib.baseColor = baseMap * lerp(1, detail.rgb, detailMask);
                attrib.metallic = lerp(0, MRA.x, _MetallicAd) * lerp(1, 0.2, detailMask);
                attrib.roughness = pow(lerp(1, MRA.y, _RoughnessAd), 2) * lerp(1, 0.3, detailMask);
                attrib.alpha = baseMap.a;
                half occlusion = lerp(1, MRA.z, _AOAd);

                AOPara aoFactor = GetAOPara(screenUV, occlusion);

                half4 shadowMask = unity_ProbesOcclusion;
                
                float4 shadowCoord = XinY_GetShadowCoord(i.positionCS, i.positionWS);
                Light light = GetMainLight(shadowCoord, i.positionWS, shadowMask, aoFactor.directAO);

                DataNeeded data;
                #ifdef LIGHTMAP_ON
                    data = CalculateDataNeeded(N, i.positionWS, i.staticLightmapUV, light, attrib, false);
                #else
                    data = CalculateDataNeeded(N, i.positionWS, 0, light, attrib, false);
                #endif

                half3 mainLightColor = 0;
                half3 additionLightColor = 0;
                float emissionMask = SAMPLE_TEXTURE2D(_EmissionMask, sampler_EmissionMask, i.uv);
                
                float4 layers = step(emissionMask, 0.3);
                layers.y = step(emissionMask, 0.5) - layers.x;
                layers.z = step(emissionMask, 0.7) - layers.x - layers.y;
                layers.w = step(emissionMask, 0.9) - layers.x - layers.y - layers.z;
                
                float3 emissionColor = _EmissionColor_Layer1 * layers.x + _EmissionColor_Layer2 * layers.y + _EmissionColor_Layer3 * layers.z + _EmissionColor_Layer4 * layers.w;
                
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
    }
    CustomEditor "TestShaderGUI"

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
