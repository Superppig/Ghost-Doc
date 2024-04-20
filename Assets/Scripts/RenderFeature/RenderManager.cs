using Services;
using Services.Event;
using System;
using UnityEngine;

public class RenderManager : Service
{
    public override Type RegisterType => GetType();
    [AutoService]
    private IEventSystem eventSystem;

    [SerializeField]
    private RenderManagerSettings settings;

    protected override void Awake()
    {
        base.Awake();
        settings.Initialize();
    }

    protected internal override void Init()
    {
        base.Init();
        eventSystem.AddListener<int>(EEvent.AfterLoadScene, AfterLoadScene);
    }

    private void OnDestroy()
    {
        eventSystem.RemoveListener<int>(EEvent.AfterLoadScene, AfterLoadScene);
    }

    private void AfterLoadScene(int sceneIndex)
    {
        settings.TryUpdateRenderData(sceneIndex);
    }
}
