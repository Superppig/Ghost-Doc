using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Gun: MonoBehaviour
{
    protected Transform pos;
    protected float bulletTimer;
    protected bool fireing;//射击
    protected bool aiming;//瞄准
    protected float fireWaitTime;
    protected float fireTimer = 0f;
    //击中镜头晃动
    protected CinemachineVirtualCamera camImpulse;

    
    protected Animator gunAnimator;
    protected Transform orientation;//摄像机的transform
    protected Rigidbody rb;
    protected Camera _playerCam;

    public Transform model;
    public Transform trans;
    
    protected virtual void Start()
    {

    }
    protected virtual void Update()
    {
        MyInput();
        FireAction();
    }
    protected virtual void MyInput()
    {
        //射击
        if (Input.GetMouseButtonDown(0))
        {
            fireing = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            fireing = false;
        }
        //瞄准
        if (Input.GetMouseButtonDown(1))
        {
            aiming = !aiming;
            gunAnimator.SetBool("aim",aiming);
        }
    }

    protected virtual void FireAction()
    {
        fireTimer += Time.deltaTime;
        if (fireing && fireTimer>=fireWaitTime)
        {
            Fire();
            gunAnimator.SetTrigger("fire");
            
            //后坐力
            //_playerCam.shotUp(Random.Range(-horOff,horOff),Random.Range(verOffMin,verOffMax),fireWaitTime);
            
            fireTimer = 0f;
        }
    }
    protected virtual void Fire()
    {
        
    }
}
