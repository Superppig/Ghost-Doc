using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumeLightRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent PassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material VLMaterial;
        public Material BlurMaterial;
        [Range(0, 2)] public float StepDis = 0.2f;
        [Range(0, 3)] public float Intensity = 1.0f;
        public float MaxDis = 20.0f;
        public float MaxCount = 500.0f;
        public float FeatherMax = 45.0f;
        public float FeatherMin = 15.0f;
        public float FeatherDis = 10.0f;

        [Range(1,10)]public int BlurLoop = 5;
        [Range(1,4)]public int BlurDownSample = 2;
        [Range(0.1f, 12.0f)] public float BlurSize = 3.0f;
        [Range(0.1f, 5.0f)] public float SpaceSigma = 1.0f;
        [Range(0.1f,5.0f)]public float RangeSigma = 1.0f;
    }
    class VolumeLightPass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "VolumeLightPass";

        Material vLMat;
        Material blurMat;
        int volumeLightRT = Shader.PropertyToID("_VolumeLightRT");
        int vLBlurRT01 = Shader.PropertyToID("_VolumeLightBlurRT01");
        int vLBlurRT02 = Shader.PropertyToID("_VolumeLightBlurRT02");
        RenderTargetIdentifier curentRT;

        float StepDis;
        float Intensity;
        float MaxDis;
        float MaxCount;
        float FeatherMax;
        float FeatherMin;
        float FeatherDis;
        float BlurSize;
        float SpaceSigma;
        float RangeSigma;
        int BlurLoop;
        int BlurDownSample;

        public void Setup(Settings settings, in RenderTargetIdentifier curentTarget)
        {
            curentRT = curentTarget;
            vLMat = settings.VLMaterial;
            blurMat = settings.BlurMaterial;
            renderPassEvent = settings.PassEvent;
            StepDis = settings.StepDis;
            Intensity = settings.Intensity;
            MaxDis = settings.MaxDis;
            MaxCount = settings.MaxCount;
            FeatherMax = settings.FeatherMax;
            FeatherMin = settings.FeatherMin;
            FeatherDis = settings.FeatherDis;
            BlurSize = settings.BlurSize;
            SpaceSigma = settings.SpaceSigma;
            RangeSigma = settings.RangeSigma;
            BlurLoop = settings.BlurLoop;
            BlurDownSample = settings.BlurDownSample;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            
            
            cmd.GetTemporaryRT(volumeLightRT, desc);
            desc.width /= BlurDownSample;
            desc.height /= BlurDownSample;
            cmd.GetTemporaryRT(vLBlurRT01, desc);
            cmd.GetTemporaryRT(vLBlurRT02, desc);

        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (vLMat == null || blurMat==null) return;

            if (!renderingData.cameraData.postProcessEnabled) return;


            //effVolum = VolumeManager.instance.stack.GetComponent<SphereBlur>();
            //if (!effVolum.UseThis.value) return;
            //if (effVolum == null) return;
            //if (effVolum.UseSceneCamera.value)
            //{

            //}
            //else
            //{
            //    if (renderingData.cameraData.isSceneViewCamera) return;//不处理scene的相机
            //}
            //if (!effVolum.active) return;
            CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);
            Render(cmd, ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            Shader.EnableKeyword("_VOLUME_LIGHT");
            Shader.SetGlobalFloat("_VL_StepDis", StepDis);
            Shader.SetGlobalFloat("_VL_LightIntenisty", Intensity * 0.01f);
            Shader.SetGlobalFloat("_VL_MaxDis", MaxDis);
            Shader.SetGlobalFloat("_VL_MaxCount", MaxCount);
            Shader.SetGlobalFloat("_VL_FeatherMaxDepth", FeatherMax);
            Shader.SetGlobalFloat("_VL_FeatherMinDepth", FeatherMin);
            Shader.SetGlobalFloat("_VL_FeatherDistance", FeatherDis);
            Shader.SetGlobalFloat("_VL_BlurSize", BlurSize);
            Shader.SetGlobalFloat("_VL_BF_SpaceSigma", SpaceSigma);
            Shader.SetGlobalFloat("_VL_BF_RangeSigma", RangeSigma);

            cmd.Blit(null, volumeLightRT, vLMat,0);
            cmd.Blit(volumeLightRT, vLBlurRT01,blurMat, 0);
            for(int i = 0; i < BlurLoop; i++)
            {
                cmd.Blit(vLBlurRT01,vLBlurRT02,blurMat,0);
                cmd.Blit(vLBlurRT02,vLBlurRT01,blurMat,0);
            }
            cmd.SetGlobalTexture("_SourTex",vLBlurRT01);
            cmd.Blit(curentRT, volumeLightRT,blurMat,1);
            cmd.Blit(volumeLightRT, curentRT);
        }


        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(volumeLightRT);
            cmd.ReleaseTemporaryRT(vLBlurRT01);
            cmd.ReleaseTemporaryRT(vLBlurRT02);
        }
    }


    public Settings settings;
    VolumeLightPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new VolumeLightPass();

        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }


    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.Setup(settings, renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


