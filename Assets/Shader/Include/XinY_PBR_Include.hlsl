#ifndef XinY_INCLUDE_URP_PBR_BASE
#define XinY_INCLUDE_URP_PBR_BASE

#include "./XinY_Include_URP.hlsl"

struct Direct_Dot_Data
{
    half3 V;
    half3 N;
    half3 viewDirTS;
    half3 R;
    half NdotV;
    half fresnelTerm;
    half NdotL;
    half3 H;
    half NdotH;
    half LdotH;
};

void GetBRDFData(out BRDFData brdfData, inout half alpha, half metallic, half roughness, half4 baseMap)
{
    half oneMinusReflectivity = OneMinusReflectivityMetallic(metallic);
    brdfData.albedo = baseMap.rgb;
    brdfData.diffuse = brdfData.albedo * oneMinusReflectivity;
    brdfData.specular = lerp(kDieletricSpec.rgb, brdfData.albedo, metallic);
    brdfData.reflectivity = half(1.0) - oneMinusReflectivity;
    brdfData.perceptualRoughness = roughness;
    half smoothness = 1 - brdfData.perceptualRoughness;
    brdfData.roughness = max(brdfData.perceptualRoughness * brdfData.perceptualRoughness, HALF_MIN_SQRT);
    brdfData.roughness2 = max(roughness * roughness, HALF_MIN);
    brdfData.grazingTerm = saturate(smoothness + brdfData.reflectivity);
    brdfData.normalizationTerm = roughness * half(4.0) + half(2.0);     // roughness * 4.0 + 2.0
    brdfData.roughness2MinusOne = brdfData.roughness2 - half(1.0);;    // roughness^2 - 1.0
    alpha = baseMap.a * oneMinusReflectivity + brdfData.reflectivity;
}

half3 GetIndirectSpecLight(BRDFData brdfData, half3 R, half fresnelTerm)
{
    half3 indirectSpecular = 0;
    half mip = brdfData.perceptualRoughness * (1.7 - 0.7 * brdfData.perceptualRoughness) * 6;
    indirectSpecular = DecodeHDREnvironment(half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, R, mip)), unity_SpecCube0_HDR);
    float surfaceReduction = 1.0 / (brdfData.roughness2 + 1.0);
    indirectSpecular *= half3(surfaceReduction * lerp(brdfData.specular, brdfData.grazingTerm, fresnelTerm));
    return indirectSpecular;
}

half3 GetIndirectSpecLight(BRDFData brdfData, Direct_Dot_Data data)
{
    return GetIndirectSpecLight(brdfData, data.R, data.fresnelTerm);
}

void GetDirDotData(out Direct_Dot_Data data, half3 T, half3 B, half3 N, half3 posWS, half3 normalTS, half3 L)
{
    half3x3 TBN = half3x3(T, B, N);
    data.V = normalize(_WorldSpaceCameraPos - posWS);
    half3 V = data.V;
    data.N = mul(normalTS, TBN);
    data.viewDirTS = XinY_GetViewDirectionTangentSpace(T, B, N, V);
    data.R = reflect(-V, data.N);
    data.NdotV = saturate(dot(data.N, V));
    data.fresnelTerm = Pow4(1.0 - data.NdotV);
    data.NdotL = saturate(dot(data.N, L));
    data.H = SafeNormalize(L + V);
    data.NdotH = saturate(dot(data.N, data.H));
    data.LdotH = saturate(dot(L, data.H));
}

void GetAddDirDotData(inout Direct_Dot_Data data, half3 L)
{
    data.NdotL = saturate(dot(data.N, L));
    data.H = SafeNormalize(L + data.V);
    data.LdotH = saturate(dot(L, data.H));
    data.NdotH = saturate(dot(data.N, data.H));
}

#if defined(LIGHTMAP_ON)
    #define GET_INDIRECT_DIFF(light, staticuv, sh, n, diffuse) GetIndirectDiff(light, staticuv, 0, n, diffuse)
