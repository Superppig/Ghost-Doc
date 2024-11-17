using UnityEngine;
public class ProcedureLevelStart : ProcedureBase
{
    //Debug
    private float timer;
    private float waitTime = 3f;
    public override void OnInit()
    {
    }

    public override void OnEnter()
    {
        //展示ui

    }

    public override void OnUpdate()
    {
        timer+=Time.deltaTime;
        if(timer>=waitTime)
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