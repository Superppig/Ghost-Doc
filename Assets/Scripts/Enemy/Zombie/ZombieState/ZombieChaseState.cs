using UnityEngine;
using DG.Tweening;
public class ZombieChaseState:ZombieStateBase
{
    public ZombieChaseState(Enemy enemy) : base(enemy)
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
    }

    public override void OnShutdown()
    {
    }

    public override void OnUpdate()
    {
        if (enemy.OnGround())
        {
            Chase();
        }
        //死亡
        if(blackboard.currentHealth<= 0f)
        {
            CurrentFsm.ChangeState<ZombieDeadState>();
        }
        
        //切换到Stagger
        if (blackboard.currentHealth<= blackboard.weakHealth&&blackboard.currentHealth>0)
        {
            CurrentFsm.ChangeState<ZombieStaggerState>();
        }
        else if(blackboard.isHit)
        {
            CurrentFsm.ChangeState<ZombieHitState>();
        }
        
        if(blackboard.distanceToPlayer <= CurrentFsm.Owner.attackRange)
        {
            CurrentFsm.ChangeState<ZombieAttackState>();
        }
        
    }

    public override void OnCheck()
    {
    }

    public override void OnFixedUpdate()
    {
    }
    void Chase()
    {
        enemy.Find(blackboard.player.transform.position);
    }
}