#ifndef XinY_INCLUDE_URP_PBR_BASE
#define XinY_INCLUDE_URP_PBR_BASE

#include "./XinY_Include_URP.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


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

struct XinY_SurfaceData
{
    half alpha;
    half metallic;
    half roughness;
    half4 baseMap;
};



void GetBRDFData(out BRDFData brdfData, inout half alpha, half metallic, half roughness, half4 baseMap)
{
    //half oneMinusReflectivity = OneMinusReflectivityMetallic(metallic);
    half oneMinusReflectivity = lerp(0.96, 0, metallic);//1-metallic
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

void GetBRDFData(out BRDFData brdfData, inout XinY_SurfaceData surfaceData)
{
    GetBRDFData(brdfData, surfaceData.alpha, surfaceData.metallic, surfaceData.roughness, surfaceData.baseMap);
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
#if _REFLECTPLANE
    TEXTURE2D(_ReflectRT);
    SAMPLER(sampler_ReflectRT);
    half3 GetIndirectSpecLight(half grazingTerm, half3 specular, half roughness, half reflectIntensity, half fresnel, float2 screenUV)
    {
        half3 reflectMap = SAMPLE_TEXTURE2D(_ReflectRT, sampler_ReflectRT, screenUV);
        half surfaceReduction = 1.0 / (roughness + 1.0);
        half3 indirectSpecular = half3(reflectMap * surfaceReduction * lerp(specular, grazingTerm, fresnel));
        return indirectSpecular;
    }
    half3 GetIndirectSpecLight(BRDFData brdfData, half reflectIntensity, half fresnel, float2 screenUV)
    {
        return GetIndirectSpecLight(brdfData.grazingTerm, brdfData.specular, brdfData.roughness, reflectIntensity, fresnel, screenUV);
    }
#endif

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
    //shadowmask烘焙使用
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


////////////////////////////customPBR
//
////参数结构
struct SurfaceAttrib
{
    half3 baseColor;
    half alpha;
    half metallic;
    half roughness;
};

struct DataNeeded
{
    half NdotV;
    half NdotH;
    half NdotL;
    half LdotH;
    half3 R;
    half3 N;
    half roughness2;
    float2 staticuv;
    half3 lightColor;//考虑过衰减的

};



DataNeeded CalculateDataNeeded(half3 N, float3 posWS, float2 staticuv, Light light, SurfaceAttrib attrib, bool acceptShadow=true)
{
    DataNeeded data;
    half3 L = normalize(light.direction);
    half3 V = normalize(_WorldSpaceCameraPos - posWS);
    half3 R = normalize(reflect(-V, N));
    half3 H = SafeNormalize(L + V);
    data.NdotV = max(0, dot(N, V));
    data.NdotH = max(0, dot(N, H));
    data.LdotH = max(0, dot(L, H));
    data.NdotL = max(0, dot(N, L));
    data.R = R;
    data.roughness2 = attrib.roughness * attrib.roughness;
    data.staticuv = staticuv;
    data.N = N;
    if (acceptShadow)
    {
        data.lightColor = light.color * light.distanceAttenuation * light.shadowAttenuation;
    }
    else
    {
        data.lightColor = light.color;
    }
    return data;
}

void SetAddLightData(inout DataNeeded data, Light light, float3 posWS)
{
    half3 L = normalize(light.direction);
    half3 V = normalize(_WorldSpaceCameraPos - posWS);
    half3 H = SafeNormalize(L + V);
    half3 N = data.N;
    data.LdotH = max(0, dot(L, H));
    data.NdotH = max(0, dot(N, H));
    data.NdotL = max(0, dot(N, L));
    data.lightColor = light.color * light.distanceAttenuation * light.shadowAttenuation;
}

//D项
half D_Function(half NdotH, half roughness2)
{
    half top = roughness2;
    half factor = (NdotH * NdotH * (roughness2 - 1) + 1) * (NdotH * NdotH * (roughness2 - 1) + 1);
    half buttom = XINY_PI * factor;
    half output = Divide(top, buttom);
    return output;
}
//G项部分
half G_Section(half dotValue, half k)
{
    half top = dotValue;
    half buttom = lerp(dotValue, 1, k);
    half output = top / buttom;
    return output;
}
//G项
half G_Function(half NdotL, half NdotV, half roughness)
{
    half k = (roughness + 1) * (roughness + 1) / 8;
    half s1 = G_Section(NdotL, k);
    half s2 = G_Section(NdotV, k);
    half output = s1 * s2;
    return output;
}
//F0
half3 GetF0(half3 baseColor, half metallic)
{
    return lerp(0.04, baseColor, metallic);
}
//F项
half3 F_Function(half3 F0, half LdotH)
{
    half a = pow(1 - LdotH, 5);
    half output = lerp(a, 1, F0);
    return output;
}
//高光部分
half3 SpecSection(half D, half G, half3 F, half NdotV, half NdotL)
{
    half top = D * G * F;
    half buttom = 4 * NdotV * NdotL;
    half output = Divide(top, buttom);
    return output;
}
//漫反射部分
half3 DiffuseSection(half3 baseColor, half3 kd)
{
    half3 output = Divide(kd * baseColor, XINY_PI);
    return output;
}
//间接光F项
half3 F_SectionIndirect(half NdotV, half3 F0, half3 roughness)
{
    half smoothness = 1 - roughness;
    return F0 + (max(smoothness.xxx, F0) - F0) * pow(1 - NdotV, 5);
}
//间接光brdf
half2 GetBRDFEnv(half roughness, half NdotV)
{
    half4 c0 = half4(-1, -0.0275, -0.572, 0.022);
    half4 c1 = half4(1, 0.0425, 1.04, -0.04);
    half4 r = roughness * c0 + c1;
    half a004 = min(r.x * r.x, exp2(-9.28 * NdotV)) * r.x + r.y;
    half2 AB = half2(-1.04, 1.04) * a004 + r.zw;
    return AB;
}
//间接光镜面反射2   已经考虑了反射强度
half3 GetIndirectSpecPartTwo(half NdotV, half3 F0, half3 roughness, inout half3 ks)
{
    half3 F_Indirect = F_SectionIndirect(NdotV, F0, roughness);
    ks = F_Indirect;
    half2 brdf_env = GetBRDFEnv(roughness, NdotV);
    half3 output = F_Indirect * brdf_env.x + brdf_env.y;
    return output;
}
// #if _REFLECTPLANE
//     TEXTURE2D(_ReflectRT);
//     SAMPLER(sampler_ReflectRT);
// #endif
//间接光镜面反射1,实现载体为反射探针 or SSPR   在part2计算强度
half3 GetIndirectSpecPartOne(half3 R, half roughness)
{
    half3 indirectSpecular = 0;
    #if _REFLECTPLANE
        half2 uv = R.xy;
        indirectSpecular = SAMPLE_TEXTURE2D(_ReflectRT, sampler_ReflectRT, uv) * (1 - roughness);
    #else
        half mip = roughness * (1.7 - 0.7 * roughness) * 6;
        indirectSpecular = DecodeHDREnvironment(half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, R, mip)), unity_SpecCube0_HDR);
    #endif
    return indirectSpecular;
}

