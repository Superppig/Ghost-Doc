#ifndef XinY_INCLUDE_URP
#define XinY_INCLUDE_URP
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

inline float3 GetWorldViewDir(float3 posWS)
{
    return normalize(_WorldSpaceCameraPos.xyz - posWS);
}

inline float3 RotateAroundY(float degree, float3 target)
{
    float rad = degree * PI / 180;
    float2x2 m_rotate = float2x2(cos(rad), -sin(rad),
    sin(rad), cos(rad));
    float2 dir_rotate = mul(m_rotate, target.xz);
    target = float3(dir_rotate.x, target.y, dir_rotate.y);
    return target;
}

inline float Remap(float x, float minOld, float maxOld, float minNew, float maxNew)
{
    return (x - minOld) / (maxOld - minOld) * (maxNew - minNew) + minNew;
}

//需要流动的噪声图，采样噪声图的uv，流动方向（由函数外部采样flowmap得到），流动强度，流动速度
inline float4 Flow(sampler2D Tex, float2 UV, float2 FlowDir, float2 FlowStrength, float FlowSpeed)
{
    //第一次
    //FlowDir+0.5是为了把0-1重映射到-0.5-0.5
    //在函数外提前将FlowDir取负即可
    float2 UV_1 = UV + (FlowDir + 0.5) * (frac(FlowSpeed * _Time.y) * FlowStrength);
    float4 Noise_1 = tex2D(Tex, UV_1);
    //第二次
    //Time移动0.5相位后在运算，得到一个和原来Noise有一定偏差的Noise
    float2 UV_2 = UV + (FlowDir + 0.5) * (frac(FlowSpeed * _Time.y + 0.5f) * FlowStrength);
    float4 Nosie_2 = tex2D(Tex, UV_2);
    //插值因子
    float factor = abs(frac(FlowSpeed * _Time.y) * 2 - 1);
    //插值
    //让结果在两张Noise中变换，模拟流动效果
    float4 final = lerp(Noise_1, Nosie_2, factor);
    return final;
}

inline float3 ACES_Tonemapping(float3 x)
{
    float a = 2.51f;
    float b = 0.03f;
    float c = 2.43f;
    float d = 0.59f;
    float e = 0.14f;
    float3 encode_color = saturate((x * (a * x + b)) / (x * (c * x + d) + e));
    return encode_color;
}

inline float3 ComputeSH(float3 normal, half4 custom_SHAr, half4 custom_SHAg, half4 custom_SHAb,
half4 custom_SHBr, half4 custom_SHBg, half4 custom_SHBb, half4 custom_SHC)
{
    float4 normalForSH = float4(normal, 1.0);
    //SHEvalLinearL0L1
    half3 x;
    x.r = dot(custom_SHAr, normalForSH);
    x.g = dot(custom_SHAg, normalForSH);
    x.b = dot(custom_SHAb, normalForSH);

    //SHEvalLinearL2
    half3 x1, x2;
    // 4 of the quadratic (L2) polynomials
    half4 vB = normalForSH.xyzz * normalForSH.yzzx;
    x1.r = dot(custom_SHBr, vB);
    x1.g = dot(custom_SHBg, vB);
    x1.b = dot(custom_SHBb, vB);

    // Final (5th) quadratic (L2) polynomial
    half vC = normalForSH.x * normalForSH.x - normalForSH.y * normalForSH.y;
    x2 = custom_SHC.rgb * vC;

    float3 sh = max(float3(0.0, 0.0, 0.0), (x + x1 + x2));
    sh = pow(sh, 1.0 / 2.2);
    return sh;
}

inline float2 ParallaxMapping(float2 uv, float3 V_TS, sampler2D parallaxMap, int count, float parallaxIntensity)
{
    //计算偏差
    float2 uv_parallax = uv;
    for (int j = 0; j < count; j++)
    {
        float height = tex2D(parallaxMap, uv_parallax);
        //uv偏移  贴图一般是高度度，但shader里面计算用的是深度(1.0-height)
        uv_parallax = uv_parallax - (1.0 - height) * V_TS.xy * parallaxIntensity * 0.01f;
    }
    return uv_parallax;
}

