using Player_FSM;
using UnityEngine;

public class PlayerJumpState : PlayerStateBase
{
    private float jumpSpeed;

    private float WallJumpSpeed => settings.jumpSettings.wallJumpSpeed;
    private float WalkSpeed => settings.walkSettings.walkSpeed;
    private RaycastHit wall;

    public PlayerJumpState(Player player) : base(player)
    {
    }

    private bool IsWallJump => blackboard.isWallJump;

    public override void OnEnter()
    {
        float height = settings.jumpSettings.height;

        //获取速度
        rb.velocity = blackboard.velocity;
        
        if (blackboard.lastState == EStateType.Sliding)
            height *= settings.otherSettings.slideToJumpHeightRate;
        else if(blackboard.lastState != EStateType.Sprinting && blackboard.lastState != EStateType.WallRunning)
        {
            //修正速度
            Vector3 XZSpeed = new Vector3(rb.velocity.x,0,rb.velocity.z);
            rb.velocity = XZSpeed.magnitude>WalkSpeed?XZSpeed.normalized*WalkSpeed:XZSpeed;
        }
        
        jumpSpeed = Speed(height);
        if (IsWallJump)
        {
            wall = blackboard.currentWall;
            rb.velocity += (wall.normal.normalized*WallJumpSpeed+new Vector3(0, jumpSpeed, 0));
            Debug.Log("墙跳");
        }
        else
        {
            rb.velocity += new Vector3(0, jumpSpeed, 0);
        }

        blackboard.velocity = rb.velocity;//提前写入速度
    }

    public override void OnExit()
    {
        
    }

    public override void OnUpdate()
    {
        
    }

    public override void OnCheck()
    {
        
    }

    public override void OnFixUpdate()
    {
        
    }
    //速度求解器
    float Speed(float height)
    {
        return Mathf.Sqrt(2 * height * -Physics2D.gravity.y);
    }
}
