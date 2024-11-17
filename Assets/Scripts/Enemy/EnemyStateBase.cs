using System.Reflection;
using UnityEngine;
    
public abstract class EnemyStateBase<T> : FsmState<T> where T : Enemy
{
    
    protected Enemy enemy;
    protected EnemyBlackboard blackboard;
    protected Rigidbody rb;
    protected Transform rbTransform;
    
    public EnemyStateBase(Enemy enemy)
    {
        this.enemy = enemy;
        blackboard = enemy.blackboard;
        rb = enemy.rb;
        rbTransform = rb.transform;
    }
}