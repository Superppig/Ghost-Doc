using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

public class InteractRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Setting
    {
        public RenderPassEvent passEvent = RenderPassEvent.BeforeRendering;
        public int texSize = 1024;
        public Material PaintMat;
        public RenderTexture InteractRT;
    }
    class InteractPass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "InteractPass";
        Material mat;
        Setting m_setting;
        int prevRT = Shader.PropertyToID("_PrevRT");
        RenderTexture interactRT;

        //RenderTexture rt1;
        //RenderTexture prevRT;


        public InteractPass(Setting setting)
        {
            m_setting = setting;
            this.renderPassEvent = m_setting.passEvent;
            mat = m_setting.PaintMat;
            interactRT = m_setting.InteractRT;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = interactRT.descriptor;

            cmd.GetTemporaryRT(prevRT, descriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isSceneViewCamera) return;
            if (interactRT == null || m_setting.InteractRT == null) return;

            CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);
            cmd.SetGlobalTexture("_InteractRT", interactRT);

            cmd.Blit(interactRT, prevRT);
            cmd.Blit(prevRT, interactRT, mat, 0);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);


        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(prevRT);
        }

    }

    InteractPass m_Pass;
    public Setting setting;

    public override void Create()
    {
        m_Pass = new InteractPass(setting);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_Pass);
    }
}


