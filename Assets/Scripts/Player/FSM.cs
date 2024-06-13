using System.Collections.Generic;
using System;

public enum EStateType
{
    Walking,
    Jumping,
    Sprinting,
    Crouching,
    Sliding,
    Air,
    WallRunning
}

public interface IState
{
    /// <summary>
    /// 状态进入
    /// </summary>
    void OnEnter();

    /// <summary>
    /// 状态退出
    /// </summary>
    void OnExit();

    /// <summary>
    /// 状态进行
    /// </summary>
    void OnUpdate();

    void OnCheck();
    void OnFixUpdate();
}

public class FSM
{
    public EStateType current;
    public IState curState;
    public Dictionary<EStateType, IState> states;
    public Player player;

    public FSM(Player player)
    {
        states = new Dictionary<EStateType, IState>();
        this.player = player;
    }

    public void AddState(EStateType stateType, IState state)
    {
        if (states.ContainsKey(stateType))
        {
            return;
        }

        states.Add(stateType, state);
    }

    public void SwitchState(EStateType stateType)
    {
        if (!states.ContainsKey(stateType))
        {
            //Debug.Log("[SwitchState] >>>>>>>>>> not contain key:"+stateType);
            return;
        }

        if (curState != null)
        {
            curState.OnExit();
        }

        curState = states[stateType];
        curState.OnEnter();
        current = stateType;
    }

    public void OnUpdate()
    {
        curState.OnUpdate();
    }

    public void OnFixUpdate()
    {
        curState.OnFixUpdate();
    }

    public void OnCheck()
    {
        curState.OnCheck();
    }
}