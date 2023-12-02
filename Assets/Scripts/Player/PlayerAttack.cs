using System.Collections;
using System.Threading;
using Cinemachine;
using UnityEngine;
using DG.Tweening;

public class PlayerAttack : MonoBehaviour
{
    [Header("射击属性")] 
    public LayerMask enemyMask;
    public float damageRate;
    public float maxShootDistance;
    public Transform pos;
    [Header("子弹(激光)")] 
    public LineRenderer bullet;

    public float lineWindth=0.1f;

    public float bulletTime;
    private float bulletTimer;
    private float bulletMaxWidth;

    private bool fireing;//射击
    private bool aiming;//瞄准

    public float fireRate = 700f;//射速
    private float fireWaitTime;
    private float fireTimer = 0f;
    [Header("后坐力")] 
    public float verOffMin = 2f;//水平偏移量
    public float verOffMax = 5f;//水平偏移量
    public float horOff = 1f;//垂直偏移量
    [Header("枪械")] 
    [SerializeField] private Animator gunAnimator;

    [Header("相机")] 
    public CinemachineImpulseSource camImpulse;
    public Transform orientation;//摄像机的transform
    public float impulseTime;
    [Header("效果")] 
    public ParticleSystem hitEenemyParticle;
    public ParticleSystem hitBuildingParticle;
    public ParticleSystem fireParticle;
    public float hitScale=0.5f;
    public float fireScale=0.5f;
    
    private Rigidbody rb;

    private Camera _playerCam; 
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        fireWaitTime = 60f / fireRate;
        _playerCam = Camera.main;

        bullet.startWidth = 0f;
        bullet.endWidth = 0f;

        camImpulse = GetComponent<CinemachineImpulseSource>();
    }
    void Update()
    {
        MyInput();
        FireAction();
    }

    private void MyInput()
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
    private void FireAction()
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
    private void Fire()
    {
        Ray fireRay = new Ray(orientation.transform.position, orientation.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(fireRay, out hit, maxShootDistance))
        {
            
            //相机振动
            camImpulse.GenerateImpulse(impulseTime);
            StartCoroutine(BulletStart(pos.position, hit.point));
            if (hit.collider.CompareTag("Enemy"))
            {
                HitPartical(hit,hitEenemyParticle);
                IEnemyBeHit enemyBeHit = hit.collider.GetComponent<IEnemyBeHit>();
                enemyBeHit.HitEnemy(damageRate);
            }
            else
            {
                HitPartical(hit,hitBuildingParticle);
            }
        }
        else
        {
            StartCoroutine(BulletStart(pos.position, orientation.transform.position+orientation.transform.forward.normalized*maxShootDistance));
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
        bullet.SetPosition(0, start);
        bullet.SetPosition(1, end);
        bulletTimer = 0f;
        float halfBulletTime = bulletTime / 2f;

        while (bulletTimer < bulletTime)
        {
            bulletTimer += Time.deltaTime;
            float lerpFactor = Mathf.Lerp(0f, 1f, bulletTimer / bulletTime);

            bullet.startWidth = lineWindth * Mathf.Lerp(2f, 0f, lerpFactor);
            bullet.endWidth = bullet.startWidth;
            yield return null;
        }
        bullet.startWidth = 0f;
        bullet.endWidth = 0f;
    }

    private void FirePartical()
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, pos.forward);
        ParticleSystem fire = Instantiate(fireParticle, pos.position, rotation);

        Destroy(fire.gameObject,fire.main.duration);
    }

    private void HitPartical(RaycastHit hit, ParticleSystem hitParticle)
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, hit.normal.normalized);
        ParticleSystem fire = Instantiate(hitParticle,hit.point , rotation);

        Destroy(fire.gameObject,fire.main.duration);
    }
}
