using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenSpacePlanarReflectRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Setting
    {
        public RenderPassEvent PassEvent=RenderPassEvent.AfterRenderingOpaques;
        public ComputeShader shader;
        public int RTSize=512;
        public float ReflectHeight=0.0f;
        //public Material blurMat;
        //[Range(1,15)]public int Loop=1;
        //public float BlurSize=1;
        //[Range(1,4)]public int BlurDownSample = 1;
    }
    public Setting setting;


    class CustomRenderPass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "SSPRPass";

        int ReflectRT = Shader.PropertyToID("_ReflectRT");
        int ReflectDepthBuffer = Shader.PropertyToID("_ReflectDepthBuffer");
        //int temp01 = Shader.PropertyToID("_ReflectBlurTemp01");
        //int temp02 = Shader.PropertyToID("_ReflectBlurTemp02");

        RenderTargetIdentifier currentColor;
        Setting m_setting;
        int groupX;
        int groupY;
        int width;
        int height;
        //Material blurMaterial;

        public void Setup(Setting setting,in RenderTargetIdentifier target)
        {
            m_setting = setting;
            currentColor=target;
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var cameraDesc = renderingData.cameraData.cameraTargetDescriptor;
            float aspect=(float)cameraDesc.width/(float)cameraDesc.height;
            int groupThreadX = 8;
            int groupThreadY = 8;
            height=m_setting.RTSize;
            width=Mathf.RoundToInt(m_setting.RTSize * aspect);
            groupX = width / groupThreadX;
            groupY = height / groupThreadY;
            var desc = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32);
            desc.enableRandomWrite = true;
            cmd.GetTemporaryRT(ReflectRT, desc);
            desc.colorFormat = RenderTextureFormat.R8;
            cmd.GetTemporaryRT(ReflectDepthBuffer, desc);
            //int blurRTWidth = width / m_setting.BlurDownSample;
            //int blurRTHeight = height / m_setting.BlurDownSample;
            //var blurDesc=new RenderTextureDescriptor(blurRTWidth, blurRTHeight, RenderTextureFormat.ARGB32);
            //cmd.GetTemporaryRT(temp01, blurDesc);
            //cmd.GetTemporaryRT(temp02, blurDesc);

        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_setting.shader == null) return;
            //if (m_setting.blurMat == null) return;
            //blurMaterial = m_setting.blurMat;

            if (renderingData.cameraData.isSceneViewCamera) return;//不处理scene的相机
            var cmd = CommandBufferPool.Get(k_RenderTag);
            Render(cmd,ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        void Render(CommandBuffer cmd,ref RenderingData renderingData)
        {
            var cs = m_setting.shader;
            cmd.SetComputeVectorParam(cs,"_RTSize",new Vector4(width,height,1,1));
            cmd.SetComputeFloatParam(cs, "_ReflectHeight", m_setting.ReflectHeight);
            cmd.SetComputeTextureParam(cs, 0, "_ReflectRT", ReflectRT);
            cmd.SetComputeTextureParam(cs, 0, "_ReflectDepthBuffer", ReflectDepthBuffer);
            cmd.SetComputeTextureParam(cs, 0, "_CameraColorRT", currentColor);
            cmd.SetComputeTextureParam(cs, 0, "_CameraDepthRT", new RenderTargetIdentifier("_CameraDepthTexture"));
            //cmd.SetRenderTarget(ReflectRT);
            cmd.DispatchCompute(cs, 0, groupX, groupY, 1);
            //cmd.Blit(ReflectRT, cameraRT);

            //cmd.Blit(ReflectRT, temp01);
            //int iteration = m_setting.Loop;
            //float size = 0;
            //for (int i = 0; i < iteration; i++)
            //{
            //    size = (i + 1) * m_setting.BlurSize;
            //    blurMaterial.SetFloat("_Size", size);
            //    cmd.Blit(temp01, temp02, blurMaterial, 0);
            //    cmd.Blit(temp02, temp01, blurMaterial, 0);
                

            //}
            //cmd.Blit(temp01, ReflectRT);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(ReflectRT);
            cmd.ReleaseTemporaryRT(ReflectDepthBuffer);
            //cmd.ReleaseTemporaryRT(temp01);
            //cmd.ReleaseTemporaryRT(temp02);
        }
    }

    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.Setup(setting,renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


