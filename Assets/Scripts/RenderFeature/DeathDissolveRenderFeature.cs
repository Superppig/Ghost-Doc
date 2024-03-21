using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DeathDissolveRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public string name = "DeathDissolvePass";
        public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Material material;
    }

    public Settings settings = new Settings();


    DeathDissolvePass m_ScriptablePass;

    public override void Create()
    {
        this.name = settings.name;
        m_ScriptablePass = new DeathDissolvePass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }

    class DeathDissolvePass : ScriptableRenderPass
    {
        // 设置渲染 Tags
        static readonly string k_RenderTag = "DeathDissolvePass";
        int temp01 = Shader.PropertyToID("dissolvetemp");

        // 后处理使用材质
        Material effMat;
        DeathDissolve effVolum;

        RenderTargetIdentifier currentTarget;   // 设置当前渲染目标

        public void Setup(in RenderTargetIdentifier currentTarget)
        {
            this.currentTarget = currentTarget;
        }

        public DeathDissolvePass(Settings setting)
        {
            renderPassEvent = setting.passEvent;
            if (setting.material == null )
                return;
            effMat = setting.material;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (effMat == null) return;

            if (!renderingData.cameraData.postProcessEnabled) return;


            effVolum = VolumeManager.instance.stack.GetComponent<DeathDissolve>();
            if (effVolum == null) return;
            if (!effVolum.UseThis.value) return;
            if (effVolum.UseSceneCamera.value)
            {

            }
            else
            {
                if (renderingData.cameraData.isSceneViewCamera) return;//不处理scene的相机
            }
            if (!effVolum.active)
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);
            //设置渲染函数
            Render(cmd, ref renderingData);

            //执行函数
            context.ExecuteCommandBuffer(cmd);
            //释放
            cmd.ReleaseTemporaryRT(temp01);
            CommandBufferPool.Release(cmd);
        }

        //定义一个渲染函数
        void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //获取摄像机属性
            ref var cameraData = ref renderingData.cameraData;
            //传入摄像机描述信息
            var desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            cmd.GetTemporaryRT(temp01, desc);
            effMat.SetColor("_OutEdgeColor",effVolum.OutEdgeColor.value);
            effMat.SetColor("_InEdgeColor",effVolum.InEdgeColor.value);
            effMat.SetColor("_BackColor",effVolum.BackColor.value);
            effMat.SetFloat("_Control",effVolum.Control.value);
            effMat.SetFloat("_BigEdgeWidth",effVolum.BigEdgeWidth.value);
            effMat.SetFloat("_SmallEdgeWidth", effVolum.SmallEdgeWidth.value);
            cmd.Blit(currentTarget, temp01,effMat,0);
            cmd.Blit(temp01, currentTarget);
        }

    }
}


