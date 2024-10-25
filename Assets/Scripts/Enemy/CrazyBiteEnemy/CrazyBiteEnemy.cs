using UnityEngine;

public class CrazyBiteEnemy : Enemy,IGrabObject
{
    public EnemyType enemyType = EnemyType.CrazyBiteEnemy;
    private float attackTimer;
    
    
    protected override void Awake()
    {
        base.Awake();
        
        fsm.AddState(IEnemyState.Idle, new CrazyBiteEnemyIdelState(this));
        fsm.AddState(IEnemyState.Attack, new CrazyBiteEnemyAttackState(this));
        fsm.AddState(IEnemyState.BeThorwn, new CrazyBiteEnemyBeTrownState(this));
        fsm.AddState(IEnemyState.Chase, new CrazyBiteEnemyChaseState(this));
        fsm.AddState(IEnemyState.Dead, new CrazyBiteEnemyDeadState(this));
        fsm.AddState(IEnemyState.Stagger, new CrazyBiteEnemyStaggerState(this));
        fsm.AddState(IEnemyState.Hit, new CrazyBiteEnemyHitState(this));
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
            if (blackboard.currentHealth<= blackboard.weakHealth&&blackboard.currentHealth>0 && blackboard.current != IEnemyState.BeThorwn)
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
                if(blackboard.distanceToPlayer<blackboard.biteRange)
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
        BuffSystem.Instance.ActivateBuff(BuffType.Zombie);
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