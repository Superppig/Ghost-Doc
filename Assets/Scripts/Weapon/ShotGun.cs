using System.Collections;
using System.Collections.Generic;
using Data.WeaponData;
using UnityEngine;

public class ShotGun : Gun
{
    [Header("霰弹枪")] public ShotGunData data;

    public Transform position;



    protected override void Start()
    {
        base.Start();
        //初始化数据
        pos = position;
        fireWaitTime = 60f / data.fireRate;
    }

    protected override void FireAction()
    {
        if (fireing && canFire||firstFire)
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


    protected override void Fire()
    {
        //求出圆面上的点
        List<Vector3> points =
            Tools.RandomTool.RandomPointsInCircle(data.bulletCircleRadius, data.bulletCount, data.bulletMinDistance);
        //求四元数并加bulletCircleDistance,求出ray并储存
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, orientation.forward);

        foreach (Vector3 point in points)
        {
            Vector3 newPoint = rotation * (point + new Vector3(0, 0, data.bulletCircleDistance)) ;
            Ray fireRay = new Ray(orientation.position, newPoint);
            RaycastHit hit;
            if (Physics.Raycast(fireRay,out hit,data.maxShootDistance))
            {
                //StartCoroutine(BulletStart(pos.position, hit.point));
                if (hit.collider.CompareTag("Enemy"))
                {
                    //相机振动
                    ScreenControl.Instance.CamShake(data.impulseTime, data.impulseAmplitude);
                    //粒子效果
                    ScreenControl.Instance.ParticleRelease(data.hitEenemyParticle,hit.point,hit.normal);
                    IEnemyBeHit enemyBeHit = hit.collider.GetComponent<IEnemyBeHit>();
                    enemyBeHit.HitEnemy(data.damageRate);
                }
                else
                {
                    ScreenControl.Instance.ParticleRelease(data.hitBuildingParticle,hit.point,hit.normal);
                }
            }
            else
            {
                //未击中
            }
        }
        ScreenControl.Instance.ParticleRelease(data.fireParticle,pos.position,pos.forward);
        ScreenControl.Instance.CamChange(new Vector3(-5,0,0),0.1f);
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
}
