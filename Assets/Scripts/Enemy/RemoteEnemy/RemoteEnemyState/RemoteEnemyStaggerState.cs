public class RemoteEnemyStaggerState:RemoteEnemyStateBase
{
    public RemoteEnemyStaggerState(Enemy enemy) : base(enemy)
    {
    }
    public override void OnInit()
    {
    }
    public override void OnEnter()
    {
        enemy.canGrab = true;
    }
    public override void OnFixedUpdate()
    {
    }
    public override void OnExit()
    {
        enemy.canGrab = false;
    }
    public override void OnShutdown()
    {
    }
    public override void OnUpdate()
    {
        if(blackboard.currentHealth<=0)
        {
            CurrentFsm.ChangeState<RemoteEnemyDeadState>();
        }
    }
    public override void OnCheck()
    {
    }
}