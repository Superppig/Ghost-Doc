public class RemoteEnemyIdelState:RemoteEnemyStateBase
{
    public RemoteEnemyIdelState(Enemy enemy) : base(enemy)
    {
    }

    public override void OnInit()
    {
    }

    public override void OnEnter()
    {
        CurrentFsm.ChangeState<RemoteEnemyChaseState>();
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
