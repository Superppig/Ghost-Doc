using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;


public class CommonKnife : Melee
{


    [Header("顿帧参数")] 
    [Header("攻击顿帧")] 
    public int attackFrame;
    public float attackStartTimeScale;
    
    [Header("格挡顿帧")]
    public int blockFrame;
    public float blockStartTimeScale;

    [Header("冲刺攻击顿帧")] 
    public int dashAttackFrame;
    public float dashAttackStartTimeScale;
    
    [Header("粒子特效")]
    public ParticleSystem dash;
    public ParticleSystem dashSlash;
    public ParticleSystem parryParticle;
    public Transform booster;
    
    
    
    protected override void Start()
    {
        AttackAnimTime=0.25f;
        dash.Stop();
        dashSlash.Stop();
        base.Start();
    }
    
    protected override void Update()
    {
        base.Update();
    }

    protected override void AnimCon()
    {
        switch (state)
        {
            case WeaponState.Idle:
                anim.SetBool("Blocking",false);
                anim.SetBool("Deffending",false);
                anim.SetBool("Slashing",false);
                break;
            case WeaponState.Blocking:
                anim.SetBool("Blocking",true);
                anim.SetBool("Deffending",false);
                anim.SetBool("Slashing",false);

                break;
            case WeaponState.Deffending:
                anim.SetBool("Blocking",false);
                anim.SetBool("Deffending",true);
                anim.SetBool("Slashing",false);
                break;
            case WeaponState.Attacking:
                anim.SetBool("Blocking",false);
                anim.SetBool("Deffending",false);
                anim.SetBool("Slashing",true);                
                break;
            case WeaponState.Comboing:
            default: break;
        }
    }

    protected override void Attack()
    {
        if (currentAttackType==AttackType.Common)
        {
            state=WeaponState.Attacking; 
            player.blackboard.isMeleeAttacking = true;
            AnimCon();
            StartCoroutine(StartAttack());
            //AttackPartical();
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
    
    //格挡(待用子物体)
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet")||other.CompareTag("EnemyAttack"))
        {
            Parry();
            other.GetComponent<IBlock>().BeBlocked();
        }
    }
    //格挡动画
    private void Parry()
    {
        state = WeaponState.Comboing;
        anim.SetBool("Parry",true);
        StartCoroutine(EndParry());
    }

    IEnumerator EndParry()
    {
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        hasAttack=true;
    }
    
    
    //动画事件
    public void BlockTimeForzen()
    {
        ScreenControl.Instance.FrameFrozen(blockFrame, blockStartTimeScale);
        ScreenControl.Instance.CamShake(0.1f, 10f);
    }

    public void DashParticle()
    {
        dash.Play();
    }
    public void DashSlashParticle()
    {
        dashSlash.Play();
    }
    public void ParryParticle()
    {
        ScreenControl.Instance.ParticleRelease(parryParticle,player.cameraTransform.position+player.cameraTransform.forward*0.5f,player.cameraTransform.forward,player.cameraTransform);
    }
}