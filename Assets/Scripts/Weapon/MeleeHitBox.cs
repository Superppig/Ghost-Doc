using System;
using System.Collections;
using System.Net.Http.Headers;
using UnityEngine;

public class MeleeHitBox : MonoBehaviour
{
    public CommonKnife melee;
    private ParticleSystem hitParticle;
    private Player player;

    private void Start()
    {
        hitParticle = melee.hitParticle;
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        
        Debug.Log(melee==null);
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
                    enemy.HitEnemy(new HitInfo(){rate = melee.damage});
                    Debug.Log("普通挥砍");

                    //粒子效果
                    HitPartical(other.ClosestPoint(transform.position), other.transform);
                    ScreenControl.Instance.FrameFrozen(melee.attackFrame, melee.attackStartTimeScale);
                    break;
                case Melee.AttackType.Ctrl:
                    //ctrl组合技
                    melee.ctrlHitInfo.dir = other.transform.position - player.transform.position;
                    enemy.HitEnemy(melee.ctrlHitInfo);
                    Debug.Log("砸地");
                    //粒子效果
                    break;
                case Melee.AttackType.Space:
                    melee.spaceHitInfo.dir = Vector3.up;
                    //升龙,给敌人一个力
                    enemy.HitEnemy(melee.spaceHitInfo);
                    Debug.Log("升龙");
                    break;
                case Melee.AttackType.Shift:
                    melee.shiftHitInfo.dir = other.transform.position - player.transform.position;
                    enemy.HitEnemy(melee.shiftHitInfo);
                    Debug.Log("突刺");
                    //粒子效果
                    HitPartical(other.ClosestPoint(transform.position), other.transform);
                    //较长顿帧
                    ScreenControl.Instance.FrameFrozen(melee.dashAttackFrame, melee.dashAttackStartTimeScale);
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
