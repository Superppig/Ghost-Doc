using System.Collections;
using System.Collections.Generic;
using Data.WeaponData;
using Services;
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
                    ServiceLocator.Get<ScreenControl>().CamShake(data.impulseTime, data.impulseAmplitude);
                    //粒子效果
                    ServiceLocator.Get<ScreenControl>().ParticleRelease(data.hitEenemyParticle,hit.point,hit.normal);
                    IEnemyBeHit enemyBeHit = hit.collider.GetComponent<IEnemyBeHit>();
                    enemyBeHit.HitEnemy(new HitInfo(){damage = data.damage});
                }
                else
                {
                    ServiceLocator.Get<ScreenControl>().ParticleRelease(data.hitBuildingParticle,hit.point,hit.normal);
                }
            }
            else
            {
                //未击中
            }
        }
        ServiceLocator.Get<ScreenControl>().ParticleRelease(data.fireParticle,pos.position,pos.forward);
        ServiceLocator.Get<ScreenControl>().CamChange(new Vector3(-5,0,0),0.1f);
    }
}
