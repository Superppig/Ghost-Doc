using System.Collections.Generic;
using Services;
using UnityEngine;
public class CrazyBiteEnemy : Enemy,IGrabObject
{
    public EnemyType enemyType = EnemyType.CrazyBiteEnemy;
    
    
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
        List<FsmState<CrazyBiteEnemy>> states = new List<FsmState<CrazyBiteEnemy>>()
        {
            new CrazyBiteEnemyIdelState(this),
            new CrazyBiteEnemyChaseState(this),
            new CrazyBiteEnemyAttackState(this),
            new CrazyBiteEnemyStaggerState(this),
            new CrazyBiteEnemyBeTrownState(this),
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
        fsm.ChangeState<CrazyBiteEnemyBeTrownState>();
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
        BuffSystem.Instance.ActivateBuff(BuffType.KenKen);
        fsm.ChangeState<CrazyBiteEnemyDeadState>();
    }


    private void OnCollisionEnter(Collision other)
    {
        if(isThrown)
        {
            ScreenControl.Instance.ParticleRelease(blackboard.boom,transform.position,Vector3.zero);
            Boom();
            fsm.ChangeState<CrazyBiteEnemyDeadState>();
        }
    }
}