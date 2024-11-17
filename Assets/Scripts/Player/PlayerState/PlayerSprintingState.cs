using System;
using DG.Tweening;
using UnityEngine;

public class PlayerSprintingState : PlayerStateBase
{    
    private float SprintSpeed => settings.sprintSettings.sprintSpeed;
    private float LeaveSpeed => settings.sprintSettings.sprintLeaveSpeed;
    private float firstSpeed;
    
    private Vector3 sprintDir;

    private float ChangeRate => settings.otherSettings.sprintChangeRate;
    private float sprintTime;

    //逻辑变量
    private float timer;


    public PlayerSprintingState(Player player) : base(player)
    {
        sprintTime = settings.sprintSettings.sprintDistance / settings.sprintSettings.sprintSpeed;
    }

    public override void OnInit()
    {
        
    }

    public override void OnEnter()
    {
        firstSpeed = rb.velocity.magnitude;

        sprintDir = blackboard.moveDir.magnitude > 0.1f ? blackboard.moveDir : player.orientation.forward.normalized;
        
        rb.velocity = Vector3.zero;

        //镜头行为

        if (blackboard.dirInput.x > 0)
        {
            //镜头晃动
            player.cameraTransform.DOLocalRotate(new Vector3(0, 0, -3), 0.2f);
        }
        else if(blackboard.dirInput.x < 0)
        {
            player.cameraTransform.DOLocalRotate(new Vector3(0, 0, 3), 0.2f);
        }
        else
        {
            if (blackboard.dirInput.z < 0)
            {
                Camera.main.DOFieldOfView(65, 0.2f);
            }
            else
            {
                Camera.main.DOFieldOfView(55, 0.2f);
            }
        }
        player.vineLine.Summon(player.orientation.position, sprintDir.normalized);
        
        //初始滑逻辑变量
        timer = 0f;
    }

    public override void OnFixedUpdate()
    {
    }

    public override void OnExit()
    { //冲刺跳
        float rate=firstSpeed;//正常为冲刺前速度

        
        
        
        rb.velocity = sprintDir * rate;
        blackboard.velocity = sprintDir * rate;
        blackboard.speed = rate;

        player.cameraTransform.DOLocalRotate(new Vector3(0, 0, 0), 0.2f);
        Camera.main.DOFieldOfView(60, 0.2f);
    }

    public override void OnShutdown()
    {
    }

    public override void OnUpdate()
    {
        rb.velocity = sprintDir * SprintSpeed;
        timer += Time.deltaTime;
        
        if(timer>=sprintTime)
        {
            if (blackboard.grounded)
            {
                CurrentFsm.ChangeState<PlayerWalkingState>();
            }
            else
            {
                CurrentFsm.ChangeState<PlayerAirState>();
            }
        }
        
    }

    public override void OnCheck()
    {
    }
}
