using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieHead : MonoBehaviour,IEnemyBeHit
{
    private Zombie zombie;
    private float damage;
    private void Awake()
    {
        zombie = transform.root.GetComponent<Zombie>();
        damage = zombie.headDamage;
    }

    public bool CanBeHit()
    {
        return !zombie.hasHit;
    }

    public void HitEnemy(HitInfo hitInfo)
    {
        zombie.TakeDamage(damage * hitInfo.rate, hitInfo.isBomb);
        Debug.Log("头部被击中");
    }
}
