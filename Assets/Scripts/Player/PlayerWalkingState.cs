using System.Collections;
using Player_FSM;
using UnityEngine;

public class PlayerWalkingState : IState
{
    private PlayerBlackboard _playerBlackboard;
    
    private float walkSpeed;
    private float moveForce;
    private Vector3 moveDir;
    private Rigidbody rb;

    public PlayerWalkingState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;
    }
    
    public void OnEnter()
    {
        rb = _playerBlackboard.m_rigidbody;
        walkSpeed = _playerBlackboard.walkSpeed;
        moveForce = _playerBlackboard.VerForce;
    }
    public void OnExit()
    {
        
    }
    public void OnUpdate()
    {
        moveDir = _playerBlackboard.moveDir;
        SpeedCon();
    }
    public void OnCheck()
    {
        
    }
    public void OnFixUpdate()
    {
        Walk();
    }

    void Walk()
    {
        rb.AddForce(moveDir.normalized*moveForce,ForceMode.Force);
    }

    void SpeedCon()
    {
        if (rb.velocity.magnitude > walkSpeed)
        {
            rb.velocity=rb.velocity.normalized*walkSpeed;
        }
    }
}
