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

        height = _playerBlackboard.height;
        last = _playerBlackboard.last;
        slideHeightRate = _playerBlackboard.slideToJumpHeightRate;

        jumpSpeed = last==EStateType.Sliding? Speed(height*slideHeightRate):Speed(height);//滑铲跳跳跃高度改变
        rb = _playerBlackboard.m_rigidbody;
        rb.velocity = _playerBlackboard.speed;
        wallSpeed = _playerBlackboard.wallJumpSpeed;

        isWallJump = _playerBlackboard.isWallJump;
        if (isWallJump)
        {
            wall = _playerBlackboard.currentWall;
            rb.velocity += (wall.normal.normalized*wallSpeed+new Vector3(0, jumpSpeed, 0));
            Debug.Log("墙跳");
        }
        else
        {
            rb.velocity += new Vector3(0, jumpSpeed, 0);
        }

        _playerBlackboard.speed = rb.velocity;//提前写入速度
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
