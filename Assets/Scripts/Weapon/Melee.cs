using System;
using System.Collections;
using System.Timers;
using UnityEngine;

public class Melee : MonoBehaviour
{
    public bool hasAttack;
    [Header("基础属性")]
    public float damage;
    public float blockingtime;//格挡状态时间



    private Animator anim;
    private Collider blockArea;
    public WeaponState state=WeaponState.Idle;
    public float timer;
    public enum WeaponState
    {
        Idle,
        Blocking,
        Deffending,
        Attacking
    }
    
    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        blockArea = GetComponent<Collider>();
        
        hasAttack = false;
        blockArea.enabled = false;
        timer = 0f;
    }
    
    protected virtual void Update()
    {
        StateChange();
        StateCon();
    }

    protected virtual void StateChange()
    {
        if (Input.GetMouseButton(1)&& state!=WeaponState.Attacking)
        {
            timer+=Time.deltaTime;
            if(timer<=blockingtime)
                state=WeaponState.Blocking;
            else
                state=WeaponState.Deffending;
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
                break;
            case WeaponState.Blocking:
                blockArea.enabled = true;
                break;
            case WeaponState.Deffending:
                blockArea.enabled = false;
                break;
            case WeaponState.Attacking:
                blockArea.enabled = true;
                break;
        }
    }
    
    //挥砍
    protected virtual void Attack()
    {
        state=WeaponState.Attacking; 
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
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        state=WeaponState.Idle;
        hasAttack = true;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            Attack();
            other.GetComponent<EnemyBullet>().BeBlocked();
        }

    }
}