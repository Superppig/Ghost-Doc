using System;
using Services;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyCore : MonoBehaviour, IGrabObject
{
    public EnemyType enemyType;
    public Collider col;
    public Rigidbody rb;
    private BuffSystem m_BuffSystem;

    public ParticleSystem boomParticle;
    public float boomRange;
    public float boomForce;
    public float boomDamage;

    private void Awake()
    {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        m_BuffSystem = ServiceLocator.Get<BuffSystem>();
        rb.useGravity = false;
        col.isTrigger = true;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Grabbed()
    {
        col.isTrigger = true;
    }

    public void Released()
    {
        col.isTrigger = false;
    }

    public void Fly(Vector3 dir, float force)
    {
        rb.useGravity = true;
        col.isTrigger = false;
        rb.AddForce(dir * force, ForceMode.Impulse);
    }

    public bool CanGrab()
    {
        return false;
    }

    public bool CanUse()
    {
        return false;
    }

    public void Use()
    {
        m_BuffSystem.ActivateBuff(GetBuffType());
        Destroy(gameObject);
    }

    private BuffType GetBuffType()
    {
        switch (enemyType)
        {
            case EnemyType.RemoteEnemy:
                return BuffType.Remote;
            case EnemyType.Zombie:
                return BuffType.Zombie;
            case EnemyType.CrazyBiteEnemy:
                return BuffType.KenKen;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public IGrabObject GetGrabObject()
    {
        return null;
    }

    private void OnCollisionEnter(Collision other)
    {
        ServiceLocator.Get<ScreenControl>().ParticleRelease(boomParticle, transform.position, Vector3.zero);
        Boom();
        Destroy(gameObject);
    }

    private void Boom()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, boomRange);
        if (colliders.Length == 0)
            return;
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                collider.transform.root.GetComponent<Player>().rb
                    .AddForce((collider.transform.position - transform.position).normalized*boomForce, ForceMode.Impulse);
            }
            else if (collider.CompareTag("Enemy"))
            {
                Enemy enemy = collider.transform.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(boomDamage);
                    enemy.rb.AddForce((collider.transform.position - transform.position).normalized * boomForce,
                        ForceMode.Impulse);
                }
            }
        }
    }
}