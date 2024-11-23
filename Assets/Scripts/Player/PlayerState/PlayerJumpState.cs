using Services;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerJumpState : PlayerStateBase
{
    private float jumpSpeed;

    private float WalkSpeed => ServiceLocator.Get<BuffSystem>().GetBuffedSpeed(settings.walkSettings.walkSpeed);
    private RaycastHit wall;
    private float JumpTime => settings.jumpSettings.jumpTime;
    
    private float timer = 0f;

    public PlayerJumpState(Player player) : base(player)
    {
    }

    private bool IsWallJump => blackboard.isWallJump;

    public override void OnInit()
    {
        
    }

    public override void OnEnter()
    {
        timer = 0f;
        
        float height = settings.jumpSettings.height;

        //获取速度
        rb.velocity = blackboard.velocity;
        

        jumpSpeed = Speed(height);
        if (blackboard.doubleJump)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpSpeed, rb.velocity.z);
        }
        else
        {
            rb.velocity += new Vector3(0, jumpSpeed, 0);
        }
        ServiceLocator.Get<ScreenControl>().ParticleRelease(settings.otherSettings.JumpParticle, player.GetGround().point, player.GetGround().normal);
            
        /*}*/
        blackboard.velocity = rb.velocity;//提前写入速度
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

    public override void OnUpdate()
    {
        timer += Time.deltaTime;
        if(timer>=JumpTime)
        {
            blackboard.doubleJump = !blackboard.doubleJump;
            CurrentFsm.ChangeState<PlayerAirState>();
        }
    }

    public override void OnCheck()
    {
        
    }


    //速度求解器
    float Speed(float height)
    {
        return Mathf.Sqrt(2 * height * -Physics2D.gravity.y);
    }
}
