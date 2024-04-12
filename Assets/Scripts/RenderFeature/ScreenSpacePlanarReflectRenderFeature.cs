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
        public int RTSize;
        public float ReflectHeight=0.0f;
    }
    public Setting setting;


    class CustomRenderPass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "SSPRPass";

        int ReflectRT = Shader.PropertyToID("_ReflectRT");
        int ReflectDepthBuffer = Shader.PropertyToID("_ReflectDepthBuffer");


        Setting m_setting;
        RenderTargetIdentifier cameraRT;
        int groupX;
        int groupY;
        int width;
        int height;

        public void Setup(Setting setting,RenderTargetIdentifier cameraRT)
        {
            m_setting = setting;
            this.cameraRT = cameraRT;
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
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_setting.shader == null) return;
            //if (renderingData.cameraData.isSceneViewCamera) return;//不处理scene的相机
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
            cmd.SetComputeTextureParam(cs, 0, "_CameraColorRT", new RenderTargetIdentifier("_CameraOpaqueTexture"));
            cmd.SetComputeTextureParam(cs, 0, "_CameraDepthRT", new RenderTargetIdentifier("_CameraDepthTexture"));
            cmd.DispatchCompute(cs, 0, groupX, groupY, 1);
            //cmd.Blit(ReflectRT, cameraRT);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(ReflectRT);
            cmd.ReleaseTemporaryRT(ReflectDepthBuffer);
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


