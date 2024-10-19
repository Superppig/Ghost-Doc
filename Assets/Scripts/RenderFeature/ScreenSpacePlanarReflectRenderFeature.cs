using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenSpacePlanarReflectRenderFeature : ScriptableRendererFeature
{

    [System.Serializable]
    public class Setting
    {
        public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public ComputeShader shader;
        public int RTSize = 512;
        public float ReflectHeight = 0.0f;
    }

    [SerializeField]
    Setting m_settings = new Setting();
    CustomRenderPass m_pass;

    //管线初始化时,创建RenderFeature时调用,用于初始化Feature
    public override void Create()
    {
        if (m_pass == null)
        {
            m_pass = new CustomRenderPass();
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.isPreviewCamera) return;
        //if (renderingData.cameraData.isSceneViewCamera) return;
        bool shouldAdd = m_pass.Setup(ref m_settings,renderer.cameraColorTarget);
        if (shouldAdd)
        {
            renderer.EnqueuePass(m_pass);
        }
    }

    //管线资源销毁时调用,用于释放Feature的资源
    protected override void Dispose(bool disposing)
    {
        m_pass?.Dispose();
        m_pass = null;
    }

    class CustomRenderPass : ScriptableRenderPass
    {
        const string tag = "SSPR";

        Setting m_settings;
        ProfilingSampler m_sampler = new ProfilingSampler(tag);

        int ReflectRT = Shader.PropertyToID("_ReflectRT");
        int ReflectDepthBuffer = Shader.PropertyToID("_ReflectDepthBuffer");

        RenderTargetIdentifier currentColor;
        int groupX;
        int groupY;
        int width;
        int height;

        public CustomRenderPass()
        {

        }

        public bool Setup(ref Setting settings, in RenderTargetIdentifier target)
        {
            if (settings.RenderPassEvent > RenderPassEvent.BeforeRenderingPostProcessing)
            {
                this.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            }
            else
            {
                this.renderPassEvent = settings.RenderPassEvent;
            }
            m_settings = settings;
            currentColor = target;
            return settings.shader != null;
        }

        //管线准备开始为每个相机渲染场景之前调用
        //主要用来获取RT和设置渲染目标
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var cameraDesc = renderingData.cameraData.cameraTargetDescriptor;
            float aspect = (float)cameraDesc.width / (float)cameraDesc.height;
            int groupThreadX = 8;
            int groupThreadY = 8;
            height = m_settings.RTSize;
            width = Mathf.RoundToInt(m_settings.RTSize * aspect);
            groupX = width / groupThreadX;
            groupY = height / groupThreadY;
            var desc = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32);
            desc.enableRandomWrite = true;
            cmd.GetTemporaryRT(ReflectRT, desc);
            desc.colorFormat = RenderTextureFormat.R8;
            cmd.GetTemporaryRT(ReflectDepthBuffer, desc);
            ConfigureTarget(ReflectRT, ReflectDepthBuffer);
            ConfigureClear(ClearFlag.All, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            using (new ProfilingScope(cmd, m_sampler))
            {
                Render(cmd, ref renderingData);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var cs = m_settings.shader;
            cmd.SetComputeVectorParam(cs, "_RTSize", new Vector4(width, height, 1, 1));
            cmd.SetComputeFloatParam(cs, "_ReflectHeight", m_settings.ReflectHeight);
            cmd.SetComputeTextureParam(cs, 0, "_ReflectRT", ReflectRT);
            cmd.SetComputeTextureParam(cs, 0, "_ReflectDepthBuffer", ReflectDepthBuffer);
            cmd.SetComputeTextureParam(cs, 0, "_CameraColorRT", renderingData.cameraData.renderer.cameraColorTarget);
            cmd.SetComputeTextureParam(cs, 0, "_CameraDepthRT", new RenderTargetIdentifier("_CameraDepthTexture"));
            cmd.DispatchCompute(cs, 0, groupX, groupY, 1);

        }

        //每帧渲染完成后调用
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(ReflectRT);
            cmd.ReleaseTemporaryRT(ReflectDepthBuffer);
        }

        public void Dispose()
        {
        }
    }
}



