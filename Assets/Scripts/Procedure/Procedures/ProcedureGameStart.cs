using UnityEngine;

public class ProcedureGameStart : ProcedureBase
{
    public override void OnInit()
    {
    }

    public override void OnEnter()
    {
        Debug.Log("GhostDoc:Start");
        CurrentFsm.ChangeState<ProcedureLevelStart>();
    }

    public override void OnUpdate()
    {
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