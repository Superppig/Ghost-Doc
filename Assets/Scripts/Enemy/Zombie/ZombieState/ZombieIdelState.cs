public class ZombieIdelState:ZombieStateBase
{
    public ZombieIdelState(Enemy enemy) : base(enemy)
    {
    }

    public override void OnInit()
    {
    }

    public override void OnEnter()
    {
        CurrentFsm.ChangeState<ZombieChaseState>();
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
