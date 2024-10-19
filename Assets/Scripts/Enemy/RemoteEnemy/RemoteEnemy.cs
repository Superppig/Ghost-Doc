
using UnityEngine;
public class RemoteEnemy:Enemy ,IGrabObject
{
    public EnemyType enemyType = EnemyType.RemoteEnemy;

    private float attackTimer;

    protected override void Awake()
    {
        base.Awake();
        
        fsm.AddState(IEnemyState.Idle, new RemoteEnemyIdelState(this));
        fsm.AddState(IEnemyState.Attack, new RemoteEnemyAttackState(this));
        fsm.AddState(IEnemyState.BeThorwn, new RemoteEnemyBeTrownState(this));
        fsm.AddState(IEnemyState.Chase, new RemoteEnemyChaseState(this));
        fsm.AddState(IEnemyState.Dead, new RemoteEnemyDeadState(this));
        fsm.AddState(IEnemyState.Stagger, new RemoteEnemyStaggerState(this));
        fsm.AddState(IEnemyState.Hit, new RemoteEnemyHitState(this));

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
        if (!blackboard.isHit && blackboard.currentHealth> 0f)
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
            if (blackboard.currentHealth<= blackboard.weakHealth&&blackboard.currentHealth>0)
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
                if(blackboard.distanceToPlayer<blackboard.MaxFireRange
                   && blackboard.distanceToPlayer>blackboard.MinFireRange)
                {
                    blackboard.isAttack= true;
                }
            }
        }
        //死亡
        else if(blackboard.currentHealth<= 0f)
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
        blackboard.currentHealth -= damage;
        
        blackboard.isHit = true;
    }


    public void Fire()
    {
        EnemyBullet bullet1 = Instantiate(blackboard.bullet, transform.position, Quaternion.identity);
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
        fsm.SwitchState(IEnemyState.BeThorwn);
        rb.AddForce(dir * force, ForceMode.Impulse);
        blackboard.current = IEnemyState.BeThorwn;
    }
    public bool CanGrab()
    {
        return blackboard.current == IEnemyState.Stagger;
    }
    public bool CanUse()
    {
        return true;
    }

    public void Use()
    {
        BuffSystem.Instance.ActivateBuff(BuffType.Remote);
        
        //TODO:机关炮


        fsm.SwitchState(IEnemyState.Dead);
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if(blackboard.current==IEnemyState.BeThorwn)
        {
            ScreenControl.Instance.ParticleRelease(blackboard.boom,transform.position,Vector3.zero);
            fsm.SwitchState(IEnemyState.Dead);
        }
    }
}