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
    
    protected float fireCd;//射击冷却时间


    protected bool hasAttack;
    public bool hasHit;
    protected bool hasDead;
    
    public ZombieState _state=ZombieState.Idle;
    public enum ZombieState
    {
        Idle,
        Walk,
        Hit,
        Fire,
        Attack
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

    protected override void StateControl()
    {
        base.StateControl();
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
            default:
                break;
        }
    }

    protected override void StateChange()
    {
        base.StateChange();
        if(enemyState==EnemyBaseState.Common||enemyState==EnemyBaseState.Move)
        {
            if (_state != ZombieState.Attack && _state != ZombieState.Fire &&
                _state != ZombieState.Hit)
            {
                if (DistanceToPlayer() <= findRange)
                    _state = ZombieState.Walk;
                else
                    _state = ZombieState.Idle;
            }

            if (DistanceToPlayer() <= attackRange && _state == ZombieState.Walk)
            {
                _state = ZombieState.Attack;
            }
            else if (_state == ZombieState.Walk)
            {
                timer += Time.deltaTime;
                if (timer > fireCd)
                {
                    timer = 0f;
                    _state = ZombieState.Fire;
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
            anim.SetBool("Hit",true);
            StartCoroutine(StartHit());
        }
    }
    IEnumerator StartHit()
    {
        yield return new WaitForSeconds(hitTime);
        anim.SetBool("Hit",false);
        hasHit = false;
        _state = ZombieState.Idle;
    }

    public override void BeStrickToFly(Vector3 dir,float speed,float time )
    {
        StartCoroutine(StartStrickToFly(dir,speed,time));
    }
    IEnumerator StartStrickToFly(Vector3 dir,float speed,float time)
    {
        float timer = 0;
        while (timer < time&&!isStrikToFly)
        {
            timer += Time.deltaTime;
            rb.velocity = dir.normalized * speed;
            yield return null;
        }
        isStrikToFly = false;
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
