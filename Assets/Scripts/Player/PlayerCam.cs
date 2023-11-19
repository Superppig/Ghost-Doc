using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class PlayerCam : MonoBehaviour
{
    private float mouthX;
    private float mouthY;
    
    [Header("鼠标灵敏度")]
    public float sensX;
    public float sensY;
    [Header("最大俯仰角")]
    public float YView;

    public Transform orientation;//Player方向
    public Transform camHolder;
    private Camera cam;
    [Header("枪械后坐力")] 
    private float verTotalOff;//水平总偏移
    private float horTotalOff;//垂直总偏移
    private float verCurrunt;
    private float horCurrent;
    private float timeToPos;
    private bool fired;
    //旋转所需变量
    private float xRotation;
    private float yRotation;
    void Start()
    {
        cam = GetComponent<Camera>();
        //锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        //获取鼠标
        mouthX = Input.GetAxisRaw("Mouse X");
        mouthY = Input.GetAxisRaw("Mouse Y");

        yRotation += mouthX * Time.deltaTime * sensX;
        //压枪
        if (verTotalOff > 0 & mouthY < 0)
        {
            verTotalOff += mouthY * sensY * Time.deltaTime;
        }
        else
        {
            xRotation += mouthY * Time.deltaTime * sensY;
        }
        
        //后坐力
        //准星上跳
        if (fired)
        {
            verCurrunt = Mathf.Lerp(verCurrunt,verTotalOff,Time.deltaTime*10f);
            horCurrent = Mathf.Lerp(horCurrent,horTotalOff,Time.deltaTime*10f);
            
            timeToPos -= Time.deltaTime;
            if (timeToPos<0)
            {
                fired = false;
            }
        }
        //准星归位
        else
        {
            if (Mathf.Abs(verCurrunt) > 0.1f)
            {
                //线性插值归位
                verCurrunt = Mathf.Lerp(verTotalOff,0f,Time.deltaTime*10f);
                verTotalOff = verCurrunt;
            }
            else
            {
                //直接归位
                verCurrunt = 0f;
                verTotalOff = 0f;
            }
            
            if (Mathf.Abs(horCurrent) > 0.1f)
            {
                //线性插值归位
                horCurrent = Mathf.Lerp(horTotalOff,0f,Time.deltaTime*10f);
                horTotalOff = horCurrent;
            }
            else
            {
                //直接归位
                horCurrent = 0f;
                horTotalOff = 0f;
            }
        }

        xRotation = Mathf.Clamp(xRotation, -1 * YView, YView);
        //旋转
        camHolder.rotation=Quaternion.Euler(Mathf.Clamp(-(xRotation+verCurrunt), -1*YView, YView),yRotation+horCurrent,0);
        orientation.rotation=Quaternion.Euler(0,yRotation+horCurrent,0);
    }
    //视角倾斜
    public void DoFov(float endValue)
    {
        cam.DOFieldOfView(endValue, 0.25f);
    }
    public void DoTile(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt),0.25f);
    }
    public void shotUp(float horOff,float verOff,float time)
    {
        verTotalOff += verOff;
        horTotalOff += horOff;
        timeToPos = time;
        fired = true;
    }
}
