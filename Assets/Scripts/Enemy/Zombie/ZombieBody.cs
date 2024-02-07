using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieBody : MonoBehaviour,IEnemyBeHit
{
    private Zombie zombie;
    private float damage;
    private void Awake()
    {
        zombie = transform.root.GetComponent<Zombie>();
        damage = zombie.bodyDamage;
    }

    public void HitEnemy(float rate)
    {
        zombie.TakeDamage(damage*rate);
    }
}
