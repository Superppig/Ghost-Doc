using DG.Tweening;
using UnityEngine;

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
    //public Transform camHolder;
    //private Camera cam;
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
    
    [Header("武器动作")] 
    private Player player;
    private PlayerSettings _playerBlackboard;
    //呼吸摇动变量
    private Transform weaponSwayObject;
    public float swayAmountA = 1;
    public float swayAmountB = 2;
    public float swayScale = 600;
    public float swayLerpSpeed = 14;
    public float swayTime;
    public Vector3 swayPosition;
    //y轴晃动幅度
    public float gunRotation;
    private Transform gunRotate;

    private bool hasRotate;
    
    
    
    void Start()
    {
        //cam = GetComponent<Camera>();
        //锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        //获取变量
        player = GetComponentInParent<Player>();
        weaponSwayObject = player.gunTrans;
        gunRotate = player.gunModel;
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
        transform.rotation=Quaternion.Euler(Mathf.Clamp(-(xRotation+verCurrunt), -1*YView, YView),yRotation+horCurrent,0);
        orientation.rotation=Quaternion.Euler(0,yRotation+horCurrent,0);

        //武器移动效果
        GunMove();
    }

    private void GunMove()
    {
        //枪械摇动
        if (mouthX > 0.01f)
        {
            if (!hasRotate)
            {
                gunRotate.DOLocalRotate(new Vector3(0, -gunRotation, 0), 0.25f);
                hasRotate = true;
            }
        }
        else if (mouthX < -0.01f)
        {
            if (!hasRotate)
            {
                gunRotate.DOLocalRotate(new Vector3(0, gunRotation, 0), 0.25f);
                hasRotate = true;
            }        
        }
        else
        {
            gunRotate.DOLocalRotate(Vector3.zero, 0.25f);
            hasRotate = false;
            CalculateWeaponSway();
        }
    }
    
    //呼吸摇动
    private void CalculateWeaponSway()
    {
        var targetPosition = LissajousCurve(swayTime, swayAmountA, swayAmountB)/swayScale;

        swayPosition = Vector3.Lerp(swayPosition, targetPosition, Time.smoothDeltaTime);

        swayTime += Time.deltaTime;

        if (swayTime>2*Mathf.PI)
        {
            swayTime = 0;
        }

        if (weaponSwayObject==null)
        {
            weaponSwayObject = player.gunTrans;
        }
        weaponSwayObject.localPosition = swayPosition;
    }
    
    
    //求解李萨如图
    private Vector3 LissajousCurve(float Time, float A, float B)
    {
        return new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.PI));
    }
    
    //视角倾斜
    public void DoFov(float endValue)
    {
        //cam.DOFieldOfView(endValue, 0.25f);
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
