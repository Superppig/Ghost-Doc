using System;
using Unity.VisualScripting;
using UnityEngine;

public class ZombieArm : MonoBehaviour,IBlock
{
    private Zombie zombie;
    private float damage;
    private void Awake()
    {
        zombie = transform.parent.GetComponent<Zombie>();
        damage = zombie.bodyDamage;
    }

    public void BeBlocked()
    {
        zombie.beBlocked=true;
    }
}