// inline float3 BlendNormal(float3 A, float3 B)
// {
//     return normalize(float3(A.xy + B.xy, A.z * B.z));
// }

inline float2 GetPolarcoordinates(float2 uv, float2 center)
{
    uv = uv - center;
    float theta = atan2(uv.y, uv.x);
    theta = theta / PI;
    theta = theta * 0.5 + 0.5;
    float r = length(uv) * 2;
    return float2(theta, r);
}

// inline float3 GetPosWSFromDepth(float2 depthMapUV, sampler2D depthTex, float3 posVS)
// {
//     //通过深度图求出像素在观察空间中坐标的Z值
//     //通过当前渲染的面片求出像素在观察空间下的坐标
//     //通过以上两者求出深度图中像素的XYZ坐标

//     //获取深度图
//     half4 depthMap = SAMPLE_DEPTH_TEXTURE(depthTex, depthMapUV);
//     //深度图的线性转换
//     half depth = LinearEyeDepth(depthMap);

//     //构建深度图上的像素在观察空间下的坐标
//     float4 depthVS = 1;//w分量
//     depthVS.z = depth;
//     //根据相似三角形的知识，有本shader渲染的模型的posVS来推出深度图中的物体的posVS（depthVS）
//     depthVS.xy = posVS.xy * depth / (-posVS.z);
//     //构建深度图上的像素在世界空间下的坐标
//     float3 depthWS = mul(unity_CameraToWorld, depthVS);
//     return depthWS;
// }

inline float4 GetPosNDC(float4 posCS)
{
    float4 ndc = posCS * 0.5f;
    float4 posNDC = float4(0, 0, 0, 0);
    posNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    posNDC.zw = posCS.zw;
    return posNDC;
}

inline float2 GetMatCapUV(float3 normalWS)
{
    float3 normalVS = normalize(mul((float3x3)UNITY_MATRIX_V, normalWS));
    return normalVS.xy * 0.5 + 0.5;
}

inline half CheapContrast(half In, half Contrast)
{
    half temp = lerp(0 - Contrast, 1 + Contrast, In);
    return clamp(temp, 0.0f, 1.0f);
}

inline float HeightLerp(float transition, float height, float blendContrast)
{
    float alpha = clamp(height - 1 + 2 * transition, 0, 1);
    float o = lerp(0 - blendContrast, 1 + blendContrast, alpha);
    return o;
}

inline float2 FlipbookUVAnimation(float2 uv, half row, half colum, half time)
{
    //计算uv起始点（左上角），且采样范围为一个小方格
    float2 uv_R = float2(uv.x / colum, uv.y / row + (row - 1) * 1 / row);
    //计算uv的变化
    uv_R.x += frac(floor(_Time.y * row * colum * (1 / time)) / colum);
    uv_R.y -= frac(floor(_Time.y * row * (1 / time)) / row);
    //time优化为跑完一周的时间,frac处理是由于Time导致的float的精度问题
    return uv_R;
}

/*
    inline float HitWave(sampler2D RampTex,float3 worldPos,float HitNoise,float HitSpread,
    float HitFadeDistance,float HitFadePower,float AffectorAmount){
        float hit_result;
        for(int j =0;j<AffectorAmount;j++)
        {
            float distance_mask = distance(HitPosition[j],worldPos);
            float hit_range = -clamp((distance_mask - HitSize[j] + HitNoise) /HitSpread,-1,0);
            float2 ramp_uv = float2(hit_range,0.5);
            float hit_wave = tex2D(RampTex,ramp_uv).r;
            float hit_fade = saturate((1.0 - distance_mask / HitFadeDistance)* HitFadePower);
            hit_result = hit_result + hit_fade* hit_wave;
        }
        return saturate(hit_result);
    }
*/

