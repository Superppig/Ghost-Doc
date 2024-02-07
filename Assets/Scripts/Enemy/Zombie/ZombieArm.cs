using System;
using Unity.VisualScripting;
using UnityEngine;

public class ZombieArm : MonoBehaviour,IBlock
{
    private Zombie zombie;
    private float damage;
    private void Awake()
    {
        zombie = transform.root.GetComponent<Zombie>();
        damage = zombie.bodyDamage;
    }

    public void BeBlocked()
    {
        zombie.beBlocked=true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.root.GetComponent<Player>().TakeDamage(damage);
        }
    }
}
