using DG.Tweening;
using UnityEngine;

public class PlayerWallRunState : PlayerStateBase
{
    private float GRate => settings.wallRunningSettings.wallRunGRate;
    private RaycastHit CurrentWall => blackboard.currentWall;
    private float WallRunSpeed => settings.wallRunningSettings.wallRunSpeed;
    
    private float speed;//速度

    //private float wallWalkForce;

    public PlayerWallRunState(Player player): base(player)
    {
    }

    public override void OnEnter()
    {
        rb.velocity = blackboard.velocity;
        speed = Mathf.Max(blackboard.speed, WallRunSpeed);
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
        Vector3 wallNormal = CurrentWall.normal;//墙壁法线
        Vector3 wallForward = Vector3.Cross(wallNormal, rbTransform.up);//墙壁向前距离
        if ((player.orientation.forward - wallForward).magnitude > (player.orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;//调整向前方向

        rb.velocity = wallForward.normalized * speed;//向前速度
        rb.AddForce(Vector3.up * (rb.mass * Physics.gravity.magnitude * GRate));//抵消部分重力的力
        rb.AddForce(CurrentWall.normal.normalized*(-1*100),ForceMode.Force);//向墙的力
    }
}
