using System.Collections.Generic;
using Services;
using UnityEngine;
public class CrazyBiteEnemy : Enemy
{
    public EnemyType enemyType = EnemyType.CrazyBiteEnemy;

    private BuffSystem _buffSystem;
    
    //CrazyBiteEnemy
    public float biteSpeed =10f; //撕咬速度
    public float biteReadyTime = 0.25f;//撕咬准备时间
    public float biteRange = 3f;//撕咬范围
    public float biteTime = 0.25f;//撕咬时间
    public float attackTime = 1f;//攻击间隔

    //状态机
    private FsmManager fsmManager;
    private Fsm<CrazyBiteEnemy> fsm;

    //投掷
    public bool isThrown;
    
    protected override void Awake()
    {
        base.Awake();
        fsmManager = ServiceLocator.Get<FsmManager>();
        _buffSystem = ServiceLocator.Get<BuffSystem>();
        List<FsmState<CrazyBiteEnemy>> states = new List<FsmState<CrazyBiteEnemy>>()
        {
            new CrazyBiteEnemyIdelState(this),
            new CrazyBiteEnemyChaseState(this),
            new CrazyBiteEnemyAttackState(this),
            new CrazyBiteEnemyStaggerState(this),
            new CrazyBiteEnemyHitState(this), 
            new CrazyBiteEnemyDeadState(this),
        };

        fsm =  fsmManager.CreateFsm(this, states.ToArray());
        fsm.Start<CrazyBiteEnemyIdelState>();
    }

    protected override void EnemyOnInable()
    {
        base.EnemyOnInable();
        fsm.ChangeState<CrazyBiteEnemyIdelState>();

        enemyCore.enemyType = enemyType;
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
        return hasCore&&canGrab;
    }

    public override bool CanUse()
    {
        return true;
    }

    public override void Use()
    {
        _buffSystem.ActivateBuff(BuffType.KenKen);
        fsm.ChangeState<CrazyBiteEnemyDeadState>();
    }
}