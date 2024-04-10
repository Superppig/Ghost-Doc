using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieBody : MonoBehaviour,IEnemyBeHit
{
    private Zombie zombie;
    private float damage;
    private HitInfo lastHitInfo;
    private void Awake()
    {
        zombie = transform.root.GetComponent<Zombie>();
        damage = zombie.bodyDamage;
    }
    public bool CanBeHit()
    {
        return !zombie.hasHit;
    }

    public void HitEnemy(HitInfo hitInfo)
    {
        lastHitInfo=hitInfo;
        zombie.TakeDamage(damage*hitInfo.rate);
        if (hitInfo.isHitFly)
        {
            zombie.BeStrickToFly(hitInfo.dir,hitInfo.speed,hitInfo.time);
        }
        Debug.Log("身体被击中");
    }
    
    //碰撞和击飞
    void OnCollisionEnter(Collision other)
    {
        Debug.Log("碰撞到了"+other.gameObject.name);
        //速度大于一定值才连续碰撞和伤害
        if (zombie.rb.velocity.magnitude > zombie.hitMinSpeed)
        {
            //撞到其他敌人则连续碰撞,自己停止击飞状态
            if (other.gameObject.CompareTag("Enemy"))
            {
                Debug.Log("撞到另一个敌人了");
                IEnemyBeHit enemy = other.gameObject.GetComponent<IEnemyBeHit>();
                if (enemy.CanBeHit() == false)
                {
                    return;
                }
                //new一个hitinfo并衰减碰撞速度与伤害
                HitInfo hitInfo = new HitInfo()
                {
                    isHitFly = true,
                    dir = other.transform.position - transform.position,
                    speed = lastHitInfo.speed * zombie.collideDecayRate,
                    time = lastHitInfo.time * zombie.collideDecayRate,
                    rate = lastHitInfo.rate * zombie.collideDecayRate
                };
                enemy.HitEnemy(hitInfo);
                zombie.isStrikToFly = false;
            }
            
            //撞到墙
            if (other.gameObject.CompareTag("Wall"))
            {
                Debug.Log("撞到墙了");
                IEnemyBeHit enemy = other.gameObject.GetComponent<IEnemyBeHit>();
                if (enemy.CanBeHit() == false)
                    enemy.HitEnemy(new HitInfo(){rate = zombie.wallDamage});
                zombie.isStrikToFly = false;
            }
        }
    }
}
