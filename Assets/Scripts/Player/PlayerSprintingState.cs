using System.Collections;
using System.Collections.Generic;
using Player_FSM;
using UnityEngine;

public class PlayerSprintingState : IState
{
    private PlayerBlackboard _playerBlackboard;
    private Rigidbody rb;
    private Transform oritation;
    
    private float sprintSpeed;
    private float firstSpeed;
    
    private Vector3 sprintDir;

    
    public PlayerSprintingState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;
    }
    
    
    public void OnEnter()
    {
        rb = _playerBlackboard.m_rigidbody;
        sprintSpeed = _playerBlackboard.sprintSpeed;
        oritation = _playerBlackboard.oritation;


        firstSpeed = rb.velocity.magnitude;
        sprintDir = (_playerBlackboard.moveDir.x * oritation.right + _playerBlackboard.moveDir.z * oritation.forward)
            .normalized;

        rb.velocity = Vector3.zero;
    }

    public void OnExit()
    {
        rb.velocity = sprintDir * firstSpeed;
        _playerBlackboard.speed=sprintDir * firstSpeed;
    }

    public void OnUpdate()
    {
        rb.velocity = sprintDir * sprintSpeed;
    }

    public void OnCheck()
    {
        
    }

    public void OnFixUpdate()
    {
        
    }
}
