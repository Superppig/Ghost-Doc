using System;
using UnityEngine;

public class Melee : MonoBehaviour
{
    public float WeaponLength;
    public Transform WeaponPoint;
    public float waitTime;
    public float damageRate;
    protected Animator anim;

    protected bool isAttacking;
    protected float timer;
    
    protected bool duringAttack;//是否处于攻击状态
    protected bool attacked;//是否攻击到了
    
    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
    }
    
    protected virtual void Update()
    {
        AttackController();
    }
    
    protected virtual void AttackController()
    {
        //射击
        if (Input.GetMouseButtonDown(0))
        {
            isAttacking = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isAttacking = false;
        }

        timer += Time.deltaTime;
        timer = timer > 10f ? 10f : timer;
        if(timer>=waitTime&&isAttacking)
        {
            Attack();
            attacked = false;
            
            timer = 0;
        }
    }

    protected virtual void Attack()
    {
        
    }
}