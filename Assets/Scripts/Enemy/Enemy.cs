using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health;
    public float damage;

    public virtual void TakeDamage(float damage)
    {
        health -= damage;
    }

    public virtual void Dead()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
