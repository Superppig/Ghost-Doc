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
        //public RenderTexture InteractRT;
    }
    class InteractPass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "InteractPass";
        Material mat;
        Setting m_setting;
        int interactRT = Shader.PropertyToID("_InteractRT");
        int prevRT = Shader.PropertyToID("_PrevRT");

        //RenderTexture rt1;
        //RenderTexture prevRT;


        public InteractPass(Setting setting)
        {
            m_setting = setting;
            this.renderPassEvent = m_setting.passEvent;
            mat = m_setting.PaintMat;
            
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(m_setting.texSize, m_setting.texSize, RenderTextureFormat.RFloat);
            cmd.GetTemporaryRT(interactRT, descriptor);
            cmd.GetTemporaryRT(prevRT, descriptor);
            //ConfigureTarget(interactRT, BuiltinRenderTextureType.RenderTexture);
            //ConfigureClear(ClearFlag.All, Color.black);
            //if (rt1 == null)
            //{
            //    rt1 = new RenderTexture(m_setting.texSize, m_setting.texSize, 0, RenderTextureFormat.RFloat);
            //}
            //if (prevRT == null)
            //{
            //    prevRT = new RenderTexture(m_setting.InteractRT.descriptor);
            //}

            //Shader.SetGlobalTexture("_PrevRT", rt2);
            //Shader.SetGlobalTexture("_InteractRT", rt1);
            //Shader.SetGlobalTexture("_InteractRT",m_setting.InteractRT);
            //cmd.GetTemporaryRT(prevRT, m_setting.InteractRT.descriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isSceneViewCamera) return;
            CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);
            cmd.Blit(interactRT, prevRT);
            cmd.Blit(prevRT, interactRT, mat);
            //cmd.Blit(rt1, rt2);
            //cmd.Blit(rt2, rt1, mat);
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


