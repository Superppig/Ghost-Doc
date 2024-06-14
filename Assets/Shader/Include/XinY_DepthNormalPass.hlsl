#ifndef XINY_DEPTH_NORMAL_PASS
#define XINY_DEPTH_NORMAL_PASS

struct DN_appdata
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 texcoord : TEXCOORD0;
};
struct DN_v2f
{
    float2 uv : TEXCOORD0;
    float4 positionCS : SV_POSITION;
    float3 normalWS : TEXCOORD1;
    half3 tangentWS : TEXCOORD2;
    half3 binormalWS : TEXCOORD3;
};

v2f DN_vert(appdata input)
{
    v2f output = (v2f)0;;
    //坐标获取
    output.positionCS = TransformObjectToHClip(input.positionOS);
    //法线获取
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    output.normalWS = normalInput.normalWS;;
    output.tangentWS = normalInput.tangentWS;
    output.binormalWS = normalInput.bitangentWS;
    //UV相关
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    return output;
}

float4 DN_frag(v2f i) : SV_TARGET
{
    half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv), _NormalScale);
    
    half3x3 TBN = half3x3(i.tangentWS, i.binormalWS, i.normalWS);
    half3 N = mul(normalTS, TBN);
    return half4(normalize(N),0.0);
}

#endif