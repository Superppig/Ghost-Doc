public class CrazyBiteEnemyBeTrownState : CrazyBiteStateBase
{
    public CrazyBiteEnemyBeTrownState(Enemy enemy) : base(enemy)
    {
    }

    public override void OnInit()
    {
    }

    public override void OnEnter()
    {
    }

    public override void OnExit()
    {
        CurrentFsm.Owner.isThrown = false;
    }

    public override void OnShutdown()
    {
    }

    public override void OnUpdate()
    {
        CurrentFsm.Owner.isThrown = true;
        if(blackboard.currentHealth<=0)
        {
            CurrentFsm.ChangeState<CrazyBiteEnemyDeadState>();
        }
    }

    public override void OnCheck()
    {
    }

    public override void OnFixedUpdate()
    {
    }
}