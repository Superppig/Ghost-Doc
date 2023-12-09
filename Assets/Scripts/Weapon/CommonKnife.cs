using System;
using System.Collections;
using UnityEngine;

public class CommonKnife : Melee
{
    protected override void Start()
    {
        base.Start();
    }
    
    protected override void Update()
    {
        base.Update();
    }
    protected override void Attack()
    {
        base.Attack();
        StartCoroutine(StartAttack());
    }
    protected void FixedUpdate()
    {
        AttackDetect();
    }

    IEnumerator StartAttack()
    {
        anim.SetTrigger("fire");
        float time = anim.GetCurrentAnimatorStateInfo(0).length;
        duringAttack=true;
        yield return new WaitForSeconds(time);
        duringAttack=false;
    }
    void AttackDetect()
    {
        if(duringAttack&&!attacked)
        {
            RaycastHit hit;
            if (Physics.Raycast(WeaponPoint.position, WeaponPoint.forward, out hit, WeaponLength))
            {
                attacked = true;
                if (hit.collider.CompareTag("Enemy"))
                {
                    IEnemyBeHit enemyBeHit = hit.collider.GetComponent<IEnemyBeHit>();
                    enemyBeHit.HitEnemy(damageRate);                
                }
            }
            Debug.DrawLine(WeaponPoint.position, WeaponPoint.position+WeaponPoint.forward.normalized*WeaponLength, Color.red,1f);
        }
    }
}