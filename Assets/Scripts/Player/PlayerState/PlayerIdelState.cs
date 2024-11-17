using UnityEngine;

public class PlayerIdelState : PlayerStateBase
{
    public PlayerIdelState(Player player) : base(player)
    {
    }

    public override void OnInit()
    {
    }

    public override void OnEnter()
    {
    }

    public override void OnUpdate()
    {
        player.rb.velocity = Vector3.zero;
        if (!blackboard.grounded)
        {
            CurrentFsm.ChangeState<PlayerAirState>();
        }
        else if(Input.GetKeyDown(settings.keySettings.jumpkey))
        {
            CurrentFsm.ChangeState<PlayerJumpState>();
        }
        else if(Input.GetKeyDown(settings.keySettings.sprintKey))
        {
            if (player.UseEnerge(100))
            {
                CurrentFsm.ChangeState<PlayerSprintingState>();
            }
        }
        else if(blackboard.dirInput.magnitude> 0f)
        {
            CurrentFsm.ChangeState<PlayerWalkingState>();
        }
    }

    public override void OnFixedUpdate()
    {
    }

    public override void OnExit()
    {
    }

    public override void OnShutdown()
    {
    }

    public override void OnCheck()
    {
    }
}