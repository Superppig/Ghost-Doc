#ifndef XINY_FOG_INCLUDE
#define XINY_FOG_INCLUDE
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

half _HeightFogDensity;
half3 _ButtomFogColor;
half3 _FarFogColor;
half _DistanceFogDensity;
float _InscatterIntensity;

float GetExpFactor(float factor)
{
    float o = -pow(factor, 2);
    return exp(o);
}

float CalculateHeightFogFactor(float3 posWS, half fogDensity)
{
    float heightFactor = max(_WorldSpaceCameraPos.y - posWS.y, 0);
    heightFactor = GetExpFactor(heightFactor * fogDensity);
    return heightFactor;
}

float CalculateDistanceFogFactor(float3 posWS, half fogDensity)
{
    float disFactor = distance(_WorldSpaceCameraPos, posWS);
    disFactor = GetExpFactor(disFactor * fogDensity);
    return disFactor;
}

float CalculateInscatterFactor(float3 posWS, half inscatterIntensity, float disFactor)
{
    Light light = GetMainLight();
    half3 L = normalize(light.direction);
    half3 V = normalize(posWS - _WorldSpaceCameraPos);
    half inscatterFactor = smoothstep(inscatterIntensity, 1, saturate(dot(L, V)));
    inscatterFactor = 1 - exp(-inscatterFactor);
    inscatterFactor *= (1 - disFactor);
    return inscatterFactor;
}

half3 XinY_MixFog(half3 col, float3 posWS)
{
    float heightFactor = CalculateHeightFogFactor(posWS, _HeightFogDensity);
    float disFactor = CalculateDistanceFogFactor(posWS, _DistanceFogDensity);
    float inscatterFactor = CalculateInscatterFactor(posWS, _InscatterIntensity, disFactor);
    //Mix fog color
    Light light = GetMainLight();

    half3 far = lerp(_FarFogColor, light.color, inscatterFactor);
    half3 o;
    o = lerp(_ButtomFogColor, col, heightFactor);
    o = lerp(far, o, disFactor);
    return o;
}

#endif