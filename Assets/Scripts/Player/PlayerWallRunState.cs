using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Player_FSM;
using UnityEngine;

public class PlayerWallRunState : IState
{
    private PlayerBlackboard _playerBlackboard;

    private Rigidbody rb;
    private Transform trans;
    private Transform ori;
    private float rate;
    private RaycastHit wall;//储存墙壁信息
    private Transform cam;
    private bool isLeft;
    
    private float speed;//速度

    private float wallWalkForce;

    public PlayerWallRunState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;
    }
    public void OnEnter()
    {
        rb = _playerBlackboard.m_rigidbody;
        wall = _playerBlackboard.currentWall;
        rate = _playerBlackboard.wallRunGRate;
        isLeft = _playerBlackboard.leftWall;

        trans = rb.GetComponent<Transform>();
        cam = Camera.main.GetComponent<Transform>();
        ori = _playerBlackboard.oritation;

        speed = _playerBlackboard.wallRunSpeed;

        if (isLeft)
        {
            //镜头晃动
            cam.DOLocalRotate(new Vector3(0, 0, -10), 0.25f);
        }
        else
        {
            cam.DOLocalRotate(new Vector3(0, 0, 10), 0.25f);
        }
    }
    public void OnExit()
    {
        cam.DOLocalRotate(Vector3.zero, 0.25f);

    }
    public void OnUpdate()
    {
        WallRun();
    }
    public void OnCheck()
    {
        
    }
    public void OnFixUpdate()
    {
        
    }

    void WallRun()
    {
        Vector3 wallNormal = wall.normal;//墙壁法线
        Vector3 wallForward = Vector3.Cross(wallNormal, trans.up);//墙壁向前距离
        if ((ori.forward - wallForward).magnitude > (ori.forward - -wallForward).magnitude)
            wallForward = -wallForward;//调整向前方向

        rb.velocity = wallForward.normalized * speed;//向前速度
        rb.AddForce(Vector3.up*(rb.mass*Physics.gravity.magnitude*rate));
    }
}
