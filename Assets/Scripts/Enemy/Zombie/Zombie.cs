using System.Collections.Generic;
using Services;
using UnityEngine;

public class Zombie : Enemy
{
    //Zombie
    public float attackRange = 2f;//攻击范围
    public float attackTime = 1f;//攻击间隔
    
    public EnemyType enemyType = EnemyType.Zombie;
    
    //状态机
    private FsmManager fsmManager;
    private Fsm<Zombie> fsm;

    private BuffSystem _buffSystem;
    
    //投掷
    public bool isThrown;
    protected override void Awake()
    {
        base.Awake();
        fsmManager = ServiceLocator.Get<FsmManager>();
        

        List<FsmState<Zombie>> states = new List<FsmState<Zombie>>()
        {
            new ZombieIdelState(this),
            new ZombieChaseState(this),
            new ZombieAttackState(this),
            new ZombieStaggerState(this),
            new ZombieHitState(this),
            new ZombieDeadState(this)
        };
        fsm = fsmManager.CreateFsm(this, states.ToArray());
        fsm.Start<ZombieIdelState>();
    }

    protected override void EnemyOnInable()
    {
        base.EnemyOnInable();
        fsm.ChangeState<ZombieIdelState>();
        
        enemyCore.enemyType = EnemyType.Zombie;
    }

    protected override void Update()
    {
        StateChange();
        
        fsm.OnCheck();
        fsm.OnUpdate();
        blackboard.distanceToPlayer = DistanceToPlayer();
        blackboard.dirToPlayer = DirToPlayer();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        fsm.OnFixedUpdate();
    }

    void StateChange()
    {
        
    }
    public override void TakeDamage(float damage)
    {
        if(blackboard.currentHealth>blackboard.weakHealth&& blackboard.currentHealth-damage<=blackboard.weakHealth)
        {
            blackboard.currentHealth = blackboard.weakHealth;
        }
        else
        {
            blackboard.currentHealth -= damage;
        }
        blackboard.isHit = true;
    }

    public override Transform GetTransform()
    {
        return transform;
    }

    public override void Grabbed()
    {
        rb.isKinematic = true;
        col.enabled = false;
    }

    public override void Released()
    {
        rb.isKinematic = false;
        col.enabled = true;
        rb.constraints = RigidbodyConstraints.None;
    }

    public override void Fly(Vector3 dir, float force)
    {
    }

    public override bool CanGrab()
    {
        return hasCore && canGrab;
    }

    public override bool CanUse()
    {
        return true;
    }

    public override void Use()
    {
        _buffSystem.ActivateBuff(BuffType.Zombie);
        fsm.ChangeState<ZombieDeadState>();
    }
}