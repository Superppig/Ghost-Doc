using System;
using Unity.VisualScripting;
using UnityEngine;

public class ZombieArm : MonoBehaviour,IBlock
{
    private Enemy enemy;
    private float damage;
    private void Awake()
    {
        enemy = transform.root.GetComponent<Enemy>();
        damage = enemy.blackboard.damage;
    }

    public void BeBlocked()
    {
        //zombie.beBlocked=true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.root.GetComponent<Player>().TakeDamage(damage);
        }
    }
}
