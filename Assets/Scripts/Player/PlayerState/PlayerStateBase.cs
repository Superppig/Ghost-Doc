using UnityEngine;

public abstract class PlayerStateBase : FsmState<Player>
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
}
