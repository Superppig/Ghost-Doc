using System.Collections;
using System.Collections.Generic;
using Player_FSM;
using UnityEngine;

public class PlayerCrouchState : IState
{
    private PlayerBlackboard _playerBlackboard;
    
    private Rigidbody rb;
    private Transform tr;
    private Transform oritation;
    private float LastYScale;
    private float YScale;
    private float crouchSpeed;
    private float accelerate;
    private Vector3 moveDir;
    public PlayerCrouchState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;
    }
    public void OnEnter()
    {
        rb = _playerBlackboard.m_rigidbody;
        YScale = _playerBlackboard.crouchYScale;
        crouchSpeed = _playerBlackboard.crouchSpeed;
        accelerate = _playerBlackboard.accelerate;
        oritation = _playerBlackboard.oritation;
        tr = rb.GetComponent<Transform>();
        LastYScale = tr.localScale.y;
        Crouch();
    }

    public void OnExit()
    {
        _playerBlackboard.speed = rb.velocity;
        tr.localScale = new Vector3(tr.localScale.x, LastYScale, tr.localScale.z);
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

    void Crouch()
    {
        tr.localScale = new Vector3(tr.localScale.x, YScale, tr.localScale.z);
        //给一个瞬间力吸附到地面上
        rb.AddForce(Vector3.down*10f,ForceMode.Impulse);
    }

    void Walk()
    {
        rb.velocity += moveDir * (Time.deltaTime * accelerate);
    }

    void SpeedCon()
    {
        Vector2 XZSpeed = new Vector2(rb.velocity.x, rb.velocity.z);
        if (XZSpeed.magnitude>crouchSpeed)
        {
            XZSpeed = XZSpeed.normalized * crouchSpeed;
            rb.velocity = new Vector3(XZSpeed.x, rb.velocity.y, XZSpeed.y);
        }
    }
}
