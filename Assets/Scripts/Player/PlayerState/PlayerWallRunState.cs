using DG.Tweening;
using UnityEngine;

public class PlayerWallRunState : PlayerStateBase
{
    private float GRate => settings.wallRunSettings.wallRunGRate;
    private RaycastHit CurrentWall => blackboard.currentWall;
    private float WallRunSpeed => settings.wallRunSettings.wallRunSpeed;
    private float MaxWallTime => settings.wallRunSettings.maxWallTime;
    

    //private float wallWalkForce;


    private float wallTimer;//计时器

    public PlayerWallRunState(Player player): base(player)
    {
    }

    public override void OnEnter()
    {
        rb.velocity = blackboard.velocity;
        player.blackboard.climbSpeed = rb.velocity.magnitude;//继承墙壁速度
        wallTimer=0f;
        if (blackboard.isLeft)
        {
            player.cameraTransform.DOLocalRotate(new Vector3(0, 0, -10), 0.25f);
        }
        else
        {
            player.cameraTransform.DOLocalRotate(new Vector3(0, 0, 10), 0.25f);
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
        
    }

    private void WallRun()
    {
        
        //修改前
        /*Vector3 wallNormal = CurrentWall.normal;//墙壁法线
        Vector3 wallForward = Vector3.Cross(wallNormal, rbTransform.up);//墙壁向前距离
        if ((player.orientation.forward - wallForward).magnitude > (player.orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;//调整向前方向

        rb.velocity = wallForward.normalized * speed;//向前速度
        rb.AddForce(Vector3.up * (rb.mass * Physics.gravity.magnitude * GRate));//抵消部分重力的力
        rb.AddForce(CurrentWall.normal.normalized*(-1*100),ForceMode.Force);//向墙的力*/
        
        //修改后(粘墙)
        wallTimer+= Time.deltaTime;
        Vector3 wallNormal = CurrentWall.normal;//墙壁法线
        rb.AddForce(CurrentWall.normal.normalized*(-1*100),ForceMode.Force);//向墙的力
        rb.AddForce(Vector3.up * (rb.mass * Physics.gravity.magnitude * GRate));//抵消部分重力的力
        if (wallTimer>=MaxWallTime)
        {
            player.blackboard.hasClimbOverTime = true;
        }
    }
}
