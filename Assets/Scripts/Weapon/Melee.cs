﻿using System;
using System.Collections;
using System.Timers;
using Player_FSM;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Melee : MonoBehaviour
{
    public bool hasAttack;
    [Header("基础属性")]
    public float damage;
    public float blockingtime;//格挡状态时间
    
    //目前为无限时间
    public float denfendtime;//架势状态时间


    [Header("动画控制")]
    public Animator anim;
    protected float AttackAnimTime;
    
    protected Collider blockArea;
    public Collider hitBox;
    
    protected Transform camTrans;
    protected Player player;
    
    [Header("粒子效果")]
    public ParticleSystem attackParticle;
    public ParticleSystem hitParticle;
    
    
    public WeaponState state=WeaponState.Idle;//武器状态
    public AttackType currentAttackType=AttackType.Common;//当前攻击类型
    
    protected EStateType playerState;//玩家状态
    
    
    public float timer;
    public enum WeaponState
    {
        Idle,
        Blocking,
        Deffending,
        Attacking,
        Comboing
    }
    
    public enum AttackType
    {
        Common,
        Shift,
        Ctrl,
        Space
    }


    [Header("Shift组合技")] 
    public float shiftDamage;
    public float shiftEnergyCost;//shift组合技能量消耗
    public float shiftSpeed;//shift组合技速度
    public float shiftTime;//shift组合技时间
    protected float shiftTimer;

    [Header("Ctrl组合技")]
    public float ctrlDamage;
    public float forceToEnemy;//ctrl组合技震飞力
    
    public float ctrlEnergyCost;//ctrl组合技能量消耗
    public float ctrlSpeed;//ctrl组合技速度
    public float ctrlStartHeight;//ctrl组合技高度
    public float ctrlEndHeight;//ctrl组合技结束高度
    
    [Header("Space组合技")]
    public float spaceDamage;
    public float spaceEnergyCost;//space组合技能量消耗
    public float spaceSpeed;//space组合技速度
    public float spaceTime;//space组合技时间
    protected float spaceTimer;

    
    
    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        blockArea = GetComponent<Collider>();
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        camTrans = player.cameraTransform;
        
        hasAttack = false;
        blockArea.enabled = false;
        timer = 0f;
    }
    
    protected virtual void Update()
    {
        Combo();
        StateChange();
        StateCon();
        Debug.Log(hitBox.enabled);
        
    }

    protected virtual void StateChange()
    {
        if (Input.GetMouseButton(1)&& state!=WeaponState.Attacking)
        {
            timer+=Time.deltaTime;
            if(timer<=blockingtime)
                state=WeaponState.Blocking;
            else
            {
                state=WeaponState.Deffending;
            }
        }
        else if (Input.GetMouseButtonUp(1) && state!=WeaponState.Attacking && state!=WeaponState.Comboing)
        {
            timer=0f;
            Attack();
        }
    
    }
    protected virtual void StateCon()
    {
        AnimCon();
        switch (state)
        {
            case WeaponState.Idle:
                blockArea.enabled = false;
                hitBox.enabled = false;
                player.blackboard.isBlocking = false;
                break;
            case WeaponState.Blocking:
                blockArea.enabled = true;
                hitBox.enabled = false;
                player.blackboard.isBlocking = true;
                break;
            case WeaponState.Deffending:
                blockArea.enabled = false;
                hitBox.enabled = false;
                player.blackboard.isBlocking = false;
                break;
            case WeaponState.Attacking:
                blockArea.enabled = true;
                hitBox.enabled = true;
                player.blackboard.isBlocking = false;
                break;
            case WeaponState.Comboing:
                blockArea.enabled = false;
                player.blackboard.isBlocking = false;
                break;
            default: break;
        }
    }
    
    //挥砍(普通攻击)
    protected virtual void Attack()
    {
        if (currentAttackType==AttackType.Common)
        {
            state=WeaponState.Attacking; 
            player.blackboard.isMeleeAttacking = true;
            AnimCon();
            StartCoroutine(StartAttack());
        }
        else
        {
            state = WeaponState.Comboing;
        }
        
    }

    protected virtual void AnimCon()
    {
        switch (state)
        {
            case WeaponState.Idle:
                anim.SetBool("Blocking",false);
                anim.SetBool("Deffending",false);
                anim.SetBool("Attacking",false);
                break;
            case WeaponState.Blocking:
                anim.SetBool("Blocking",true);
                anim.SetBool("Deffending",false);
                anim.SetBool("Attacking",false);

                break;
            case WeaponState.Deffending:
                anim.SetBool("Blocking",false);
                anim.SetBool("Deffending",true);
                anim.SetBool("Attacking",false);
                break;
            case WeaponState.Attacking:
                anim.SetBool("Blocking",false);
                anim.SetBool("Deffending",false);
                anim.SetBool("Attacking",true);                
                break;
            
            case WeaponState.Comboing:
            default: break;
        }
    }
    protected virtual IEnumerator StartAttack()
    {
        yield return new WaitForSeconds(AttackAnimTime);
        player.blackboard.isMeleeAttacking = false;
        state=WeaponState.Idle;
        hasAttack = true;
    }
    
    
    //格挡(待用子物体)
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet")||other.CompareTag("EnemyAttack"))
        {
            Attack();
            other.GetComponent<IBlock>().BeBlocked();
        }
    }
    
    
    //组合技
    protected virtual void Combo()
    {
        playerState = player.blackboard.currentState;//获取当前玩家状态
        if (state != WeaponState.Attacking)
        {
            //连招相关逻辑
            
            //左键
            if (Input.GetMouseButton(0) && currentAttackType==AttackType.Common)
            {
                
            }
            
            /* - shift
                - 功能：自由视角的长冲刺，方向为角色面朝方向，冲刺过程中能穿过敌人，造成伤害。相当于正常冲刺的加强版，距离和速度加快+穿透攻击敌人。
                - 后续改进想法：1.攻击到的敌人产生时停效果
                - 条件：需要消耗一格能量，不到一格无法释放，正常空闲状态下应该都可以释放 */
            if (Input.GetKeyDown(KeyCode.LeftShift) && currentAttackType==AttackType.Common)
            {
                ShiftCombo();
            }
            
            /*- ctrl
                - 功能：在空中加快落地速度,落地时对落点周围范围内敌人造成伤害判定，并把敌人震飞（圆形范围内，同一块地面上才有判定，垂直击飞+震退）
                - 后续改进想法：1.使用时将面前一定空间范围上的敌人由主角带着同步下落，类似把敌人劈砍下来的感觉 2.下落时的状态是否可以被冲刺上勾拳等取消？我想着应该是不能
                - 条件：要求垂直方向上的距离地面上有一定高度，略高于跳起高度，消耗一格能量
                - 参考：ultrakill里的下落砸地 视频待补充*/

            if (Input.GetKeyDown(KeyCode.LeftControl) && currentAttackType==AttackType.Common)
            {
                CtrlCombo();
            }
            
            /*- 空格
                - 功能：上劈，类似上勾拳的功能？同步垂直击飞前方空间的敌人，主角也同步跃起。
                - 后续改进想法：加入辅助近战吸附的功能
                - 条件：消耗一格能量，基本空闲状态下都可释放*/
            if (Input.GetKeyDown(KeyCode.Space) && currentAttackType==AttackType.Common)
            {
                SpaceCombo();
            }
        }
    }
    
    //组合技相关
    
    //shift组合技
    protected virtual void ShiftCombo()
    {
        player.blackboard.isCombo= true;
        if (player.GetEnerge() >= shiftEnergyCost && player.blackboard.currentState!=EStateType.Sprinting)
        {
            player.UseEnerge(shiftEnergyCost);
            StartCoroutine(StartShiftCombo());
        }
        else
        {
            player.blackboard.isCombo = false;
        }
    }
    protected virtual IEnumerator StartShiftCombo()
    {
        //记录方向
        Vector3 dir = camTrans.forward;
        player.rb.velocity=Vector3.zero;
        
        //关闭碰撞体
        //player.playerCollider.enabled = false;
        
        //攻击相关
        state = WeaponState.Comboing;
        currentAttackType = AttackType.Shift;
        anim.SetBool("Shift",true);
        hitBox.enabled=true;


        shiftTimer = 0f;
        

        //移动
        while (shiftTimer<=shiftTime)
        {
            shiftTimer += Time.deltaTime;
            player.rb.velocity = dir * shiftSpeed;
            yield return null;
        }
        
        player.blackboard.isCombo = false; 
        
        //攻击相关
        anim.SetBool("Shift",false);
        hitBox.enabled=false;

        //打开碰撞体
        player.playerCollider.enabled = true;
        hasAttack=true;
        
    }
    
    //ctrl组合技
    protected virtual void CtrlCombo()
    {
        player.blackboard.isCombo= true;
        if (player.GetEnerge()>=ctrlEnergyCost && player.blackboard.currentState==EStateType.Air && !player.IsGrounded(ctrlStartHeight))
        {
            player.UseEnerge(ctrlEnergyCost);
            

            StartCoroutine(StartCtrlCombo());
        }
        else
        {
            player.blackboard.isCombo = false;
        }
    }
    protected virtual IEnumerator StartCtrlCombo()
    {
        anim.SetBool("Ctrl",true);
        state = WeaponState.Comboing;
        currentAttackType = AttackType.Ctrl;
        hitBox.enabled=true;

        
        while (!player.IsGrounded(ctrlEndHeight))
        {

            player.rb.velocity = Vector3.down * ctrlSpeed;
            yield return null;
        }
        
        
        //屏幕晃动
        ScreenControl.Instance.CamShake(0.1f,6);

        Invoke("EndCrtlCombo",0.1f);
    }
    protected virtual void EndCrtlCombo()
    {
        anim.SetBool("Ctrl",false);
        
        hitBox.enabled = false;
        currentAttackType = AttackType.Common;
        
        player.blackboard.isCombo = false;
        hasAttack=true;
    }
    
    //跳跃组合技
    protected virtual void SpaceCombo()
    {
        player.blackboard.isCombo= true;
        if (player.GetEnerge()>=spaceEnergyCost&& player.IsGrounded(0.1f))
        {
            player.UseEnerge(spaceEnergyCost);
            StartCoroutine(StartSpaceCombo());
        }
        else
        {
            player.blackboard.isCombo = false;
        }
    }
    protected virtual IEnumerator StartSpaceCombo()
    {
        state = WeaponState.Comboing;
        currentAttackType = AttackType.Space;
        hitBox.enabled=true;

        spaceTimer = 0f;
        
        //获取速度
        player.rb.velocity = Vector3.up * spaceSpeed;

        anim.SetBool("Space",true);

        while (spaceTimer<=spaceTime)
        {
            spaceTimer += Time.deltaTime;
            Vector3 speed = new Vector3(player.rb.velocity.x,0,player.rb.velocity.z);
            player.rb.velocity = speed + Vector3.up * spaceSpeed;

            yield return null;
        }
        
        anim.SetBool("Space",false);
        
        currentAttackType = AttackType.Common;
        hitBox.enabled=false;

        player.blackboard.isCombo = false;
        hasAttack=true;
    }
}