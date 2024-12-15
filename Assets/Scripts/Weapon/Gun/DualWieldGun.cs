using System;
using Services.ObjectPools;
using System.Collections;
using Services;
using UnityEngine;

public class DualWieldGun : Gun
{
    [Header("双持冲锋枪")] 
    public DualWieldGunData data;
    public float currentTemperature;
    public Transform[] position;
    
    
    private bool isLeft;//左右射击
    public bool isReachMax;
    private float frozenTimer;
    
    
    //记录世界时间
    private float worldTimer;

    //温度状态
    public enum DualWieldGunState
    {
        Idle,
        Up,
        Frozen,
        Down
    }
    
    public  DualWieldGunState state = DualWieldGunState.Idle;
    
    [Header("对象池")] protected MyObject selfMyObject;

    
    protected override void Start()
    {
        base.Start();
        //初始化数据
        fireWaitTime = 60f / data.fireRate;
        
        //注册方法
        selfMyObject = GetComponent<MyObject>();
        selfMyObject.OnActivate+= SelfActivate;
        selfMyObject.OnRecycle += SelfRecycle;

    }

    protected override void Update()
    {
        MyInput();
        FireAction();
        StateCon();
    }

    protected void StateCon()
    {
        switch (state)
        {
            case DualWieldGunState.Idle:
                isReachMax = false;
                if(fireing)
                    state=DualWieldGunState.Up;
                break;
            case DualWieldGunState.Up:
                TempUp();
                break;
            case DualWieldGunState.Frozen:
                TempFrozen();
                break;
            case DualWieldGunState.Down:
                TempDown();
                break;
        }
    }
    void TempUp()
    {
        currentTemperature += data.temperatureUpSpeed * Time.deltaTime;
        currentTemperature = Mathf.Clamp(currentTemperature, 0, data.temperatureMax);
        if(data.temperatureMax-currentTemperature<Mathf.Epsilon || !fireing)
            state = DualWieldGunState.Frozen;
        
        if(data.temperatureMax-currentTemperature<Mathf.Epsilon)
            isReachMax= true;
    }
    void TempFrozen()
    {
        frozenTimer += Time.deltaTime;
        if (frozenTimer > data.temperatureFrozenTime)
        {
            state = DualWieldGunState.Down;
            frozenTimer = 0f;
        }

        if (fireing&&data.temperatureMax-currentTemperature>Mathf.Epsilon)
        {
            frozenTimer = 0f;
            state = DualWieldGunState.Up;
        }
    }
    void TempDown()
    {
        currentTemperature -= data.temperatureDownSpeed * Time.deltaTime;
        currentTemperature = Mathf.Clamp(currentTemperature, 0, data.temperatureMax);
        if (currentTemperature < Mathf.Epsilon)
            state = DualWieldGunState.Idle;
        if(fireing&&!isReachMax)
            state = DualWieldGunState.Up;
    }

    protected override void FireAction()
    {
        if ((fireing && canFire)||firstFire)
        {
            Fire();
            //gunAnimator.SetBool("fire",true);
            
            //后坐力
            //_playerCam.shotUp(Random.Range(-horOff,horOff),Random.Range(verOffMin,verOffMax),fireWaitTime);
            
            fireTimer = 0f;
            canFire = false;
            firstFire = false;
        }
        else
        {
            //gunAnimator.SetBool("fire",false);
            fireTimer += Time.deltaTime;
            if (fireTimer > fireWaitTime && !isReachMax)
            {
                canFire = true;
            }
        }
    }

    protected override void Fire()
    {
        //求出圆内一个随机点
        Vector3 point = Tools.RandomTool.RandomPointsInCircle(data.bulletCircleRadius, 1)[0];
        //求四元数并加bulletCircleDistance,求出ray并储存
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, orientation.forward);
        Vector3 dir = rotation * (point + new Vector3(0, 0, data.bulletCircleDistance));
        Ray fireRay = new Ray(orientation.transform.position, dir);
        RaycastHit hit;
        
        if (Physics.Raycast(fireRay, out hit, data.maxShootDistance))
        {
            //左右射击
            if (isLeft)
            {
                StartCoroutine(BulletStart(position[0].position, hit.point));
            }
            else
            {
                StartCoroutine(BulletStart(position[1].position, hit.point));
            }
            
            //判断击中物体
            if (hit.collider.CompareTag("Enemy"))
            {
                if (hit.collider.GetComponent<IEnemyBeHit>() == null)
                {
                    return;
                }
                //相机振动
                ServiceLocator.Get<ScreenControl>().CamShake(data.impulseTime, data.impulseAmplitude);
                IEnemyBeHit enemyBeHit = hit.collider.GetComponent<IEnemyBeHit>();
                
                enemyBeHit.HitEnemy(new HitInfo(){damage = data.damage});
            }
            else
            {
                ServiceLocator.Get<ScreenControl>().ParticleRelease(data.hitBuildingParticle,hit.point,hit.normal);
            }
        }

        if (isLeft)
        {
            ServiceLocator.Get<ScreenControl>().ParticleRelease(data.fireParticle,position[0].position,position[0].forward);
            StartCoroutine(BulletStart(position[0].position, orientation.transform.position+orientation.transform.forward.normalized*data.maxShootDistance));
        }
        else
        {
            ServiceLocator.Get<ScreenControl>().ParticleRelease(data.fireParticle,position[1].position,position[0].forward);
            StartCoroutine(BulletStart(position[1].position, orientation.transform.position+orientation.transform.forward.normalized*data.maxShootDistance));
        }
        isLeft=!isLeft;
        //gunAnimator.SetBool("isLeft",isLeft);
    }
    IEnumerator BulletStart(Vector3 start,Vector3 end)
    {
        LineRenderer bullet= Instantiate(data.bullet);
        bullet.SetPosition(0, start);
        bullet.SetPosition(1, end);
        bulletTimer = 0f;
        float halfBulletTime = data.bulletTime / 2f;

        while (bulletTimer < data.bulletTime)
        {
            bulletTimer += Time.deltaTime;
            float lerpFactor = Mathf.Lerp(0f, 1f, bulletTimer / data.bulletTime);

            bullet.startWidth = data.lineWindth * Mathf.Lerp(2f, 0f, lerpFactor);
            bullet.endWidth = bullet.startWidth;
            yield return null;
        }
        bullet.startWidth = 0f;
        bullet.endWidth = 0f;
        Destroy(bullet.gameObject);
    }

    
    //在后台冷却
    public void SelfRecycle()
    {
        worldTimer = Time.time;
    }

    public void SelfActivate()
    {
        float time = Time.time - worldTimer;
        currentTemperature -= time;
        currentTemperature = Mathf.Clamp(currentTemperature, 0, data.temperatureMax);
    }
}
