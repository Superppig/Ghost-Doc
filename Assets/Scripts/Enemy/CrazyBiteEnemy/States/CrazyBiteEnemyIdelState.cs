using UnityEngine;

public class CrazyBiteEnemyIdelState : CrazyBiteStateBase
{
    public CrazyBiteEnemyIdelState(Enemy enemy) : base(enemy)
    {
    }

    public override void OnInit()
    {
    }

    public override void OnEnter()
    {
        CurrentFsm.ChangeState<CrazyBiteEnemyChaseState>();
    }

    public override void OnExit()
    {
    }

    public override void OnShutdown()
    {
    }

    public override void OnUpdate()
    {
    }

    public override void OnCheck()
    {
    }

    public override void OnFixedUpdate()
    {
    }
}