using UnityEngine;
using Player_FSM;

public abstract class EnemyBaseState:IState
{
    protected Enemy enemy;
    
    
    public abstract void OnCheck();
    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void OnFixUpdate();
    public abstract void OnUpdate();
}