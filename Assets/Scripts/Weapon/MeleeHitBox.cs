using System;
using UnityEngine;

public class MeleeHitBox: MonoBehaviour
{
    private Melee melee;
    private ParticleSystem hitParticle;

    private void Start()
    {
        melee = transform.parent.GetComponent<Melee>();
        hitParticle = melee.hitParticle;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            IEnemyBeHit enemy = other.GetComponent<IEnemyBeHit>();
            enemy.HitEnemy(melee.damage);
            //粒子效果
            HitPartical(other.ClosestPoint(transform.position));
        }
    }
    
    private void HitPartical(Vector3 point)
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, transform.position-point);
        ParticleSystem hit = Instantiate(hitParticle, transform.position, rotation);
        hit.gameObject.transform.SetParent(transform);
        Destroy(hit.gameObject, hit.main.duration);
    }
}
