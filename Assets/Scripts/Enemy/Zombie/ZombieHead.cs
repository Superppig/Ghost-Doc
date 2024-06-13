using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieHead : MonoBehaviour,IEnemyBeHit
{
    private Enemy enemy;
    private float rate;
    private void Awake()
    {
        enemy = transform.root.GetComponent<Enemy>();
        rate = enemy.blackboard.criticalStrikeRate;
    }

    public bool CanBeHit()
    {
        return false; //!zombie.hasHit;
    }

    public void HitEnemy(HitInfo hitInfo)
    {
        enemy.TakeDamage(rate*hitInfo.damage);
        Debug.Log("头部被击中");
    }
}
