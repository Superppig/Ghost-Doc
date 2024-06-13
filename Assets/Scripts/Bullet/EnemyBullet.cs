using System;
using UnityEngine;

public class EnemyBullet : Bullet ,IBlock
{
    private bool beBlocked = false;

    //Debug
    public Color enemyColor = Color.red;
    public Color blockedColor = Color.blue;
    protected override void Start()
    {
        base.Start();
        GetComponent<Renderer>().material.color = enemyColor;
    }

    protected override void Move()
    {
        if (beBlocked)
        {
            rb.velocity = dir * (-1 * speed);
            timer += Time.deltaTime;
            if (timer > lifeTime)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            rb.velocity = dir * speed;
            timer += Time.deltaTime;
            if (timer>lifeTime)
            {
                Destroy(gameObject);
            }
        }
    }

    public void BeBlocked()
    {
        timer = 0f;
        beBlocked = true;
        
        //Debug
        GetComponent<Renderer>().material.color = blockedColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (beBlocked)
        {
            if (other.CompareTag("Enemy"))
            {
                if(other.GetComponent<IEnemyBeHit>() == null) return;
                other.GetComponent<IEnemyBeHit>().HitEnemy(new HitInfo(){damage = damage});
                Destroy(gameObject);
            }
        }
        else
        {
            if (other.CompareTag("Player"))
            {
                other.transform.root.GetComponent<Player>().TakeDamage(damage);
                Destroy(gameObject);
            }
        }

        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}