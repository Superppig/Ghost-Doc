using System.Collections;
using System.Collections.Generic;
using Player_FSM;
using UnityEngine;

public class PlayerSlideState : IState
{
    private PlayerBlackboard _playerBlackboard;

    private Rigidbody rb;
    private Transform tr;
    private Transform oritation;
    private float LastYScale;
    private float YScale;
    private float slideSpeed;
    private float accelerate;
    private Vector3 moveDir;
    public PlayerSlideState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;
    }
    public void OnEnter()
    {
        rb = _playerBlackboard.m_rigidbody;
        YScale = _playerBlackboard.slideYScale;
        slideSpeed = _playerBlackboard.slideSpeed;
        accelerate = _playerBlackboard.accelerate;
        oritation = _playerBlackboard.oritation;
        tr = rb.GetComponent<Transform>();
        LastYScale = tr.localScale.y;
        StartSlide();
    }

    public void OnExit()
    {
        _playerBlackboard.speed = rb.velocity;
        tr.localScale = new Vector3(tr.localScale.x, LastYScale, tr.localScale.z);
    }

    public void OnUpdate()
    {
        Slide();
    }

    public void OnCheck()
    {
    }

    public void OnFixUpdate()
    {
    }

    void StartSlide()
    {
        tr.localScale = new Vector3(tr.localScale.x, YScale, tr.localScale.z);
        //给一个瞬间力吸附到地面上
        rb.AddForce(Vector3.down*10f,ForceMode.Impulse);
    }

    void Slide()
    {
        moveDir = (_playerBlackboard.moveDir.x*oritation.right+_playerBlackboard.moveDir.z*oritation.forward).normalized;
        rb.velocity = moveDir * slideSpeed;
    }
}