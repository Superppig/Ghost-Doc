using System.Collections;
using Cinemachine;
using Data.WeaponData;
using UnityEngine;
using DG.Tweening;

public class CommonGun : Gun
{
    [Header("普通左轮")] 
    public CommonGundata data;
    public Transform position;

    private Player player;
    

    protected override void Start()
    {
        base.Start();
        //初始化数据
         pos = position;
         gunAnimator = GetComponent<Animator>();
         player = GameObject.FindWithTag("Player").GetComponent<Player>();
         camImpulse = Camera.main.GetComponent<CinemachineVirtualCamera>();
         rb=player.GetComponent<Rigidbody>();
         orientation = player.cameraTransform;
         _playerCam=Camera.main;
         
         fireWaitTime = 60f / data.fireRate;
         
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void Fire()
    {
        Ray fireRay = new Ray(orientation.transform.position, orientation.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(fireRay, out hit, data.maxShootDistance))
        {
            StartCoroutine(BulletStart(pos.position, hit.point));
            if (hit.collider.CompareTag("Enemy"))
            {
                //相机振动
                ScreenControl.Instance.CamShake(data.impulseTime, data.impulseAmplitude);
                HitPartical(hit,data.hitEenemyParticle);
                IEnemyBeHit enemyBeHit = hit.collider.GetComponent<IEnemyBeHit>();
                enemyBeHit.HitEnemy(data.damageRate);
            }
            else
            {
                HitPartical(hit,data.hitBuildingParticle);
            }
        }
        else
        {
            StartCoroutine(BulletStart(pos.position, orientation.transform.position+orientation.transform.forward.normalized*data.maxShootDistance));
        }
        FirePartical();

        StartCoroutine(CamChange());
    }

    IEnumerator CamChange()
    {
        //镜头缩放
        _playerCam.DOFieldOfView(59.5f, 0.05f);
        yield return new WaitForSeconds(0.05f);
        _playerCam.DOFieldOfView(60, 0.05f);
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

    private void FirePartical()
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, pos.forward);
        ParticleSystem fire = Instantiate(data.fireParticle, pos.position, rotation);

        Destroy(fire.gameObject,fire.main.duration);
    }

    private void HitPartical(RaycastHit hit, ParticleSystem hitParticle)
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, hit.normal.normalized);
        ParticleSystem fire = Instantiate(hitParticle,hit.point , rotation);
        Destroy(fire.gameObject,fire.main.duration);
    }
}