#ifndef XINY_FOG_INCLUDE
#define XINY_FOG_INCLUDE
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

half _HeightFogDensity;
half3 _ButtomFogColor;
half3 _FarFogColor;
half _DistanceFogDensity;
float _InscatterIntensity;


half3 XinY_MixFog(half3 col, half3 posWS)
{
    float heightFactor = max(_WorldSpaceCameraPos.y - posWS.y, 0);
    heightFactor = -pow(heightFactor * _HeightFogDensity, 2);
    heightFactor = exp(heightFactor);

    float disFactor = distance(_WorldSpaceCameraPos, posWS);
    disFactor = -pow(disFactor * _DistanceFogDensity, 2);
    disFactor = exp(disFactor);

    Light light = GetMainLight();
    half3 L=normalize(light.direction);
    half3 V=normalize(posWS-_WorldSpaceCameraPos);
    half inscatterFactor=smoothstep(_InscatterIntensity,1,saturate(dot(L,V)));
    inscatterFactor=1-exp(-inscatterFactor);
    inscatterFactor*=(1-disFactor);
    half3 far=lerp(_FarFogColor,light.color,inscatterFactor);

    half3 o;
    o = lerp(_ButtomFogColor, col, heightFactor);
    o = lerp(far, o, disFactor);
    return o;
}

#endif