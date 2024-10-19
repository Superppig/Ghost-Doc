using UnityEngine;

public interface FsmState<T> where T : class
{
    void OnInit(IFsm<T> fsm);
    void OnEnter(IFsm<T> fsm);
    void OnUpdate(IFsm<T> fsm);
    void OnFixedUpdate(IFsm<T> fsm);
    void OnExit(IFsm<T> fsm);
}