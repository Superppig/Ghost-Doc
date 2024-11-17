using UnityEngine;

public class CrazyBiteEnemyStaggerState : CrazyBiteStateBase
{
    public CrazyBiteEnemyStaggerState(Enemy enemy) : base(enemy)
    {
    }

    public override void OnInit()
    {
    }

    public override void OnEnter()
    {
        enemy.canGrab = true;
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
    }

    public override void OnCheck()
    {
    }

    public override void OnFixedUpdate()
    {
    }
}