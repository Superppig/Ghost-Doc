using DG.Tweening;
using UnityEngine;

public class PlayerWallRunState : PlayerStateBase
{
    private float GRate => settings.wallRunSettings.wallRunGRate;
    private RaycastHit WallHit => blackboard.wallHit;
    private float WallRunSpeed => settings.wallRunSettings.wallRunSpeed;
    private float MaxWallTime => settings.wallRunSettings.maxWallTime;
    private float LeaveMaxAngel => settings.wallRunSettings.leaveMaxAngel;
    private float EnergyCostPerSecond => settings.wallRunSettings.energyCostPerSecond;

    //private float wallWalkForce;


    private float wallTimer;//计时器

    public PlayerWallRunState(Player player): base(player)
    {
    }

    public override void OnEnter()
    {
        rb.velocity = blackboard.velocity;
        player.blackboard.climbSpeed = new Vector3(rb.velocity.x,0,rb.velocity.z).magnitude;//继承墙壁速度
        wallTimer=0f;
        player.blackboard.hasClimbEnergyOut = false;
        //计算角度并判断是否超过最大角度
        if(Vector3.Angle(blackboard.climbXZDir,-1*WallHit.normal)>LeaveMaxAngel)
        {
            player.blackboard.hasClimbOverAngel = true;
        }
    }
    public override void OnExit()
    {
        player.cameraTransform.DOLocalRotate(Vector3.zero, 0.25f);
    }
    public override void OnUpdate()
    {
        WallRun();
    }
    public override void OnCheck()
    {
        
    }
    public override void OnFixUpdate()
    {
        if (wallTimer>=MaxWallTime)
        {
            player.UseEnerge(EnergyCostPerSecond/60f);
            if(player.GetEnerge()<0.1f)
            {
                player.blackboard.hasClimbEnergyOut = true;
            }
        }
    }

    private void WallRun()
    {
        //修改后(粘墙)
        wallTimer+= Time.deltaTime;
        Vector3 wallNormal = WallHit.normal;//墙壁法线
        rb.AddForce(wallNormal.normalized*(-1*100),ForceMode.Force);//向墙的力
        rb.AddForce(Vector3.up * (rb.mass * Physics.gravity.magnitude * GRate));//抵消部分重力的力
        if (wallTimer>=MaxWallTime)
        {
            player.blackboard.hasClimbOverTime = true;
        }
    }
}
