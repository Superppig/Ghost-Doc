using UnityEngine;

public abstract class FsmState<T> where T : class
{

    public Fsm<T> CurrentFsm;
    public abstract void OnInit();
    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnFixedUpdate();
    public abstract void OnExit();
    public abstract void OnShutdown();

    public abstract void OnCheck();
}