// float hit_result;
// for(int j =0;j<AffectorAmount;j++)
// {
//     float distance_mask = distance(HitPosition[j],worldPos);
//     float hit_range = -clamp((distance_mask - HitSize[j] + HitNoise) /HitSpread,-1,0);
//     float2 ramp_uv = float2(hit_range,0.5);
//     float hit_wave = tex2D(RampTex,ramp_uv).r;
//     float hit_fade = saturate((1.0 - distance_mask / HitFadeDistance)* HitFadePower);
//     hit_result = hit_result + hit_fade* hit_wave;
// }

//浮雕映射
float2 ReliefMapping(float2 uv, float3 V_TS, float offsetMax, float rayNumberMax, sampler2D heightMap)
{
    //最大偏移向量   V.xy/V.z是视方向与uv平面的竖轴的夹角的tan值，
    //乘上offsetMax即可得到uv在uv平面沿V在uv平面的投影方向的最大偏移值
    float2 offlayerUV = V_TS.xy / V_TS.z * offsetMax;
    //最大步进次数
    float RayNumber = rayNumberMax;
    //层间的高度差
    float layerHeight = 1 / RayNumber;
    //每步的偏移向量
    float2 SteppingUV = offlayerUV / RayNumber;
    //当前高度
    float currentLayerHeight = 0;
    //最大偏移向量的模
    float offlayerUVL = length(offlayerUV);
    //uv的偏移值
    float2 offUV = float2(0, 0);
    for (int i = 0; i < RayNumber; i++)
    {
        offUV += SteppingUV;
        float currentHeight = tex2D(heightMap, uv + offUV).r;
        currentLayerHeight += layerHeight;
        //当前的采样高度比层高度要小，即视线与精确高度信息相碰
        if (currentHeight < currentLayerHeight)
        {
            break;
        }
    }
    //二分查找终点，即步进算法中的uv
    float2 T0 = uv + offUV;
    //二分查找起点，
    float2 T1 = uv + offUV - SteppingUV;//上一个UV

    //二分查找
    for (int j = 0; j < RayNumber; j++)
    {
        //中点
        float2 P0 = (T1 + T0) * 0.5;
        //中点采样高度
        float P0Height = tex2D(heightMap, P0).r;
        //中点在起点层高和终点层高间的插值层高
        float P0LayerHeight = length(P0) / offlayerUVL;

        if (P0Height < P0LayerHeight)
        {
            T0 = P0;
        }
        else
        {
            T1 = P0;
        }
    }
    //返回uv的偏移值
    return (T0 + T1) / 2 - uv;
}

float3 HSV2RGB(float3 c)
{
    float3 rgb = clamp(abs(fmod(c.x * 6.0 + float3(0.0, 4.0, 2.0), 6) - 3.0) - 1.0, 0, 1);
    rgb = rgb * rgb * (3.0 - 2.0 * rgb);
    return c.z * lerp(float3(1, 1, 1), rgb, c.y);
}
float3 RGB2HSV(float3 c)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

half Brightness_Gamma(half3 c)
{
    return dot(c, half3(0.22, 0.707, 0.071));
}

half XinY_Brightness_Linear(half3 c)
{
    return dot(c, half3(0.0396, 0.458, 0.0061));
}

float RandFromFloat3(float3 co)
{
    return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
}

float3x3 AngleAxis3x3(float angle, float3 axis)
{
    float c, s;
    sincos(angle, s, c);

    float t = 1 - c;
    float x = axis.x;
    float y = axis.y;
    float z = axis.z;

    return float3x3(
        t * x * x + c, t * x * y - s * z, t * x * z + s * y,
        t * x * y + s * z, t * y * y + c, t * y * z - s * x,
        t * x * z - s * y, t * y * z + s * x, t * z * z + c
    );
}

inline float3 XinY_DecodeNormal(float4 enc4)
{
    float kScale = 1.7777;
    float3 nn = enc4.xyz * float3(2 * kScale, 2 * kScale, 0) + float3(-kScale, -kScale, 1);
    float g = 2.0 / dot(nn.xyz, nn.xyz);
    float3 n;
    n.xy = g * nn.xy;
    n.z = g - 1;
    return n;
}

