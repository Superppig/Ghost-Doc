using Unity.VisualScripting;
using UnityEngine;

public class PlayerJumpState : PlayerStateBase
{
    private float jumpSpeed;

    private float WallJumpSpeed => settings.jumpSettings.wallJumpSpeed;
    private float WalkSpeed => settings.walkSettings.walkSpeed;
    private float WallUpSpeed => settings.jumpSettings.wallUpSpeed;
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
        
        /*if (blackboard.lastState == EStateType.Sliding)
            height *= settings.otherSettings.slideToJumpHeightRate;
        else */
        if(blackboard.lastState != EStateType.Sprinting && blackboard.lastState != EStateType.WallRunning)
        {
            //修正速度
            Vector3 XZSpeed = new Vector3(rb.velocity.x,0,rb.velocity.z);
            rb.velocity = XZSpeed.magnitude>WalkSpeed?XZSpeed.normalized*WalkSpeed:XZSpeed;
        }
        
        jumpSpeed = Speed(height);
        /*if (IsWallJump)
        {
            wall = blackboard.wallHit;
            //判断墙跳类型
            // 设定最小墙跳速度
            float speed = blackboard.climbSpeed > WallJumpSpeed
                ? blackboard.climbSpeed
                : WallJumpSpeed;
            
            
            Debug.Log("hasClimbOverTime"+blackboard.hasClimbOverTime);
            Debug.Log("hasClimbOverAngle"+blackboard.hasClimbOverAngel);
            //x > 某个角度时上墙且快速跳离，可以理解为玩家想要类似跑墙的效果，此时可以给玩家一个固定的脱离墙壁的速度方向。
            if (!blackboard.hasClimbOverTime&& blackboard.hasClimbOverAngel)
            {
                rb.velocity = (Vector3.Reflect(blackboard.climbXZDir,blackboard.wallHit.normal).normalized*speed+new Vector3(0, jumpSpeed, 0));
            }
            //x < 某个角度时上墙且快速跳离，可以理解为玩家是想要向墙上跳或是垂直于墙壁跳开
            else if (!blackboard.hasClimbOverTime&& !blackboard.hasClimbOverAngel)
            {
                rb.velocity = (wall.normal.normalized * WallUpSpeed + new Vector3(0, jumpSpeed, 0));
                Debug.Log(rb.velocity);
            }
            //玩家非快速跳离，可以理解为玩家想要在墙上呆一会、判断形势。
            else
            {
                rb.velocity = (wall.normal.normalized*speed+new Vector3(0, jumpSpeed, 0));
            }
            //速度线和顿帧
            player.vineLine.Summon(player.transform.position,
                new Vector3(player.rb.velocity.x, 0, player.rb.velocity.z), 0.1f);
            ScreenControl.Instance.FrameFrozen(5,0.1f);
            //重置参数
            player.blackboard.hasClimbOverTime = false;
            player.blackboard.hasClimbOverAngel = false;
            
            //墙跳的粒子效果
            ScreenControl.Instance.ParticleRelease(settings.otherSettings.JumpParticle, wall.point+new Vector3(0,-0.5f * settings.airSettings.playerHeight ,0), wall.normal);
        }
        else
        {*/
            rb.velocity += new Vector3(0, jumpSpeed, 0);
            
            //跳跃的粒子效果
            ScreenControl.Instance.ParticleRelease(settings.otherSettings.JumpParticle, player.GetGround().point, player.GetGround().normal);
            
        /*}*/

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
