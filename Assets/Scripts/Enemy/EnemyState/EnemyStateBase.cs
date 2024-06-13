using System.Reflection;
using UnityEngine;
    
public abstract class EnemyStateBase : IState
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
    
    
    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void OnUpdate();
    public abstract void OnCheck();
    public abstract void OnFixUpdate();
    
}