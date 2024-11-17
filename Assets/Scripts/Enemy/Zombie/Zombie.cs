using System;
using System.Collections.Generic;
using Mono.CompilerServices.SymbolWriter;
using Services;
using UnityEngine;

public class Zombie : Enemy,IGrabObject
{
    //Zombie
    public float attackRange = 2f;//攻击范围
    public float attackTime = 1f;//攻击间隔
    
    public EnemyType enemyType = EnemyType.Zombie;
    
    //状态机
    private FsmManager fsmManager;
    private Fsm<Zombie> fsm;

    
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
            new ZombieBeTrownState(this),
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

    public Transform GetTransform()
    {
        return transform;
    }

    public void Grabbed()
    {
        rb.isKinematic = true;
        col.enabled = false;
    }

    public void Released()
    {
        rb.isKinematic = false;
        col.enabled = true;
        rb.constraints = RigidbodyConstraints.None;
    }

    public void Fly(Vector3 dir, float force)
    {
        Debug.Log("Fly");
        fsm.ChangeState<ZombieBeTrownState>();
        rb.AddForce(dir * force, ForceMode.Impulse);
    }

    public bool CanGrab()
    {
        return canGrab;
    }

    public bool CanUse()
    {
        return true;
    }

    public void Use()
    {
        BuffSystem.Instance.ActivateBuff(BuffType.Zombie);
        fsm.ChangeState<ZombieDeadState>();
    }


    private void OnCollisionEnter(Collision other)
    {
        if(isThrown)
        {
            ScreenControl.Instance.ParticleRelease(blackboard.boom,transform.position,Vector3.zero);
            Boom();
            fsm.ChangeState<ZombieDeadState>();
        }
    }
}
