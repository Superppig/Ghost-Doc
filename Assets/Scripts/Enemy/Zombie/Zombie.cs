using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Enemy
{
    public float headDamage;
    public float bodyDamage;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Dead();
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        //其他效果
    }
}
