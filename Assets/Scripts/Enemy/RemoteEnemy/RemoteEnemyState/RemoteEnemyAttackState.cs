using UnityEngine;

public enum EnemyType
{
    Zombie,
    RemoteEnemy,
}

public class RemoteEnemyAttackState:EnemyStateBase
{
    RemoteEnemy remoteEnemy => enemy as RemoteEnemy;
    private float animTime;
    private float animTimer;
    public RemoteEnemyAttackState(Enemy enemy) : base(enemy)
    {
    }
    public override void OnEnter()
    {
        enemy.anim.SetBool("Attack",true);
        animTime = enemy.anim.GetCurrentAnimatorStateInfo(0).length;
        animTimer = 0;
        
        blackboard.isAttack = false;
    }

    public override void OnExit()
    {
    }

    public override void OnUpdate()
    {
        
    }

    public override void OnCheck()
    {
    }

    public override void OnFixUpdate()
    {
        animTimer+= Time.fixedDeltaTime;
        if(animTimer>animTime)
        {
            enemy.anim.SetBool("Attack",false);
            remoteEnemy.Fire();
            blackboard.hasAttack = true;
        }
    }
}
