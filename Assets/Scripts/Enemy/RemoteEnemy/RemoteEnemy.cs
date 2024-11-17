using System.Collections.Generic;
using Services;
using UnityEngine;


public class RemoteEnemy:Enemy ,IGrabObject
{
    public EnemyType enemyType = EnemyType.RemoteEnemy;
    
    public bool isThrown;

    //RemoteEnemy
    public float MaxFireRange = 15f;//最大射程
    public float MinFireRange = 10f;//逃离范围
    public float fireTime = 1f;//攻击间隔
    public EnemyBullet bullet;//子弹
    public float attackTime = 1f;//攻击间隔

    
    
    //状态机
    private FsmManager fsmManager;
    private Fsm<RemoteEnemy> fsm;

    protected override void Awake()
    {
        base.Awake();
        
        fsmManager = ServiceLocator.Get<FsmManager>();
        List<FsmState<RemoteEnemy>> states = new List<FsmState<RemoteEnemy>>()
        {
            new RemoteEnemyIdelState(this),
            new RemoteEnemyChaseState(this),
            new RemoteEnemyAttackState(this),
            new RemoteEnemyStaggerState(this),
            new RemoteEnemyBeTrownState(this),
            new RemoteEnemyHitState(this),
            new RemoteEnemyDeadState(this)
        };
        fsm = fsmManager.CreateFsm(this, states.ToArray());
        fsm.Start<RemoteEnemyIdelState>();
    }

    protected override void EnemyOnInable()
    {
        base.EnemyOnInable();
        fsm.ChangeState<RemoteEnemyIdelState>();
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
        blackboard.currentHealth -= damage;
        
        blackboard.isHit = true;
    }


    public void Fire()
    {
        EnemyBullet bullet1 = Instantiate(bullet, transform.position, Quaternion.identity);
        bullet1.dir = (blackboard.player.transform.position - transform.position).normalized;
        bullet1.damage = blackboard.damage;
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
        fsm.ChangeState<RemoteEnemyBeTrownState>();
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
        BuffSystem.Instance.ActivateBuff(BuffType.Remote);
        
        //TODO:机关炮

        fsm.ChangeState<RemoteEnemyDeadState>();
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if(isThrown)
        {
            ScreenControl.Instance.ParticleRelease(blackboard.boom,transform.position,Vector3.zero);
            Boom();
            fsm.ChangeState<RemoteEnemyDeadState>();
        }
    }
}