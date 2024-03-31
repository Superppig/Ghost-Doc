using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Gun: MonoBehaviour
{
    protected Transform pos;
    protected float bulletTimer;
    protected bool fireing;//射击
    protected bool canFire;
    protected bool aiming;//瞄准
    protected float fireWaitTime;
    protected float fireTimer = 0f;


    protected Animator gunAnimator;
    protected Transform orientation;//摄像机的transform
    protected Rigidbody rb;
    protected Player player;
    protected Camera _playerCam;

    public Transform model;
    public Transform trans;

    public bool firstFire;//切换武器时第一次射击
    
    protected virtual void Start()
    {
        canFire = true;
        gunAnimator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        rb=player.GetComponent<Rigidbody>();
        orientation = player.cameraTransform;
        _playerCam=Camera.main;
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
        if ((fireing && canFire)||firstFire)
        {
            Fire();
            gunAnimator.SetBool("fire",true);
            
            //后坐力
            //_playerCam.shotUp(Random.Range(-horOff,horOff),Random.Range(verOffMin,verOffMax),fireWaitTime);
            
            fireTimer = 0f;
            canFire = false;
            firstFire = false;
        }
        else
        {
            gunAnimator.SetBool("fire",false);
            fireTimer += Time.deltaTime;
            if (fireTimer > fireWaitTime)
            {
                canFire = true;
            }
        }
    }
    protected virtual void Fire()
    {
        
    }
}
