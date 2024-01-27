using UnityEngine;

public class PlayerAirState : PlayerStateBase
{
    private float AirTransformAccelerate => settings.airSettings.airTransformAccelerate;

    private Vector3 MoveDir => blackboard.moveDir;

    public PlayerAirState(Player player) : base(player)
    {
    }

    public override void OnCheck()
    {
        
    }

    public override void OnEnter()
    {
        rb.velocity = blackboard.velocity;//初始化速度
    }

    public override void OnExit()
    {
        
    }

    public override void OnFixUpdate()
    {
        
    }

    public override void OnUpdate()
    {
        MoveInAir();
    }
    //空中转向
    private void MoveInAir()
    {
        Vector3 XZSpeed = new Vector3(rb.velocity.x,0,rb.velocity.z);
        rb.velocity += MoveDir * (AirTransformAccelerate * Time.deltaTime);
        Vector3 dir = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;
        if ((MoveDir.normalized + XZSpeed.normalized).magnitude > 0.1f)
            rb.velocity = new Vector3(0, rb.velocity.y, 0) + XZSpeed.magnitude * dir;
    }
}
