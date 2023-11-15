using System.Collections.Generic;
using System;

namespace Player_FSM
{
    public enum StateType
    {
        walking,
        jumping,
        sprinting,
        crouching,
        sliding,
        air,
        wallRunning
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
    [Serializable]
    public class Blackboard
    {
        //储存共享的数据,或者向外展示的数据,可配置的数据
        
    }

    public class FSM
    {
        public StateType current;
        public IState curState;
        public Dictionary<StateType, IState> states;
        public Blackboard blackboard;

        public FSM(Blackboard blackboard)
        {
            this.states = new Dictionary<StateType, IState>();
            this.blackboard = blackboard;
        }
        
        public void AddState(StateType stateType, IState state)
        {
            if (states.ContainsKey(stateType))
            {
                return;
            }
            states.Add(stateType,state);
        }

        public void SwitchState(StateType stateType)
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
    

    
}