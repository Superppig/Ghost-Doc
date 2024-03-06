using System;
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
    protected Animator anim;
    protected float AttackAnimTime;
    
    protected Collider blockArea;
    public Collider hitBox;
    
    protected Transform camTrans;
    protected Player player;
    
    [Header("粒子效果")]
    public ParticleSystem attackParticle;
    public ParticleSystem hitParticle;
    
    
    public WeaponState state=WeaponState.Idle;//武器状态
    
    private EStateType playerState;//玩家状态
    
    
    public float timer;
    public enum WeaponState
    {
        Idle,
        Blocking,
        Deffending,
        Attacking
    }

    //组合技相关
    private bool isCombo;
    
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
        StateChange();
        StateCon();
        Combo();
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
        else if (Input.GetMouseButtonUp(1) && state!=WeaponState.Attacking)
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
        }
    }
    
    //挥砍(普通攻击)
    protected virtual void Attack()
    {
        state=WeaponState.Attacking; 
        player.blackboard.isMeleeAttacking = true;
        AnimCon();
        StartCoroutine(StartAttack());
    }

    void AnimCon()
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
        }
    }
    IEnumerator StartAttack()
    {
        yield return new WaitForSeconds(AttackAnimTime);
        player.blackboard.isMeleeAttacking = false;
        state=WeaponState.Idle;
        hasAttack = true;
    }
    
    
    //格挡(待用子物体)
    private void OnTriggerEnter(Collider other)
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
            if (Input.GetMouseButton(0))
            {
                
            }
            
            /* - shift
                - 功能：自由视角的长冲刺，方向为角色面朝方向，冲刺过程中能穿过敌人，路径上有攻击判定
                - 条件：需要消耗一格能量，不到一格无法释放 */
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                
            }
            
            /*- ctrl
                - 功能：在空中加快落地速度,落地时对*/

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                
            }
            
            /*- 空格
                - 上勾拳？同步垂直击飞敌人和自己向上升起*/
            if (Input.GetKeyDown(KeyCode.Space))
            {
                
            }
        }
        if (isCombo)
        {
            //停止3c相关
        }
    }
}