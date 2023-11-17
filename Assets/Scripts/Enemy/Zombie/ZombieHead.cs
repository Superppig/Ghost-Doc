using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieHead : MonoBehaviour,IEnemyBeHit
{
    private Zombie zombie;
    private float damage;
    private void Awake()
    {
        zombie = transform.parent.GetComponent<Zombie>();
        damage = zombie.headDamage;
    }

    public void HitEnemy(float rate)
    {
        zombie.TakeDamage(damage*rate);
    }
}
