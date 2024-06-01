using System.Collections;
using Cinemachine;
using Data.WeaponData;
using UnityEngine;
using DG.Tweening;
using Services;
using Services.Audio;

public class CommonGun : Gun
{
    [Header("普通左轮")] 
    public CommonGundata data;
    public Transform position;
    public Light pointLight;
    private IAudioPlayer audioPlayer;

    private void Awake()
    {
        audioPlayer = ServiceLocator.Get<IAudioPlayer>();
    }

    protected override void Start()
    {
        base.Start();
        //初始化数据
         pos = position;
         fireWaitTime = 60f / data.fireRate;
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void Fire()
    { 
        LayerMask mask = ~(1<<13);
        Ray fireRay = new Ray(orientation.transform.position, orientation.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(fireRay, out hit, data.maxShootDistance,Physics.DefaultRaycastLayers&mask))
        {
            StartCoroutine(BulletStart(pos.position, hit.point));
            if (hit.collider.CompareTag("Enemy"))
            {
                //相机振动
                ScreenControl.Instance.CamShake(data.impulseTime, data.impulseAmplitude);
                ScreenControl.Instance.ParticleRelease(data.hitEenemyParticle,hit.point,hit.normal);
                IEnemyBeHit enemyBeHit = hit.collider.GetComponent<IEnemyBeHit>();
                enemyBeHit.HitEnemy(new HitInfo(){rate = data.damageRate});
            }
            else
            {
                ScreenControl.Instance.ParticleRelease(data.hitBuildingParticle,hit.point,hit.normal);
            }
        }
        else
        {
            StartCoroutine(BulletStart(pos.position, orientation.transform.position+orientation.transform.forward.normalized*data.maxShootDistance));
        }
        ScreenControl.Instance.ParticleRelease(data.fireParticle, pos.position, pos.forward);
        audioPlayer.CreateAudioByGroup("Fire_Magnum", transform.position, -1, transform);
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
        pointLight.enabled= true;
        
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
        
        pointLight.enabled = false;
    }


}