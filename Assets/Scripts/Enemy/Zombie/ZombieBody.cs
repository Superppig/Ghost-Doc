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
            zombie.lastHitInfo = hitInfo;
        }
        Debug.Log("身体被击中");
    }
    
    
}
