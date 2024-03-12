using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;


public class CommonKnife : Melee
{
    
    
    
    protected override void Start()
    {
        AttackAnimTime=0.25f;
        base.Start();
    }
    
    protected override void Update()
    {
        base.Update();
    }

    protected override void Attack()
    {
        if (currentAttackType==AttackType.Common)
        {
            state=WeaponState.Attacking; 
            player.blackboard.isMeleeAttacking = true;
            AnimCon();
            StartCoroutine(StartAttack());
            AttackPartical();
        }
        else
        {
            state = WeaponState.Comboing;
        }
    }

    //Attack时的相机效果
    public void AttackCamChange()
    {
        camTrans.DOLocalRotate(new Vector3(30, 0, 0), 0.2f);
        StartCoroutine(FinishCamChange());
    }
    IEnumerator FinishCamChange()
    {
        yield return new WaitForSeconds(0.1f);
        camTrans.DOLocalRotate(Vector3.zero, 0.1f);
    }
    
    //攻击时的粒子效果
    private void AttackPartical()
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, player.cameraTransform.forward);
        ParticleSystem hit = Instantiate(attackParticle, transform.position, transform.rotation);
        hit.gameObject.transform.SetParent(transform);
        Destroy(hit.gameObject, hit.main.duration);
    }
}