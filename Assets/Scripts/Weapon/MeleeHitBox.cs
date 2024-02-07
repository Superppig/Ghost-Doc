using System;
using UnityEngine;

public class MeleeHitBox: MonoBehaviour
{
    private Melee melee;

    private void Awake()
    {
        melee = transform.root.GetComponent<Melee>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            IEnemyBeHit enemy = other.GetComponent<IEnemyBeHit>();
            enemy.HitEnemy(melee.damage);
        }
    }
}
