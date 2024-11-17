public class RemoteEnemyBeTrownState:RemoteEnemyStateBase
{
    public RemoteEnemyBeTrownState(Enemy enemy) : base(enemy)
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
    }

    public override void OnCheck()
    {
    }

    public override void OnFixedUpdate()
    {
    }
    
}
