using System.Collections;
using DG.Tweening;
using UnityEngine;
using Pathfinding;

public class Zombie : Enemy
{
    [Header("Zombie属性")]
    public float attackRange;
    public float attackWaitTime;

    protected bool isAttacking;//是否在近战攻击
    public bool beBlocked;//是否被格挡
    
    private Animator anim;
    
    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
    }
    
    protected override void Update()
    {
        if(DistanceToPlayer()<attackRange &&! isAttacking)
        {
            Attack();
        }
        else if(!isAttacking)
        {
            Find();
            FireBullet();
        }
        BlockJudge();
        Dead();
    }
    protected override void FixedUpdate()
    {

    }
    
    //远程攻击
    void FireBullet()
    {
        float time = 1 / attackRate;
        timer+=Time.deltaTime;
        if (timer>time)
        {
            timer = 0f;
            EnemyBullet bullet1 = Instantiate(bullet, transform.position, Quaternion.identity);
            bullet1.dir = (player.transform.position - transform.position).normalized;
            bullet1.damage = damage;
        }
    }

    void Attack()
    {
        isAttacking = true;
        StartCoroutine(StartAttack());
    }
    IEnumerator StartAttack()
    {
        yield return new WaitForSeconds(attackWaitTime);
        StartCoroutine(EndAttack());
        anim.SetBool("Attack",true);
    }
    IEnumerator EndAttack()
    {
        yield return new WaitForSeconds(0.333f);
        anim.SetBool("Attack",false);
        isAttacking = false;
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
        //其他效果
    }
}
