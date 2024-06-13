using UnityEngine;
using DG.Tweening;
public class RemoteEnemyChaseState:EnemyStateBase
{
    public RemoteEnemyChaseState(Enemy enemy) : base(enemy)
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

    public override void OnUpdate()
    {
        Chase();
    }

    public override void OnCheck()
    {
    }

    public override void OnFixUpdate()
    {
    }


    void Chase()
    {
        if (blackboard.distanceToPlayer > blackboard.MaxFireRange)
        {
            //找到最近的射击点
            Vector3 target = blackboard.player.transform.position +
                             (enemy.transform.position - blackboard.player.transform.position).normalized *
                             blackboard.MaxFireRange;
            enemy.Find(target);
        }
        else if (blackboard.distanceToPlayer < blackboard.MinFireRange)
        {
            //找到最近的射击点
            Vector3 target = blackboard.player.transform.position +
                             (enemy.transform.position - blackboard.player.transform.position).normalized *
                             blackboard.MinFireRange;
            enemy.Find(target);
        }
    }
}