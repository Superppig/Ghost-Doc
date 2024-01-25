using System.Collections;
using System.Collections.Generic;
using Player_FSM;
using UnityEngine;

public class PlayerAirState : IState
{
    private PlayerBlackboard _playerBlackboard;

    
    private Vector3 moveDir;
    private Vector3 dirInput;
    
    private float airTransformAccelerate;
    private Rigidbody rb;


    
    public PlayerAirState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;


    }
    public void OnEnter()
    {
        rb = _playerBlackboard.otherSettings.m_rigidbody;
        
        rb.velocity = _playerBlackboard.otherSettings.speed;//初始化速度
        airTransformAccelerate = _playerBlackboard.airSettings.airTransformAccelerate;
    }

    public void OnExit()
    {
        
    }

    public void OnUpdate()
    {
        moveDir = _playerBlackboard.otherSettings.moveDir;
        MoveInAir();
    }

    public void OnCheck()
    {
        
    }

    public void OnFixUpdate()
    {
        
    }
    //空中转向
    void MoveInAir()
    {
        Vector3 XZSpeed = new Vector3(rb.velocity.x,0,rb.velocity.z);
        rb.velocity += moveDir * (airTransformAccelerate * Time.deltaTime);
        Vector3 dir = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;
        if ((moveDir.normalized + XZSpeed.normalized).magnitude > 0.1f)
            rb.velocity = new Vector3(0, rb.velocity.y, 0) + XZSpeed.magnitude * dir;
    }
}
