using UnityEngine;

public class CrazyBiteEnemyHitState : CrazyBiteStateBase
{
    private float HitTime => blackboard.hitTime;
    private float hitTimer;

    public CrazyBiteEnemyHitState(Enemy enemy) : base(enemy)
    {
    }

    public override void OnInit()
    {
    }

    public override void OnEnter()
    {
        hitTimer = 0f;

        enemy.anim.SetBool("Hit", true);
    }



    public override void OnExit()
    {
        enemy.anim.SetBool("Hit", false);
    }

    public override void OnShutdown()
    {
        throw new System.NotImplementedException();
    }

    public override void OnUpdate()
    {
    }

    public override void OnCheck()
    {
    }

    public override void OnFixedUpdate()
    {
        hitTimer += Time.fixedDeltaTime;
        if (hitTimer > HitTime)
        {
            blackboard.isHit = false;
            CurrentFsm.ChangeState<CrazyBiteEnemyIdelState>();
        }
    }
}