inline float3 XinY_Saturation(float3 col, float brightness, float alpha)
{
    return lerp(brightness, col, alpha);
}
inline float3 XinY_Contrast(float3 col, float alpha)
{
    return lerp(0.5, col, alpha);
}
inline float unity_noise_randomValue(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

inline float unity_noise_interpolate(float a, float b, float t)
{
    return (1.0 - t) * a + (t * b);
}

inline float unity_valueNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    uv = abs(frac(uv) - 0.5);
    float2 c0 = i + float2(0.0, 0.0);
    float2 c1 = i + float2(1.0, 0.0);
    float2 c2 = i + float2(0.0, 1.0);
    float2 c3 = i + float2(1.0, 1.0);
    float r0 = unity_noise_randomValue(c0);
    float r1 = unity_noise_randomValue(c1);
    float r2 = unity_noise_randomValue(c2);
    float r3 = unity_noise_randomValue(c3);

    float bottomOfGrid = unity_noise_interpolate(r0, r1, f.x);
    float topOfGrid = unity_noise_interpolate(r2, r3, f.x);
    float t = unity_noise_interpolate(bottomOfGrid, topOfGrid, f.y);
    return t;
}

half Unity_SimpleNoise_float(float2 UV, float Scale)
{
    float t = 0.0;

    float freq = pow(2.0, float(0));
    float amp = pow(0.5, float(3 - 0));
    t += unity_valueNoise(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(1));
    amp = pow(0.5, float(3 - 1));
    t += unity_valueNoise(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(2));
    amp = pow(0.5, float(3 - 2));
    t += unity_valueNoise(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    return t;
}

float2 XinY_Rotate_Radians(float2 UV, float2 Center, float Rotation)
{
    UV -= Center;
    float s = sin(Rotation);
    float c = cos(Rotation);
    float2x2 rMatrix = float2x2(c, -s, s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2 - 1;
    UV.xy = mul(UV.xy, rMatrix);
    UV += Center;
    return UV;
}

float4 XinY_GetShadowCoord(float4 positionCS, float3 positionWS)
{
    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
        return ComputeScreenPos(positionCS);
        
    #else
        return TransformWorldToShadowCoord(positionWS);
        
    #endif
}

half3 XinY_GetViewDirectionTangentSpace(half3 tangentWS, half3 binormalWS, half3 normalWS, half3 viewDirWS)
{
    // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
    half3 unnormalizedNormalWS = normalWS;
    const half renormFactor = 1.0 / length(unnormalizedNormalWS);

    // use bitangent on the fly like in hdrp
    // IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
    // we do not need to multiple GetOddNegativeScale() here, as it is done in vertex shader
    half3 bitang = binormalWS;

    half3 WorldSpaceNormal = renormFactor * normalWS.xyz;       // we want a unit length Normal Vector node in shader graph

    // to preserve mikktspace compliance we use same scale renormFactor as was used on the normal.
    // This is explained in section 2.2 in "surface gradient based bump mapping framework"
    half3 WorldSpaceTangent = renormFactor * tangentWS.xyz;
    half3 WorldSpaceBiTangent = renormFactor * bitang;

    half3x3 tangentSpaceTransform = half3x3(WorldSpaceTangent, WorldSpaceBiTangent, WorldSpaceNormal);
    half3 viewDirTS = mul(tangentSpaceTransform, viewDirWS);

    return viewDirTS;
}

// Force enable fog fragment shader evaluation
real XinY_InitializeFog(float4 positionWS)
{
    real fogFactor = 0.0;

    #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
        // Compiler eliminates unused math --> matrix.column_z * vec
        float viewZ = - (mul(UNITY_MATRIX_V, positionWS).z);
        // View Z is 0 at camera pos, remap 0 to near plane.
        float nearToFarZ = max(viewZ - _ProjectionParams.y, 0);
        fogFactor = ComputeFogFactorZ0ToFar(nearToFarZ);
    #endif

    return fogFactor;
}
#endif