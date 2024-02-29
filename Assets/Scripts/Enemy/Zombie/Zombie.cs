using System.Collections;
using DG.Tweening;
using UnityEngine;
using Pathfinding;
using UnityEngine.UI;

public class Zombie : Enemy
{
    [Header("Zombie属性")]
    public float attackRange;//攻击范围
    public float attackWaitTime;//攻击等待时间

    public float findRange;//寻路范围

    public float hitTime;//受击时间
    
    public bool beBlocked;//是否被格挡
    
    private Animator anim;
    protected float fireCd;//射击冷却时间


    private bool hasAttack;
    private bool hasHit;
    private bool hasDead;
    
    public ZombieState _state=ZombieState.Idle;
    public enum ZombieState
    {
        Idle,
        Walk,
        Hit,
        Fire,
        Attack,
        Dead
    }

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        fireCd = 1 / attackRate;
    }
    
    protected override void Update()
    {
        StateChange();
        BlockJudge();

        StateControl();
    }
    protected override void FixedUpdate()
    {

    }

    protected void StateControl()
    {
        switch (_state)
        {
            case ZombieState.Idle:
                break;
            case ZombieState.Walk:
                Find();
                break;
            case ZombieState.Hit:
                Hit();
                break;
            case ZombieState.Fire:
                FireBullet();
                break;
            case ZombieState.Attack:
                Attack();
                break;
            case ZombieState.Dead:
                Dead();
                break;
            default:
                break;
        }
    }

    protected void StateChange()
    {
        if (health<=0 && _state!=ZombieState.Dead)
        {
            _state=ZombieState.Dead;
        }
        else if(_state!=ZombieState.Dead)
        {
            if (_state != ZombieState.Attack && _state != ZombieState.Fire && _state != ZombieState.Dead && _state != ZombieState.Hit)
            {
                if(DistanceToPlayer()<=findRange)
                    _state= ZombieState.Walk;
                else
                    _state= ZombieState.Idle;
            }
        
            if (DistanceToPlayer()<=attackRange && _state==ZombieState.Walk)
            {
                _state=ZombieState.Attack;
            }
            else if (_state == ZombieState.Walk)
            {
                timer+=Time.deltaTime;
                if (timer>fireCd)
                {
                    timer = 0f;
                    _state=ZombieState.Fire;
                }
            }
        }
    }

    //远程攻击
    void FireBullet()
    {
        EnemyBullet bullet1 = Instantiate(bullet, transform.position, Quaternion.identity);
        bullet1.dir = (player.transform.position - transform.position).normalized;
        bullet1.damage = damage;
        _state = ZombieState.Idle;
    }

    void Attack()
    {
        if (!hasAttack)
        {
            hasAttack = true;
            StartCoroutine(StartAttack());
        }
    }
    IEnumerator StartAttack()
    {
        yield return new WaitForSeconds(attackWaitTime);
        StartCoroutine(EndAttack());
        anim.SetBool("Attack",true);
    }
    IEnumerator EndAttack()
    {
        yield return new WaitForSeconds(0.5f);
        anim.SetBool("Attack",false);
        hasAttack= false;
        _state = ZombieState.Idle;
    }
    
    protected override void Dead()
    {
        if (!hasDead)
        {
            hasDead = true;
            anim.SetTrigger("Die");
            StartCoroutine(StartDie());
        }
    }
    IEnumerator StartDie()
    {
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }

    protected override void Hit()
    {
        if (!hasHit)
        {
            hasHit = true;
            anim.SetTrigger("Hit");
            StartCoroutine(StartHit());
        }
    }
    IEnumerator StartHit()
    {
        yield return new WaitForSeconds(hitTime);
        hasHit = false;
        _state = ZombieState.Idle;
    }
    
    //计算与玩家的举例
    public float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, player.transform.position);
    }
    
    //格挡判定
    public void BlockJudge()
    {
        if (beBlocked)
        {
            TakeDamage(damage);
            anim.SetBool("Attack",false);
            beBlocked = false;//重置格挡
        }
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        _state= ZombieState.Hit;
        //其他效果
    }
}
