using UnityEngine;

public interface IFsm<T> where T : class
{
    T Owner
    {
        get;
    }

    void ChangeState<TState>(T owner) where TState : FsmState<T>;
}