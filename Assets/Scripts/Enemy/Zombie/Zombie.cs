using System.Collections;
using UnityEngine;
using Services;

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
    //攻击相关
    public HitInfo lastHitInfo;
    [Header("Debug")]
    public MeshRenderer meshRenderer;
    public Material Material0;
    public Material Material1;
    
    [Header("特效")]
    public ParticleSystem BombParticle;
    
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
        if(enemyState==EnemyBaseState.Move)
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

    public override void TakeDamage(float damage,bool isBomb = false)
    {
        base.TakeDamage(damage,isBomb);
        _state= ZombieState.Hit;
        //其他效果
        if (shield > 0)
        {
            
        }
    }

    protected int Pop(float minDistance)
    {
        //按照其他敌人与自己的距离进行排序
        enemyList.Sort((a, b) =>
        {
            if (Vector3.Distance(a.position, transform.position) < Vector3.Distance(b.position, transform.position))
            {
                return -1;
            }
            else
            {
                return 1;
            }
        });
        //找到范围之内的敌人并给一个力
        for (int i = 0; i < enemyList.Count; i++)
        {
            if (Vector3.Distance(transform.position, enemyList[i].position) > minDistance)
            {
                return i;
            }
        }

        return enemyList.Count;
    }
    
    //重写爆炸状态
    protected override void BombState(float time)
    {
        StartCoroutine(StartBomb(popWaitTime));
    }
    
    IEnumerator StartBomb(float time)
    {
        float timer = 0;
        float shineTime = 0.2f;
        bool isBomb = true;
        int cot = 0;

        while (timer < time||enemyState==EnemyBaseState.Dead)
        {
            timer += Time.deltaTime;
            if (timer > shineTime * cot)
            {
                isBomb = !isBomb;
                cot++;
            }
            if(isBomb)
                meshRenderer.material = Material1;
            else
                meshRenderer.material = Material0;
            yield return null;
        }
        
        meshRenderer.material = Material0;
        if(enemyState==EnemyBaseState.Bomb)
        {
            Debug.Log("产生爆炸");
            //爆炸效果
            int pop =  Pop(popMinDistance);
            for (int i = 1; i < pop; i++)
            {
                enemyList[i].GetComponent<Enemy>().TakeDamage(popDamage);
                enemyList[i].GetComponent<Rigidbody>().AddForce(((enemyList[i].position - transform.position)*20+30*Vector3.up).normalized * popForce, ForceMode.Impulse);
            }
            //屏幕震动
            ScreenControl.Instance.CamShake(0.2f,50f);//振幅待修改
            //特效
            ScreenControl.Instance.ParticleRelease(BombParticle,transform.position,Vector3.zero);
            
            isStrikToFly = true;
        }
        enemyState = EnemyBaseState.Dead;
        
        //爆炸直接结束
        Destroy(gameObject);
    }

    //碰撞和击飞
    void OnCollisionEnter(Collision other)
    {
        //速度大于一定值才连续碰撞和伤害
        if (other.relativeVelocity.magnitude > hitMinSpeed)
        {
            //撞到其他敌人则连续碰撞,自己停止击飞状态
            if (other.gameObject.CompareTag("Enemy"))
            {
                Debug.Log("撞到另一个敌人了");
                if (enemyState == EnemyBaseState.Bomb)
                {
                    Debug.Log("产生爆炸");
                    //爆炸效果
                    int pop =  Pop(popMinDistance);
                    for (int i = 1; i < pop; i++)
                    {
                        enemyList[i].GetComponent<Enemy>().TakeDamage(popDamage);
                        enemyList[i].GetComponent<Rigidbody>().AddForce(((enemyList[i].position - transform.position)*20+30*Vector3.up).normalized * popForce, ForceMode.Impulse);
                    }
                    //屏幕震动
                    ScreenControl.Instance.CamShake(0.2f,50f);//振幅待修改
                    //特效
                    ScreenControl.Instance.ParticleRelease(BombParticle,transform.position,Vector3.zero);
                    
                    isStrikToFly = true;
                    rb.velocity=Vector3.zero;
                    enemyState = EnemyBaseState.Dead;
                    //爆炸直接结束
                    Destroy(gameObject);
                }
                else if(enemyState == EnemyBaseState.Move||enemyState == EnemyBaseState.Unbalanced)
                {
                    IEnemyBeHit enemy = other.transform.Find("Body").GetComponent<IEnemyBeHit>();
                    if (enemy.CanBeHit() == false)
                    {
                        return;
                    }
                    //new一个hitinfo并衰减碰撞速度与伤害
                    HitInfo hitInfo = new HitInfo()
                    {
                        isHitFly = true,
                        dir = other.transform.position - transform.position,
                        speed = lastHitInfo.speed * collideDecayRate,
                        time = lastHitInfo.time * collideDecayRate,
                        rate = lastHitInfo.rate * collideDecayRate,
                        isBomb = false
                    };
                    if (FindNextEnemy(hitInfo.speed*hitInfo.time*10,findAngle,hitInfo.dir))
                    {
                        hitInfo.dir = nextEnemy.transform.position - transform.position;
                    }
                    enemy.HitEnemy(hitInfo);
                    isStrikToFly = true;
                    rb.velocity=Vector3.zero;
                }
            }
            //撞到墙
            if (other.gameObject.CompareTag("Wall"))
            {
                Debug.Log("撞到墙了");
                TakeDamage(wallDamage);
                isStrikToFly = true;
            }
        }
    }
}
