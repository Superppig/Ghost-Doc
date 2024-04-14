
Shader "XinY/ProvedureSkybox"
{
    Properties
    {
        _HorizonRange ("HorizonRange", Range(0, 0.2)) = 0.1
        [HDR]_HorizonColor_Day ("HorizonColor_Day", color) = (1, 1, 1, 1)
        [HDR]_HorizonColor_Night ("HorizonColor_Night", color) = (1, 1, 1, 1)
        [HDR]_SkyColor_Day ("SkyColor_Day", color) = (1, 1, 1, 1)
        [HDR]_SkyColor_Night ("SkyColor_Night", color) = (1, 1, 1, 1)
        _SunSize ("SunSize", Range(0, 1)) = 0.8
        _SunContrast ("SunContrast", Range(0, 1)) = 0.8
        [HDR]_SunColor ("SunColor", color) = (1, 1, 1, 1)
        _MoonSize ("MoonSize", Range(0, 1)) = 0.8
        _MoonContrast ("MoonContrast", Range(0, 1)) = 0.8
        [HDR]_MoonColor ("MoonColor", color) = (1, 1, 1, 1)
        _CloudNoise ("CloudNoise", 2D) = "black" { }
        _CloudDistort ("CloudDistort", 2D) = "black" { }
        _CloudDetail ("CloudDetail", 2D) = "gray" { }
        _CloudStart ("CloudStart", Range(0, 1)) = 0.1
        _CloudEnd ("CloudEnd", Range(0, 1)) = 0.8
        _DistortIntensity ("DistortIntensity", Range(0, 0.1)) = 0.05
        [HDR]_CloudColor_1_Day ("CloudColor_1_Day", color) = (1, 1, 1, 1)
        [HDR]_CloudColor_2_Day ("CloudColor_2_Day", color) = (1, 1, 1, 1)
        [HDR]_CloudColor_1_Night ("CloudColor_2_Night", color) = (1, 1, 1, 1)
        [HDR]_CloudColor_2_Night ("CloudColor_2_Night", color) = (1, 1, 1, 1)
        _StarTex("StarTex",2D)="black"{}
        [HDR]_StarColor_1("StarColor_1",color)=(1,1,1,1)
        [HDR]_StarColor_2("StarColor_2",color)=(1,1,1,1)
        _FlashMask("FlashMask",2D)="black"{}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" "IgnoreProjector" = "True" }
        LOD 100
        Cull Off
        ZWrite Off
        ZTest LEqual
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "../Shader/Include/XinY_Include_URP.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"

        struct appdata
        {
            float4 positionOS : POSITION;
            float4 texcoord : TEXCOORD0;
        };
        struct v2f
        {
            float4 positionCS : SV_POSITION;
            float4 uv : TEXCOORD0;
        };


        CBUFFER_START(UnityPerMaterial)
            half _HorizonRange;
            half4 _HorizonColor_Day;
            half4 _HorizonColor_Night;
            half4 _SkyColor_Day;
            half4 _SkyColor_Night;
            half _SunSize;
            half _SunContrast;
            half4 _SunColor;
            half _MoonSize;
            half _MoonContrast;
            half4 _MoonColor;
            half4 _CloudNoise_ST;
            half _CloudStart;
            half _CloudEnd;
            half4 _CloudDistort_ST;
            half _DistortIntensity;
            half4 _CloudDetail_ST;
            half4 _CloudColor_1_Day;
            half4 _CloudColor_2_Day;            
            half4 _CloudColor_1_Night;
            half4 _CloudColor_2_Night;
            half4 _StarTex_ST;
            half4 _StarColor_1;
            half4 _StarColor_2;
            half4 _FlashMask_ST;
        CBUFFER_END
        TEXTURE2D(_CloudNoise);SAMPLER(sampler_CloudNoise);
        TEXTURE2D(_CloudDistort);SAMPLER(sampler_CloudDistort);
        TEXTURE2D(_CloudDetail);SAMPLER(sampler_CloudDetail);
        TEXTURE2D(_StarTex);SAMPLER(sampler_StarTex);
        TEXTURE2D(_FlashMask);SAMPLER(sampler_FlashMask);

        half _Control;


        ENDHLSL

        Pass
        {

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert(appdata v)
            {
                v2f output = (v2f)0;;
                output.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                output.uv = v.texcoord;
                return output;
            }

            half4 frag(v2f i) : SV_TARGET
            {
                half4 FinalCol = 1;
                half3 skyPara = i.uv.xyz;
                Light mainLight = GetMainLight();
                half isDay = 1-abs(_Control-0.5)*2;
                half NightMask=step(isDay,0.5);
                isDay=pow(isDay,2);
                

                half horizonMask = abs(skyPara.y);
                horizonMask = 1 - smoothstep(0, _HorizonRange, horizonMask);
                half4 horizonCol = lerp(_HorizonColor_Night, _HorizonColor_Day, isDay);
                half4 baseCol = lerp(_SkyColor_Night, _SkyColor_Day, isDay);

                float2 starUV=skyPara.xz / skyPara.y * _StarTex_ST.xy + _StarTex_ST.zw * _Time.y;
                half star=SAMPLE_TEXTURE2D(_StarTex, sampler_StarTex, starUV);
                half4 StarCol=lerp(0,lerp(_StarColor_1,_StarColor_2,star),step(0.01,star))*clamp(skyPara.y,0,1);
                

                half4 SkyCol = lerp(baseCol, baseCol+horizonCol, horizonMask);


                half sunContrast = Remap(_SunContrast, 0, 1, 1 - _SunSize, 0);
                half3 sunPos = mainLight.direction;
                half sunMask = smoothstep(1 - _SunSize - sunContrast, 1 - _SunSize, 1 - clamp(distance(sunPos, skyPara), 0, 1));
                half4 SunCol = sunMask * _SunColor;

                half moonContrast = Remap(_MoonContrast, 0, 1, 1 - _MoonSize, 0);
                half3 moonPos = mainLight.direction;
                half moonMask = smoothstep(1 - _MoonSize - moonContrast, 1 - _MoonSize, 1 - clamp(distance(moonPos, skyPara), 0, 1));
                half4 MoonCol = moonMask * _MoonColor;

                half4 PlanetColor=lerp(SunCol,MoonCol,NightMask);
                half PlanetMask=lerp(sunMask,moonMask,NightMask);

                float2 distortUV = skyPara.xz / skyPara.y * _CloudDistort_ST.xy + _CloudDistort_ST.zw * _Time.y;
                half distort = SAMPLE_TEXTURE2D(_CloudNoise, sampler_CloudNoise, distortUV);
                float2 cloudUV = skyPara.xz / skyPara.y * _CloudNoise_ST.xy + _CloudNoise_ST.zw * _Time.y;
                half cloudNoise = SAMPLE_TEXTURE2D(_CloudNoise, sampler_CloudNoise, cloudUV + distort * _DistortIntensity);
                float2 detailUV = skyPara.xz / skyPara.y * _CloudDetail_ST.xy + _CloudDetail_ST.zw * _Time.y;
                half cloudDetail = SAMPLE_TEXTURE2D(_CloudDetail, sampler_CloudDetail, detailUV);
                half start=clamp(lerp(_CloudStart*1.5,_CloudStart,isDay),0,1);
                half end=clamp(lerp(_CloudEnd*1.5,_CloudEnd,isDay),0,1);
                cloudNoise = smoothstep(start, end, cloudNoise * cloudDetail);
                half cloud = step(0.001, cloudNoise);
                half4 cloud_day=lerp(_CloudColor_1_Day, _CloudColor_2_Day, cloudNoise);
                half4 cloud_night=lerp(_CloudColor_1_Night, _CloudColor_2_Night, cloudNoise);
                half4 CloudCol = lerp(0, lerp(cloud_night, cloud_day, isDay), cloud);

                float2 flashUV=skyPara.xz / skyPara.y * _FlashMask_ST.xy + _FlashMask_ST.zw * _Time.y;
                half flash=SAMPLE_TEXTURE2D(_FlashMask, sampler_FlashMask, flashUV+distort*_DistortIntensity*0.5);
                flash=flash*abs(frac(_Time.y*0.5)-0.5)*2;
                StarCol*=flash;
                SkyCol=lerp(SkyCol+StarCol,SkyCol,isDay);


                FinalCol = lerp(lerp(SkyCol, SkyCol + PlanetColor, PlanetMask), CloudCol, cloudNoise);
                FinalCol = lerp(FinalCol, PlanetColor * CloudCol, cloudNoise * (PlanetMask));
                half verticalMask = smoothstep(0, _HorizonRange, clamp(skyPara.y, 0, 1));
                FinalCol = lerp(SkyCol, FinalCol, verticalMask);
                return FinalCol;
            }
            ENDHLSL
        }
    }
}