using UnityEngine;

public class CrazyBiteEnemyAttackState : CrazyBiteStateBase
{
    private float animTime;
    private float timer;
    
    private float BiteReadyTime => CurrentFsm.Owner.biteReadyTime;
    private float BiteTime => CurrentFsm.Owner.biteTime;
    private float BiteSpeed => CurrentFsm.Owner.biteSpeed;
    private Vector3 biteDir;

    private bool readyBite = false;
    public CrazyBiteEnemyAttackState(Enemy enemy) : base(enemy)
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
        
        biteDir = (blackboard.player.transform.position - enemy.transform.position).normalized;
        
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
            CurrentFsm.ChangeState<CrazyBiteEnemyDeadState>();
        }
        
        if (blackboard.currentHealth<= blackboard.weakHealth&&blackboard.currentHealth>0)
        {
            CurrentFsm.ChangeState<CrazyBiteEnemyStaggerState>();
        }
        else if(blackboard.isHit)
        {
            CurrentFsm.ChangeState<CrazyBiteEnemyHitState>();
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
        
        if(timer > BiteReadyTime)
        {
            enemy.rb.velocity = biteDir * BiteSpeed;
            if (timer < BiteReadyTime + CurrentFsm.Owner.attackTime)
            {
                enemy.rb.velocity = Vector3.zero;
            }
        }
        if (timer >= BiteReadyTime + CurrentFsm.Owner.attackTime)
        {
            CurrentFsm.ChangeState<CrazyBiteEnemyChaseState>();
        }
    }
}