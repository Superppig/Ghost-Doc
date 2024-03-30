using DG.Tweening;
using Player_FSM;
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
    private EStateType next;//下一个状态
    private float timer;
    
    

    public PlayerSprintingState(Player player) : base(player)
    {
        sprintTime = settings.sprintSettings.sprintDistance / settings.sprintSettings.sprintSpeed;
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

    public override void OnExit()
    {
        next = blackboard.nextState;
        //冲刺跳
        float rate=firstSpeed;//正常为冲刺前速度
        if (next == EStateType.Jumping)
        {
            if (player.GetEnerge() > 100)
            {
                player.UseEnerge(100);
                //rate = ChangeRate * ((timer / sprintTime < 1 ? timer / sprintTime : 1)*(SprintSpeed-firstSpeed)+firstSpeed);//在first和sprint速度之间线性取值
                //改为固定速度
                rate = LeaveSpeed;
                
                blackboard.sprintingPause= true;//冲刺打断
            }
            else
            {
                //消耗失败
                player.TakeEnergeFailAudio();
            }
        }

        if (next == EStateType.WallRunning)
        {
            blackboard.sprintingPause= true;//冲刺打断
        }
        
        
        rb.velocity = sprintDir * rate;
        blackboard.velocity = sprintDir * rate;
        blackboard.speed = rate;

        player.cameraTransform.DOLocalRotate(new Vector3(0, 0, 0), 0.2f);
        Camera.main.DOFieldOfView(60, 0.2f);
    }

    public override void OnUpdate()
    {
        rb.velocity = sprintDir * SprintSpeed;
        timer += Time.deltaTime;
        
    }

    public override void OnCheck()
    {
        
    }

    public override void OnFixUpdate()
    {
        
    }
}
