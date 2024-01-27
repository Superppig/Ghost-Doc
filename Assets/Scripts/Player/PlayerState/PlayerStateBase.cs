using Player_FSM;
using UnityEngine;

public abstract class PlayerStateBase : IState
{
    protected Player player;
    protected PlayerBlackboard blackboard;
    protected PlayerSettings settings;
    protected Rigidbody rb;
    protected Transform rbTransform;

    public PlayerStateBase(Player player)
    {
        this.player = player;
        blackboard = player.blackboard;
        settings = player.settings;
        rb = player.rb;
        rbTransform = rb.transform;
    }

    public abstract void OnCheck();
    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void OnFixUpdate();
    public abstract void OnUpdate();
}
