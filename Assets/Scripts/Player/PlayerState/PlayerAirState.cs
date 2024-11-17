using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAirState : PlayerStateBase
{
    private float AirTransformAccelerate => settings.airSettings.airTransformAccelerate;
    private float AirStopAccelerate => settings.airSettings.airStopAccelerate;
    private float MaxAirSpeed => BuffSystem.Instance.GetBuffedSpeed(settings.walkSettings.walkSpeed);

    private Vector3 MoveDir => blackboard.moveDir;

    private float speed;

    //视角变化
    private float currentFov = 60f;
    private float targetFov = 55f;
    private float FovChangeSpeed = 0.01f;
    private float timer;

    public PlayerAirState(Player player) : base(player)
    {
    }

    public override void OnShutdown()
    {
    }

    public override void OnCheck()
    {
        
    }

    public override void OnInit()
    {
        
    }

    public override void OnEnter()
    {
        rb.velocity = blackboard.velocity;//初始化速度
        timer = 0f;
        blackboard.hasClimbOverTime = false;//初始化
        
    }

    public override void OnExit()
    {
        Camera.main.DOFieldOfView(currentFov, 0.2f);
    }

    public override void OnFixedUpdate()
    {
        FovChange();
    }

    public override void OnUpdate()
    {
        if (!blackboard.isWallJump)
        {
            MoveInAir();
        }

        if(Input.GetKeyDown(settings.keySettings.sprintKey))
        {
            if (player.UseEnerge(100))
            {
                CurrentFsm.ChangeState<PlayerSprintingState>();
            }
        }
        else if (blackboard.grounded)
        {
            CurrentFsm.ChangeState<PlayerWalkingState>();
        }
        else if (Input.GetKeyDown(settings.keySettings.jumpkey)&&blackboard.doubleJump)
        {
            CurrentFsm.ChangeState<PlayerJumpState>();
        }
        
    }
    //空中转向
    private void MoveInAir()
    {
        Vector3 XZSpeed = new Vector3(rb.velocity.x,0,rb.velocity.z);

        if (blackboard.dirInput.magnitude > 0)
        {
            rb.velocity += MoveDir * (AirTransformAccelerate * Time.deltaTime);
            Vector3 dir = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;
            if (blackboard.isWall&&Vector3.Angle(-1*blackboard.wallHit.normal,XZSpeed)<30f)
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }
            else
            {
                //保持水平动量
                if ((MoveDir.normalized + XZSpeed.normalized).magnitude > 0.1f&& XZSpeed.magnitude > MaxAirSpeed)
                    rb.velocity = new Vector3(0, rb.velocity.y, 0) + XZSpeed.magnitude * dir;
            }
        }
        else
        {
            speed = XZSpeed.magnitude;
            speed -= AirStopAccelerate * Time.deltaTime;
            rb.velocity = new Vector3(0, rb.velocity.y, 0) + speed * XZSpeed.normalized;
        }
    }
    //FOV变化
    private void FovChange()
    {
        timer += Time.fixedDeltaTime;
        Camera.main.DOFieldOfView(SigmoidFunction(timer * FovChangeSpeed-100000f) * (targetFov - currentFov) + currentFov,Time.fixedDeltaTime) ;
    }
    
    // Sigmoid 函数
    float SigmoidFunction(float x)
    {
        return 1f / (1f + Mathf.Exp(-x));
    }
}
