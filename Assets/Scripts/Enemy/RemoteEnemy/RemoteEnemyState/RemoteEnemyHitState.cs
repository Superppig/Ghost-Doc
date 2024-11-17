using UnityEngine;

public class RemoteEnemyHitState:RemoteEnemyStateBase
{
    private float HitTime => blackboard.hitTime;
    private float hitTimer;
    public RemoteEnemyHitState(Enemy enemy) : base(enemy)
    {
    }

    public override void OnInit()
    {
    }

    public override void OnEnter()
    {
        hitTimer = 0f;
        
        enemy.anim.SetBool("Hit",true);
    }

    public override void OnExit()
    {
        enemy.anim.SetBool("Hit",false);
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
        hitTimer+= Time.fixedDeltaTime;
        if(hitTimer>HitTime)
        {
            CurrentFsm.ChangeState<RemoteEnemyIdelState>();
            blackboard.isHit = false;
        }
    }
}
