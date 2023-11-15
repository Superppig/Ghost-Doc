using System.Threading;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("射击属性")] 
    public LayerMask enemyMask;
    public float damage;
    public float maxShootDistance;
    public Transform pos;
    

    private bool fireing;//射击
    private bool aiming;//瞄准

    private Bullet _bullet;
    public float fireRate = 700f;//射速
    private float fireWaitTime;
    private float fireTimer = 0f;
    [Header("后坐力")] 
    public float verOffMin = 2f;//水平偏移量
    public float verOffMax = 5f;//水平偏移量
    public float horOff = 1f;//垂直偏移量
    [Header("枪械")] 
    [SerializeField] private Animator gunAnimator;

    public Transform orientation;//摄像机的transform
    private Rigidbody rb;

    public PlayerCam _playerCam; 
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _bullet = Resources.Load<Bullet>("Perfabs/Bullet");
        fireWaitTime = 60f / fireRate;
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
            Bullet bullet = Instantiate(_bullet, pos.position, Quaternion.identity);
            bullet.end = hit.point;
            bullet.ready = true;
        }
        else
        {
            Bullet bullet = Instantiate(_bullet, pos.position, Quaternion.identity);
            bullet.end = orientation.transform.forward.normalized * maxShootDistance + pos.position;
            bullet.ready = true;
        }
    }
}
