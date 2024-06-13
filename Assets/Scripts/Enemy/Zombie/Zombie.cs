using Mono.CompilerServices.SymbolWriter;
using UnityEngine;

public class Zombie : Enemy
{
    public EnemyType enemyType = EnemyType.Zombie;
    private float attackTimer;
    
    protected override void Awake()
    {
        base.Awake();
        
        fsm.AddState(IEnemyState.Idle, new ZombieIdelState(this));
        fsm.AddState(IEnemyState.Attack, new ZombieAttackState(this));
        fsm.AddState(IEnemyState.BeThorwn, new ZombieBeTrownState(this));
        fsm.AddState(IEnemyState.Chase, new ZombieChaseState(this));
        fsm.AddState(IEnemyState.Dead, new ZombieDeadState(this));
        fsm.AddState(IEnemyState.Stagger, new ZombieStaggerState(this));
        fsm.AddState(IEnemyState.Hit, new ZombieHitState(this));

    }
    protected override void Start()
    {
        base.Start();
        fsm.SwitchState(IEnemyState.Idle);
    }
    
    protected override void Update()
    {
        StateChange();
        
        fsm.OnCheck();
        fsm.OnUpdate();
        blackboard.current = fsm.current;
        blackboard.distanceToPlayer = DistanceToPlayer();
        blackboard.dirToPlayer = DirToPlayer();
    }
    protected override void FixedUpdate()
    {
        fsm.OnFixUpdate();
        if (blackboard.current!=IEnemyState.Attack)
        {
            attackTimer += Time.fixedDeltaTime;
        }
    }

    void StateChange()
    {
        //在这里编写逻辑
        if (!blackboard.isHit && blackboard.weakHealth > 0)
        {
            if (blackboard.current == IEnemyState.Hit)
            {
                fsm.SwitchState(IEnemyState.Idle);
            }
            //Idle
            if (blackboard.current == IEnemyState.Idle)
            {
                //Idel->Attack
                if (attackTimer > blackboard.attackTime)
                {
                    fsm.SwitchState(IEnemyState.Chase);
                }
            }
            //切换到Stagger
            if (blackboard.commonHealth <= 0&&blackboard.weakHealth>0)
            {
                fsm.SwitchState(IEnemyState.Stagger);
            }
            //Chase和Attack之间的切换
            if(blackboard.isAttack)
            {
                fsm.SwitchState(IEnemyState.Attack);
                attackTimer = 0f;
            }
        
            if(blackboard.current==IEnemyState.Attack&&blackboard.hasAttack)
            {
                fsm.SwitchState(IEnemyState.Idle);
                blackboard.hasAttack = false;
            }
            if (blackboard.current == IEnemyState.Chase)
            {
                if(blackboard.distanceToPlayer<blackboard.attackRange)
                {
                    blackboard.isAttack= true;
                }
            }
        }
        //死亡
        else if(blackboard.weakHealth<= 0)
        {
            fsm.SwitchState(IEnemyState.Dead);
        }
        else if(blackboard.current!=IEnemyState.Hit&&blackboard.isHit)
        {
            fsm.SwitchState(IEnemyState.Hit);
        }
        
    }
    public override void TakeDamage(float damage)
    {
        blackboard.commonHealth -= damage;
        if(blackboard.commonHealth<=0)
        {
            blackboard.weakHealth += blackboard.commonHealth;
        }
        
        blackboard.isHit = true;
    }
}
