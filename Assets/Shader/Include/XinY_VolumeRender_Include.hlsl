#ifndef XinY_VOLUMERENDER_INCLUDE
#define XinY_VOLUMERENDER_INCLUDE
#include "../Include/XinY_Include_URP.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


struct BBoxPara
{
    float3 boxMin;
    float3 boxMax;
};
struct IntersectPara
{
    float distanceToBBox;
    float distanceInBBox;
};
//射线与包围盒相交, x分量到包围盒最近的距离， y分量穿过包围盒的距离
float2 RayBoxDst(float3 boxMin, float3 boxMax, float3 pos, float3 rayDir)
{
    float3 t0 = (boxMin - pos) / rayDir;//计算origin到包围盒Min和Max的XYZ平面交点的距离
    float3 t1 = (boxMax - pos) / rayDir;
    
    float3 tmin = min(t0, t1);//处理从Max到Min方向
    float3 tmax = max(t0, t1);
    
    //射线到box两个相交点的距离, dstA最近距离， dstB最远距离
    float nearest = max(max(tmin.x, tmin.y), tmin.z);
    float farthest = min(min(tmax.x, tmax.y), tmax.z);
    
    float dstToBox = max(0, nearest);
    float dstInBox = max(0, farthest - nearest);
    
    return float2(dstToBox, dstInBox);
}
float2 RayBoxDst(BBoxPara para, float3 posWS, float3 rayDir)
{
    return RayBoxDst(para.boxMin, para.boxMax, posWS, rayDir);
}
IntersectPara GetRayBoxPara(BBoxPara para, float3 posWS, float3 rayDir)
{
    float2 o = RayBoxDst(para.boxMin, para.boxMax, posWS, rayDir);
    IntersectPara output;
    output.distanceToBBox = o.x;
    output.distanceInBBox = o.y;
    return output;
}

float _VL_StepDis;
float _VL_LightIntenisty;
float _VL_MaxDis;
float _VL_MaxCount;
float _VL_FeatherMaxDepth;
float _VL_FeatherMinDepth;
float _VL_FeatherDistance;

half VL_GetShadowFactor(float4 positionCS, float3 positionWS)
{
    float4 shadowCoord = XinY_GetShadowCoord(positionCS, positionWS);
    half shadowFactor = MainLightRealtimeShadow(shadowCoord);
    return shadowFactor;
}

half VL_GetAddLightShadowFactor()
{
    uint pixelLightCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(pixelLightCount)
    Light addLight = GetAdditionalLight(lightIndex, i.positionWS, shadowMask);

    LIGHT_LOOP_END
}

half VL_GetShadowFactor(float3 positionWS)
{
    float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
    half shadowFactor = MainLightRealtimeShadow(shadowCoord);
    return shadowFactor;
}

half VL_GetStepLight(float3 curPos)
{
    //在阴影则不贡献光照，不在阴影则贡献光照
    half output = _VL_LightIntenisty * VL_GetShadowFactor(curPos);
    return output;
}

half VL_GetVolumeLightIntensity(float3 startPos, half3 rayDir, float pixelDepth)
{
    float dis = 0;
    float3 curPos = startPos;
    half totalLum = 0;
    int shadowCounts = 0;
    float stepDis = _VL_StepDis;
    half curLum = 0;
    int iteration = 0;
    for (iteration = 0; iteration < _VL_MaxCount; iteration++)
    {
        if (shadowCounts > 2)
        {
            stepDis = 2 * _VL_StepDis;
        }
        else
        {
            stepDis = _VL_StepDis;
        }


        curPos = startPos + rayDir * dis;
        dis += stepDis;
        if (dis >= _VL_MaxDis) break;
        if (dis >= pixelDepth) break;
        curLum = VL_GetStepLight(curPos);
        if (curLum < 0.1)
        {
            shadowCounts++;
        }
        else
        {
            shadowCounts = 0;
        }
        totalLum += curLum;
    }
    // float check1 = dis > _VL_MaxDis ? 0 : 1;
    // float check2 = iteration == _VL_MaxCount ? 0 : 1;
    // totalLum = totalLum * check1 * check2;
    return totalLum;
}

half VL_DepthFeather(float pixelDepth)
{
    half factor_low = saturate(RemapTo01(pixelDepth, _VL_FeatherMinDepth, _VL_FeatherMinDepth + _VL_FeatherDistance));
    half factor_heigh = saturate(RemapTo10(pixelDepth, _VL_FeatherMaxDepth - _VL_FeatherDistance, _VL_FeatherMaxDepth));
    return factor_heigh * factor_low;
}

#define XinY_PI 3.1415926
float _VL_BlurSize;
float _VL_BF_SpaceSigma;
float _VL_BF_RangeSigma;


// half VL_BilateralFiltering(float2 uv)
// {
//     float weight_sum = 0;
//     float color_sum = 0;
//     float color_origin = SAMPLE_TEXTURE2D(_VolumeLightRT, sampler_VolumeLightRT, uv);
//     for (int i = -_VL_BlurSize; i < _VL_BlurSize; i++)
//     {
//         for (int j = -_VL_BlurSize; j < _VL_BlurSize; j++)
//         {
//             //空域高斯
//             float2 varible = uv + float2(i * _VolumeLightRT_TexelSize.x, j * _VolumeLightRT_TexelSize.y);
//             float space_factor = i * i + j * j;
//             space_factor = (-space_factor) / (2 * _VL_BF_SpaceSigma * _VL_BF_SpaceSigma);
//             float space_weight = 1 / (_VL_BF_SpaceSigma * _VL_BF_SpaceSigma * 2 * XinY_PI) * exp(space_factor);

//             //值域高斯
//             float color_neighbor = SAMPLE_TEXTURE2D(_VolumeLightRT, sampler_VolumeLightRT, varible);
//             float color_distance = (color_neighbor - color_origin);
//             float value_factor = color_distance.r * color_distance.r ;
//             value_factor = (-value_factor) / (2 * _VL_BF_RangeSigma * _VL_BF_RangeSigma);
//             float value_weight = (1 / (_VL_BF_RangeSigma * _VL_BF_RangeSigma * 2 * XinY_PI)) * exp(value_factor);

//             weight_sum += space_weight * value_weight;
//             color_sum += color_neighbor * space_weight * value_weight;
//         }
//     }
//     half output=color_sum/weight_sum;

//     return output;
// }

#endif