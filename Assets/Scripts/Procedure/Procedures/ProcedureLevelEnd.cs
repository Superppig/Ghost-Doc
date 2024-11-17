public class ProcedureLevelEnd : ProcedureBase
{
    public bool newLevel;
    public override void OnInit()
    {
    }

    public override void OnEnter()
    {
        //显示ui
    }

    public override void OnUpdate()
    {
        if (newLevel)
        {
            CurrentFsm.ChangeState<ProcedureLevelRun>();
        }
    }

    public override void OnFixedUpdate()
    {
    }

    public override void OnExit()
    {
    }

    public override void OnShutdown()
    {
    }

    public override void OnCheck()
    {
    }
}