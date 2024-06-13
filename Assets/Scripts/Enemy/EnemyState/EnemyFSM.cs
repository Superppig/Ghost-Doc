using System.Collections.Generic;

public enum IEnemyState
{
    Idle,
    Chase,
    Attack,
    Stagger,
    BeThorwn,
    Hit,
    Dead
}

public class EnemyFSM
{
    public IEnemyState current;
    public IState curState;
    public Dictionary<IEnemyState, IState> states;
    public Enemy enemy;
    
    public EnemyFSM(Enemy enemy)
    {
        states = new Dictionary<IEnemyState, IState>();
        this.enemy = enemy;
    }
    
    
    //添加状态
    public void AddState(IEnemyState stateType, IState state)
    {
        if (states.ContainsKey(stateType))
        {
            return;
        }

        states.Add(stateType, state);
    }
    
    //切换状态
    public void SwitchState(IEnemyState stateType)
    {

        if (!states.ContainsKey(stateType))
        {
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
