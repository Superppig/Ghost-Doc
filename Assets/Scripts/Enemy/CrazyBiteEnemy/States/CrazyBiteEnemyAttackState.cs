using UnityEngine;

public class CrazyBiteEnemyAttackState : EnemyStateBase
{
    private float animTime;
    private float timer;
    
    private float BiteReadyTime => blackboard.biteReadyTime;
    private float BiteTime => blackboard.biteTime;
    private float BiteSpeed => blackboard.biteSpeed;
    private Vector3 biteDir;

    private bool readyBite = false;
    public CrazyBiteEnemyAttackState(Enemy enemy) : base(enemy)
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
        enemy.rb.velocity = Vector3.zero;
    }

    public override void OnUpdate()
    {
    }

    public override void OnCheck()
    {
    }
    public override void OnFixUpdate()
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
        }
    }
}