using System;
using System.Collections.Generic;
using Services;
public class ProcedureManager :Service,IService
{
    public override Type RegisterType => typeof(ProcedureManager);
    
    private FsmManager fsmManager;

    private Fsm<ProcedureManager> fsm;

    protected internal override void Init()
    {
        base.Init();
        fsmManager = ServiceLocator.Get<FsmManager>();
        
        //注册流程
        List<ProcedureBase> procedures = new List<ProcedureBase>()
        {
            new ProcedureGameStart(),
            new ProcedureLevelStart(),
            new ProcedureLevelRun(),
            new ProcedureLevelEnd(),
        };

        fsm = fsmManager.CreateFsm(this, procedures.ToArray());
        fsm.Start<ProcedureGameStart>();
    }

    private void Update()
    {
        fsm.OnCheck();
        fsm.OnUpdate();
    }
    private void FixedUpdate()
    {
        fsm.OnFixedUpdate();
    }
}