#else
    #define GET_INDIRECT_DIFF(light, staticuv, sh, n, diffuse) GetIndirectDiff(light, 0, sh, n, diffuse)
#endif

half3 GetIndirectDiff(inout Light light, float2 staticLightmapUV, half3 sh, half3 N, half3 diffuse)
{
    half3 bakedGI = 0;
    #if defined(DYNAMICLIGHTMAP_ON)
        //bakedGI = SAMPLE_GI(staticLightmapUV, dynamicLightmapUV, sh, N);
    #else
        bakedGI = SAMPLE_GI(staticLightmapUV, sh, N);
    #endif
    MixRealtimeAndBakedGI(light, N, bakedGI);
    half3 indirectDiffuse = bakedGI * diffuse;
    return indirectDiffuse;
}

half3 GetIndirectCol(half3 spec, half3 diff, half ao)
{
    return (diff + spec) * ao;
}

half3 GetOneLightPBRCol(Light light, Direct_Dot_Data dataNeed, BRDFData brdfData, half adjust)
{
    half3 c = 1;
    half lightAtten = light.distanceAttenuation * light.shadowAttenuation;
    half3 diffuseTerm = light.color * (lightAtten * dataNeed.NdotL);
    float d = dataNeed.NdotH * dataNeed.NdotH * brdfData.roughness2MinusOne + 1.00001f;
    half LdotH2 = dataNeed.LdotH * dataNeed.LdotH;
    half specularTerm = brdfData.roughness2 / ((d * d) * max(0.1h, LdotH2) * brdfData.normalizationTerm);
    
    c = clamp((brdfData.diffuse + brdfData.specular * specularTerm * adjust) * diffuseTerm, 0, 1.5);
    return c;
}

half XinY_SampleAmbientOcclusion(float2 normalizedScreenSpaceUV)
{
    //float2 uv = UnityStereoTransformScreenSpaceTex(normalizedScreenSpaceUV);
    float2 uv = normalizedScreenSpaceUV;
    return half(SAMPLE_TEXTURE2D_X(_ScreenSpaceOcclusionTexture, sampler_ScreenSpaceOcclusionTexture, uv).x);
}

struct AOPara
{
    half directAO;//lerp(half(1.0), ssao, lightIntensity)
    half indirectAO;//min(ssao,texAO)

};

AOPara GetSSAO(float2 normalizedScreenSpaceUV)
{
    AOPara aoFactor;

    #if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
        float ssao = XinY_SampleAmbientOcclusion(normalizedScreenSpaceUV);

        aoFactor.indirectAO = ssao;
        aoFactor.directAO = lerp(half(1.0), ssao, _AmbientOcclusionParam.w);
    #else
        aoFactor.directAO = 1;
        aoFactor.indirectAO = 1;
    #endif

    #if defined(DEBUG_DISPLAY)
        switch(_DebugLightingMode)
        {
            case DEBUGLIGHTINGMODE_LIGHTING_WITHOUT_NORMAL_MAPS:
                aoFactor.directAO = 0.5;
                aoFactor.indirectAO = 0.5;
                break;

            case DEBUGLIGHTINGMODE_LIGHTING_WITH_NORMAL_MAPS:
                aoFactor.directAO *= 0.5;
                aoFactor.indirectAO *= 0.5;
                break;
        }
    #endif

    return aoFactor;
}

AOPara GetAOPara(float2 normalizedScreenSpaceUV, half occlusion)
{
    AOPara aoFactor = GetSSAO(normalizedScreenSpaceUV);
    aoFactor.indirectAO = min(aoFactor.indirectAO, occlusion);
    return aoFactor;
}
//直接光混合了directAO,但没有混合indirectAO
Light GetMainLight(float4 shadowCoord, float3 positionWS, half4 shadowMask, half directAO)
{
    Light light = GetMainLight(shadowCoord, positionWS, shadowMask);

    #if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
        if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_AMBIENT_OCCLUSION))
        {
            light.color *= directAO;
        }
    #endif

    return light;
}

#endif