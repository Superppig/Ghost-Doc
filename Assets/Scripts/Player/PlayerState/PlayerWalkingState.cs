using DG.Tweening;
using Services;
using UnityEngine;

public class PlayerWalkingState : PlayerStateBase
{
    private float WalkSpeed => ServiceLocator.Get<BuffSystem>().GetBuffedSpeed(settings.walkSettings.walkSpeed);
    private float Accelerate => settings.walkSettings.accelerate;
    private Vector3 MoveDir => blackboard.moveDir;
    private Vector3 DirInput => blackboard.dirInput;

    private float CovoteTime => settings.otherSettings.walkToSlideCovoteTime;
    //private EStateType next;
    
    private bool isMeleeAttack => blackboard.isMeleeAttacking;

    //逻辑变量
    private float timer;
    private float firstSpeed;
    
    private Tween velocityTween;

    public PlayerWalkingState(Player player) : base(player)
    {
    }

    public override void OnInit()
    {
        
    }

    public override void OnEnter()
    {
        //初始化逻辑变量
        timer = 0f;
        firstSpeed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;

        velocityTween = null;
    }

    public override void OnFixedUpdate()
    {
    }

    public override void OnExit()
    {
        //退出时复原视角
        player.cameraTransform.DOLocalRotate(new Vector3(0, 0, 0), 0.2f);
        
        //脚步视角变化
        player.playerCam.isWalk = false;

        //动量继承
        //next = _playerBlackboard.next;
        //AudioManager.Instance.StopSound(player.transform);
        
        velocityTween?.Kill();
    }

    public override void OnShutdown()
    {
    }

    public override void OnUpdate()
    {
        Walk();

        if (DirInput.magnitude>0)
        {
            player.playerCam.isWalk = true;
        }
        else
        {
            player.playerCam.isWalk = false;
        }
        
        
        if (DirInput.magnitude <= 0.1f)
        {
            CurrentFsm.ChangeState<PlayerIdelState>();
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
        else if(!blackboard.grounded)
        {
            blackboard.doubleJump = !blackboard.doubleJump;
            CurrentFsm.ChangeState<PlayerAirState>();
        }
        
        SpeedCon();
    }
    public override void OnCheck()
    {
        
    }

    
    private void Walk()
    {
        rb.velocity += MoveDir * (Time.deltaTime * Accelerate);
        if (!isMeleeAttack&& blackboard.canCamChange)
        {
            if (DirInput.x < 0)
            {
                player.cameraTransform.DOLocalRotate(new Vector3(0, 0, 1), 0.2f);

            }
            else if (DirInput.x>0)
            {
                player.cameraTransform.DOLocalRotate(new Vector3(0, 0, -1), 0.2f);
            }
            else
            {
                player.cameraTransform.DOLocalRotate(new Vector3(0, 0, 0), 0.2f);
            }
        }
    }
    private void SpeedCon()
    {

        if (rb.velocity.magnitude>WalkSpeed)
        {
            rb.velocity = rb.velocity.normalized * WalkSpeed;
        }

        if (MoveDir.magnitude<0.1f&&rb.velocity.magnitude>0.1f)
        {
            if (velocityTween == null)
            {
                velocityTween = DOTween.To(() => rb.velocity, x => rb.velocity = x, Vector3.zero, 0.05f)
                    .SetEase(Ease.InOutQuad);
            }
        }
        else
        {
            velocityTween?.Kill();
            velocityTween = null;
        }
    }
}
