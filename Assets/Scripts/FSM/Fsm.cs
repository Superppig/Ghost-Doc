using System;
using System.Collections.Generic;

public class Fsm<T> : FsmBase where T : class
{
    public T Owner { get; private set; }
    public FsmState<T> m_CurrFsmState;
    public Dictionary<Type, FsmState<T>> m_FsmStateDic;
    public Fsm(int fsmID, T owner, FsmState<T>[] states) : base(fsmID)
    {
        Owner = owner;
        m_FsmStateDic = new Dictionary<Type, FsmState<T>>();

        for (int i = 0; i < states.Length; i++)
        {
            var type = states[i].GetType();
            m_FsmStateDic[type] = states[i];
            states[i].CurrentFsm = this;
            states[i].OnInit();
        }

        currentState = null;
    }


    /// <summary>
    /// 切换状态
    /// </summary>
    /// <returns></returns>
    /// <returns></returns>
    public void ChangeState(Type type)
    {
        if (type == currentState) return;
        if (m_CurrFsmState != null)
        {
            m_CurrFsmState.OnExit();
        }

        if (m_FsmStateDic.TryGetValue(type, out FsmState<T> state))
        {
            m_CurrFsmState = state;
            m_CurrFsmState.OnEnter();
            currentState = type;
        }
    }
    public void ChangeState<TState>() where TState : FsmState<T>
    {
        ChangeState(typeof(TState));
    }

    public void OnCheck()
    {
        if (m_CurrFsmState != null)
        {
            m_CurrFsmState.OnCheck();
        }
    }
    
    public void OnUpdate()
    {
        if (m_CurrFsmState != null)
        {
            m_CurrFsmState.OnUpdate();
        }
    }

    public void OnFixedUpdate()
    {
        if (m_CurrFsmState != null)
        {
            m_CurrFsmState.OnFixedUpdate();
        }
    }

    public override void Start(Type stateType)
    {
        if (m_FsmStateDic.TryGetValue(stateType, out FsmState<T> state))
        {
            m_CurrFsmState = state;
            m_CurrFsmState.OnEnter();
            currentState = stateType;
        }
    }
    
    public void Start<TState>() where TState : FsmState<T>
    {
        Start(typeof(TState));
    }

    public override void Shutdown()
    {
        if (m_CurrFsmState != null)
        {
            m_CurrFsmState.OnExit();
        }

        var enumerator = m_FsmStateDic.GetEnumerator();
        while (enumerator.MoveNext())
        {
            enumerator.Current.Value.OnShutdown();
        }

        m_FsmStateDic.Clear();
    }

    public Type GetCurrentState()
    {
        return currentState;
    }
}