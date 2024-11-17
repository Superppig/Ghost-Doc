using UnityEngine;



public class RemoteEnemyAttackState:RemoteEnemyStateBase
{
    RemoteEnemy remoteEnemy => enemy as RemoteEnemy;
    private float animTime;
    private float timer;
    public RemoteEnemyAttackState(Enemy enemy) : base(enemy)
    {
    }

    public override void OnInit()
    {
        
    }

    public override void OnEnter()
    {
        enemy.anim.SetBool("Attack",true);
        animTime = enemy.anim.GetCurrentAnimatorStateInfo(0).length;
        timer = 0;
        remoteEnemy.Fire();
        blackboard.isAttack = false;
    }

    public override void OnExit()
    {
    }

    public override void OnShutdown()
    {
    }

    public override void OnUpdate()
    {
        //死亡
        if(blackboard.currentHealth<= 0f)
        {
            CurrentFsm.ChangeState<RemoteEnemyDeadState>();
        }

        //切换到Stagger
        if (blackboard.currentHealth<= blackboard.weakHealth&&blackboard.currentHealth>0)
        {
            CurrentFsm.ChangeState<RemoteEnemyStaggerState>();
        }
        else if(blackboard.isHit)
        {
            CurrentFsm.ChangeState<RemoteEnemyHitState>();
        }
    }

    public override void OnCheck()
    {
    }

    public override void OnFixedUpdate()
    {
        timer+= Time.fixedDeltaTime;
        if(timer>animTime)
        {
            enemy.anim.SetBool("Attack",false);
            blackboard.hasAttack = true;
        }
        
        if(timer> CurrentFsm.Owner.attackTime + animTime)
        {
            CurrentFsm.ChangeState<RemoteEnemyChaseState>();
        }
    }
}
