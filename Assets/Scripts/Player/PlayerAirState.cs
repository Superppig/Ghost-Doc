using System.Collections;
using System.Collections.Generic;
using Player_FSM;
using UnityEngine;

public class PlayerAirState : IState
{
    private PlayerBlackboard _playerBlackboard;

    
    private Vector3 moveDir;
    private float airTransformAccelerate;
    private Rigidbody rb;
    private Transform oritation;


    
    public PlayerAirState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;
        
        rb = _playerBlackboard.m_rigidbody;
        oritation = _playerBlackboard.oritation;
        
        rb.velocity = _playerBlackboard.speed;//初始化速度
        airTransformAccelerate = _playerBlackboard.airTransformAccelerate;

    }
    public void OnEnter()
    {
        
    }

    public void OnExit()
    {
        _playerBlackboard.speed = rb.velocity;
    }

    public void OnUpdate()
    {
        moveDir = (_playerBlackboard.moveDir.x*oritation.right+_playerBlackboard.moveDir.z*oritation.forward).normalized;
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
        Vector2 XZSpeed = new Vector2(rb.velocity.x,rb.velocity.z);
        rb.velocity += moveDir * (airTransformAccelerate * Time.deltaTime);
        Vector3 dir = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;
        rb.velocity = new Vector3(0, rb.velocity.y, 0) + XZSpeed.magnitude * dir;
    }
}
