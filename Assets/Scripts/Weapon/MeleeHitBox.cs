using System;
using System.Collections;
using System.Net.Http.Headers;
using UnityEngine;

public class MeleeHitBox : MonoBehaviour
{
    private Melee melee;
    private ParticleSystem hitParticle;
    private Player player;

    private void Start()
    {
        melee = transform.parent.GetComponent<Melee>();
        hitParticle = melee.hitParticle;
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            IEnemyBeHit enemy = other.GetComponent<IEnemyBeHit>();
            //判断敌人是否会被击中
            if (enemy.CanBeHit() == false)
            {
                return;
            }
            switch (melee.currentAttackType)
            {
                case Melee.AttackType.Common:

                    //普通挥砍
                    enemy.HitEnemy(melee.damage);
                    Debug.Log("普通挥砍");

                    //粒子效果
                    HitPartical(other.ClosestPoint(transform.position), other.transform);
                    ScreenControl.Instance.FrameFrozen(5, 0.2f);
                    break;
                case Melee.AttackType.Ctrl:
                    //ctrl组合技
                    enemy.HitEnemy(melee.ctrlDamage);
                    Debug.Log("砸地");

                    //粒子效果
                    //震飞
                    Vector3 dir = other.transform.position - player.transform.position;
                    other.transform.root.GetComponent<Rigidbody>().AddForce(dir.normalized * melee.forceToEnemy, ForceMode.Impulse);
                    break;
                case Melee.AttackType.Space:
                    //升龙,给敌人一个力
                    enemy.HitEnemy(melee.spaceDamage);
                    other.transform.root.GetComponent<Rigidbody>().AddForce(player.rb.velocity.normalized * 5, ForceMode.Impulse);
                    Debug.Log("升龙");
                    break;
                case Melee.AttackType.Shift:
                    enemy.HitEnemy(melee.shiftDamage);
                    Debug.Log("突刺");
                    //粒子效果
                    HitPartical(other.ClosestPoint(transform.position), other.transform);
                    ScreenControl.Instance.FrameFrozen(5, 0.2f);
                    //较长顿帧
                    ScreenControl.Instance.FrameFrozen(10, 0.2f);
                    break;
            }
        }
    }
    private void HitPartical(Vector3 point, Transform enemtTrans)
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, transform.position - point);
        ParticleSystem hit = Instantiate(hitParticle, point, rotation);
        hit.gameObject.transform.SetParent(enemtTrans);
        Destroy(hit.gameObject, hit.main.duration);
    }
}
