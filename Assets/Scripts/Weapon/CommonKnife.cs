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
    
    //攻击时的粒子效果(暂时弃用)
    private void AttackPartical()
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, player.cameraTransform.forward);
        ParticleSystem hit = Instantiate(attackParticle, transform.position, transform.rotation);
        hit.gameObject.transform.SetParent(transform);
        Destroy(hit.gameObject, hit.main.duration);
    }
    
    //重写dash
    protected override IEnumerator StartShiftCombo()
    {
        //记录方向
        Vector3 dir = camTrans.forward;
        player.rb.velocity=Vector3.zero;
        
        //关闭碰撞体
        //player.playerCollider.enabled = false;
        
        //攻击相关
        state = WeaponState.Comboing;
        currentAttackType = AttackType.Shift;


        shiftTimer = 0f;
        

        //移动
        while (shiftTimer<=shiftTime)
        {
            shiftTimer += Time.deltaTime;
            player.rb.velocity = dir * shiftSpeed;
            //添加速度线
            int i = 1;
            if (shiftTimer > i * 0.2f)
            {
                player.vineLine.Summon(player.transform.position,new Vector3(player.rb.velocity.x,0,player.rb.velocity.z));
                i++;
            }
            yield return null;
        }
        
        player.blackboard.isCombo = false; 
        
        currentAttackType = AttackType.Common;
        state= WeaponState.Idle;

        //打开碰撞体
        player.playerCollider.enabled = true;
        RetrackMelee();
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
        if (!hasBlockedAmin)
        {
            state = WeaponState.Comboing;
            anim.SetBool("Parry",true);
            hasBlockedAmin=true;
            StartCoroutine(EndParry());
            
        }
        
    }

    IEnumerator EndParry()
    {
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        hasBlockedAmin = false;
        EndAttackAnim();
        RetrackMelee();
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