//间接光镜面反射，，金属已经带有baseColor信息
half3 GetIndirectSpec(half NdotV, half3 F0, half3 roughness, half3 R, inout half3 ks)
{
    half3 p1 = GetIndirectSpecPartOne(R, roughness);
    half3 p2 = GetIndirectSpecPartTwo(NdotV, F0, roughness, ks);
    return p1 * p2;
}
#ifdef LIGHTMAP_ON
    #define SAMPLE_INDIRECT_DIFF(staticuv, n) SampleLightmap(staticuv, 0, n)
#else
    #define SAMPLE_INDIRECT_DIFF(staticuv, n) SampleSH(n)
#endif
//间接光漫反射,实现载体为sh or lightmap  已计算强度，baseColor
half3 GetIndirectDiffuse(half3 baseColor, half3 kd, float2 staticuv, half3 N)
{
    half3 radians = SAMPLE_INDIRECT_DIFF(staticuv, N);//sh or lightmap
    half3 output = radians * baseColor / XINY_PI * kd;
    return output;
}
//直接光部分
half3 GetDirectLightPBRColor(half NdotH, half roughness2, half NdotL, half NdotV, half roughness, half LdotH, half3 baseColor, half metallic, half3 lightColor, half3 F0)
{
    half D = D_Function(NdotH, roughness2);
    half G = G_Function(NdotL, NdotV, roughness);
    half3 F = F_Function(F0, LdotH);
    half3 ks = F;
    half3 kd = (1 - ks) * (1 - metallic);
    half3 diffuse = DiffuseSection(baseColor, kd);
    half3 spec = SpecSection(D, G, F, NdotV, NdotL);
    half3 output = NdotL * lightColor * (diffuse + spec);
    return output;
}
half3 GetDirectLightPBRColor(DataNeeded data, SurfaceAttrib attrib, AOPara ao)
{
    half3 F0 = GetF0(attrib.baseColor, attrib.metallic);
    return GetDirectLightPBRColor(data.NdotH, data.roughness2, data.NdotL, data.NdotV, attrib.roughness, data.LdotH, attrib.baseColor, attrib.metallic, data.lightColor, F0);
}

//间接光部分
half3 GetIndirectLightPBRColor(half3 baseColor, float2 staticuv, half3 N, half NdotV, half3 F0, half3 roughness, half3 R, half metallic)
{
    half3 ks = 1;//inout
    half3 spec = GetIndirectSpec(NdotV, F0, roughness, R, ks);
    half3 kd = (1 - ks) * (1 - metallic);
    half3 diffuse = GetIndirectDiffuse(baseColor, kd, staticuv, N);
    half3 output = diffuse + spec;
    return output;
}

half3 GetOneLightPBRColor(half3 baseColor, float2 staticuv, half3 N, half NdotV, half3 roughness, half3 R, half metallic, half NdotH, half roughness2, half NdotL, half LdotH, half3 lightColor, half directAO, half indirectAO)
{
    half3 F0 = GetF0(baseColor, metallic);
    half3 direct = GetDirectLightPBRColor(NdotH, roughness2, NdotL, NdotV, roughness, LdotH, baseColor, metallic, lightColor, F0);
    half3 indirect = GetIndirectLightPBRColor(baseColor, staticuv, N, NdotV, F0, roughness, R, metallic);
    half3 output = direct * directAO + indirect * indirectAO;
    return output;
}

half3 GetOneLightPBRColor(DataNeeded data, SurfaceAttrib attrib, AOPara ao)
{
    return GetOneLightPBRColor(attrib.baseColor, data.staticuv, data.N, data.NdotV, attrib.roughness, data.R, attrib.metallic, data.NdotH, data.roughness2, data.NdotL, data.LdotH, data.lightColor, ao.directAO, ao.indirectAO);
}

#endif