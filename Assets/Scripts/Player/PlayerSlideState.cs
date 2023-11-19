using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Player_FSM;
using UnityEngine;

public class PlayerSlideState : IState
{
    private PlayerBlackboard _playerBlackboard;

    private Rigidbody rb;
    private Transform tr;
    private float LastYScale;
    private float YScale;
    private float slideSpeed;
    private float accelerate;
    private Vector3 moveDir;
    
    //摄像机
    private Camera cam;
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
        tr = rb.GetComponent<Transform>();
        cam = Camera.main;
        LastYScale = tr.localScale.y;
        StartSlide();

        cam.DOFieldOfView(70, 0.1f);
    }

    public void OnExit()
    {
        _playerBlackboard.speed = rb.velocity;
        tr.localScale = new Vector3(tr.localScale.x, LastYScale, tr.localScale.z);
        
        cam.DOFieldOfView(60, 0.1f);

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
        moveDir = _playerBlackboard.moveDir;
        rb.velocity = moveDir * slideSpeed;
    }
}