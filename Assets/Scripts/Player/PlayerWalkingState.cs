using Player_FSM;
using UnityEngine;

public class PlayerWalkingState : IState
{
    private PlayerBlackboard _playerBlackboard;
    
    private float walkSpeed;
    private float accelerate;
    private Vector3 moveDir;
    private Rigidbody rb;
    private Transform oritation;

    public PlayerWalkingState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;
    }
    
    public void OnEnter()
    {
        //读取黑板数据

        rb = _playerBlackboard.m_rigidbody;
        walkSpeed = _playerBlackboard.walkSpeed;
        accelerate = _playerBlackboard.accelerate;
        oritation = _playerBlackboard.oritation;
        
        rb.velocity = _playerBlackboard.speed;
    }
    public void OnExit()
    {
        _playerBlackboard.speed = rb.velocity;
    }
    public void OnUpdate()
    {
        moveDir = (_playerBlackboard.moveDir.x*oritation.right+_playerBlackboard.moveDir.z*oritation.forward).normalized;
        Walk();
        SpeedCon();
    }
    public void OnCheck()
    {
        
    }
    public void OnFixUpdate()
    {
        
    }
    
    void Walk()
    {
        rb.velocity += moveDir * (Time.deltaTime * accelerate);
    }
    void SpeedCon()
    {
        Vector2 XZSpeed = new Vector2(rb.velocity.x, rb.velocity.z);
        if (XZSpeed.magnitude>walkSpeed)
        {
            XZSpeed = XZSpeed.normalized * walkSpeed;
            rb.velocity = new Vector3(XZSpeed.x, rb.velocity.y, XZSpeed.y);
        }
    }
}
