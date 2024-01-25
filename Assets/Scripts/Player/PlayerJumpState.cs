using Player_FSM;
using UnityEngine;

public class PlayerJumpState : IState
{
    private PlayerBlackboard _playerBlackboard;
    private float jumpSpeed;

    private float wallSpeed;
    private RaycastHit wall;
    private bool isWallJump;
    
    private float height;
    private Rigidbody rb;

    private float slideHeightRate;

    //逻辑变量
    private EStateType last;
    

    public PlayerJumpState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;
    }


    public void OnEnter()
    {
        //读取黑板数据

        height = _playerBlackboard.jumpSettings.height;
        last = _playerBlackboard.otherSettings.last;
        slideHeightRate = _playerBlackboard.otherSettings.slideToJumpHeightRate;

        jumpSpeed = last==EStateType.Sliding? Speed(height*slideHeightRate):Speed(height);//滑铲跳跳跃高度改变
        rb = _playerBlackboard.otherSettings.m_rigidbody;
        rb.velocity = _playerBlackboard.otherSettings.speed;
        wallSpeed = _playerBlackboard.jumpSettings.wallJumpSpeed;

        isWallJump = _playerBlackboard.jumpSettings.isWallJump;
        if (isWallJump)
        {
            wall = _playerBlackboard.wallRunningSettings.currentWall;
            rb.velocity += (wall.normal.normalized*wallSpeed+new Vector3(0, jumpSpeed, 0));
            Debug.Log("墙跳");
        }
        else
        {
            rb.velocity += new Vector3(0, jumpSpeed, 0);
        }

        _playerBlackboard.otherSettings.speed = rb.velocity;//提前写入速度
    }

    public void OnExit()
    {
        
    }

    public void OnUpdate()
    {
        
    }

    public void OnCheck()
    {
        
    }

    public void OnFixUpdate()
    {
        
    }
    //速度求解器
    float Speed(float height)
    {
        return Mathf.Sqrt(2 * height * -Physics2D.gravity.y);
    }
}
