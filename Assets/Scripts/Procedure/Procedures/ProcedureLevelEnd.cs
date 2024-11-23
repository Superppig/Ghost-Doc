using Services;
using Services.Event;

public class ProcedureLevelEnd : ProcedureBase
{
    public bool newLevel;
    private IEventSystem eventSystem;
    private UIManager uiManager;
    public override void OnInit()
    {
        eventSystem = ServiceLocator.Get<IEventSystem>();
        uiManager = ServiceLocator.Get<UIManager>();
    }

    public override void OnEnter()
    {
        //显示ui
        eventSystem.AddListener(EEvent.NextLevel, NextLevel);
    }

    public override void OnUpdate()
    {
        if (newLevel)
        {
            CurrentFsm.ChangeState<ProcedureLevelStart>();
            newLevel = false;
        }
    }

    public override void OnFixedUpdate()
    {
    }

    public override void OnExit()
    {
        eventSystem.RemoveListener(EEvent.NextLevel, NextLevel);
        uiManager.CloseView<LevelCompeleteUI>();
    }

    public override void OnShutdown()
    {
    }

    public override void OnCheck()
    {
    }
    
    
    void NextLevel()
    {
        newLevel = true;
    }
}