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

    public PlayerJumpState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;
    }


    public void OnEnter()
    {
        //读取黑板数据

        height = _playerBlackboard.height;
        jumpSpeed = Speed(height);
        rb = _playerBlackboard.m_rigidbody;
        rb.velocity = _playerBlackboard.speed;
        wallSpeed = _playerBlackboard.wallJumpSpeed;


        if (isWallJump)
        {
            wall = _playerBlackboard.currentWall;
            rb.velocity += (wall.normal.normalized*wallSpeed+new Vector3(0, jumpSpeed, 0));
        }
        else
        {
            rb.velocity += new Vector3(0, jumpSpeed, 0);
        }
    }

    public void OnExit()
    {
        _playerBlackboard.speed = rb.velocity;
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
