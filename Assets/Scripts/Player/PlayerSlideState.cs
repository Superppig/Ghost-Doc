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
    private Vector3 moveDir;

    private float accelerate;//减速的加速度
    //摄像机
    private Camera cam;

    private VLineSummon vineLine;
    private Transform orientation;
    
    private float timer;
    private float vinelineTime;

    public PlayerSlideState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;
    }
    public void OnEnter()
    {
        rb = _playerBlackboard.m_rigidbody;
        rb.velocity = _playerBlackboard.speed;

        YScale = _playerBlackboard.slideYScale;
        accelerate = _playerBlackboard.slideAccelerate;
        slideSpeed = _playerBlackboard.speedMag;//继承向量
        
        vineLine= _playerBlackboard.vineLine;
        vinelineTime = _playerBlackboard.vineLineTime;
        orientation = _playerBlackboard.orientation;
        
        tr = rb.GetComponent<Transform>();
        cam = Camera.main;
        LastYScale = tr.localScale.y;
        StartSlide();

        cam.DOFieldOfView(63, 0.2f);
        timer = 0f;
    }

    public void OnExit()
    {
        tr.localScale = new Vector3(tr.localScale.x, LastYScale, tr.localScale.z);
        
        cam.DOFieldOfView(60, 0.2f);

    }

    public void OnUpdate()
    {
        Slide();
        timer+=Time.deltaTime;
        if (timer> vinelineTime)
        {
            vineLine.Summon(orientation.position, moveDir.normalized);
            timer = 0f;
        }
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
        slideSpeed -= accelerate * Time.deltaTime;
        slideSpeed = slideSpeed > 0 ? slideSpeed : 0;
        moveDir = _playerBlackboard.moveDir;
        rb.velocity = moveDir * slideSpeed;
    }
}