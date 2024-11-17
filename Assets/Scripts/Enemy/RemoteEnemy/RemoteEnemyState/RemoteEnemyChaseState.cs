using UnityEngine;
using DG.Tweening;
public class RemoteEnemyChaseState:RemoteEnemyStateBase
{
    public RemoteEnemyChaseState(Enemy enemy) : base(enemy)
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
        //朝向玩家
        Vector3 dir = blackboard.dirToPlayer;
        Vector3 rotation = new Vector3(0,
            Quaternion.FromToRotation(enemy.transform.forward, dir).eulerAngles.y + enemy.transform.rotation.eulerAngles.y, 0);
        enemy.transform.DOLocalRotate(rotation, 0.2f);
    }

    public override void OnShutdown()
    {
    }

    public override void OnUpdate()
    {
        Chase();
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
        
        if(blackboard.distanceToPlayer>=CurrentFsm.Owner.MinFireRange&&blackboard.distanceToPlayer<=CurrentFsm.Owner.MaxFireRange)
        {
            CurrentFsm.ChangeState<RemoteEnemyAttackState>();
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
        if (blackboard.distanceToPlayer > CurrentFsm.Owner.MaxFireRange)
        {
            //找到最近的射击点
            Vector3 target = blackboard.player.transform.position +
                             (enemy.transform.position - blackboard.player.transform.position).normalized *
                             CurrentFsm.Owner.MaxFireRange;
            enemy.Find(target);
        }
        else if (blackboard.distanceToPlayer < CurrentFsm.Owner.MinFireRange)
        {
            //找到最近的射击点
            Vector3 target = blackboard.player.transform.position +
                             (enemy.transform.position - blackboard.player.transform.position).normalized *
                             CurrentFsm.Owner.MinFireRange;
            enemy.Find(target);
        }
    }
}