using DG.Tweening;
using Pathfinding;
using Services;
using Services.ObjectPools;
using UnityEngine;

public enum EnemyType
{
    Zombie,
    RemoteEnemy,
    CrazyBiteEnemy,
}

public class Enemy : MonoBehaviour,IGrabObject
{
    public EnemyBlackboard blackboard;
    

    public Animator anim;
    public Rigidbody rb;
    public Collider col;
    public MyObject selfMyObject;

    
    public bool canGrab;
    
    //core
    public bool hasCore;
    public GameObject core;
    protected EnemyCore enemyCore;
    public Vector3 corePosition = Vector3.zero;
    
    //wave
    public Wave wave;
    private WaveManager waveManager;
    
    [Header("寻路相关")]
    public Vector3 targetPosition;
    protected Seeker seeker;
    //存储路径
    public Path path;
    //判断玩家与航点的距离
    public float nextWaypointDistance = 1;
    //对当前的航点进行编号
    protected int currentWaypoint = 0;
    public float minDistance = 1f;
    
    //score
    private ScoreManager scoreManager;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider>();
        
        waveManager = ServiceLocator.Get<WaveManager>();
        scoreManager = ServiceLocator.Get<ScoreManager>();
        //注册对象池回调函数
        selfMyObject = GetComponent<MyObject>();
        selfMyObject.OnRecycle += EnemyOnDisable;
        selfMyObject.OnActivate += EnemyOnInable;
        selfMyObject.OnActivate+= ReBorn;
        
        
        seeker = GetComponent<Seeker>();
    }

    protected virtual void Start()
    {
        //注册回调函数，在Astar Path完成后调用此函数
        seeker.pathCallback += OnPathComplete;
        AstarPath.active.logPathResults = PathLog.None;//关闭寻路日志
    }
    protected virtual void Update()
    {

    }
    protected virtual void FixedUpdate()
    {
        if (hasCore)
        {
            enemyCore.transform.localRotation = Quaternion.identity;
            enemyCore.transform.localPosition = corePosition;
        }
    }
    public virtual void TakeDamage(float damage)
    {

    }

    //寻路
    public void Find(Vector3 targetPosition)
    {
        //开始寻路
        seeker.StartPath(transform.position, targetPosition);
        if (path==null)
        {
            return;
        }
        
        //当前搜索点编号大于等于路径存储的总点数时，路径搜索结束
        if (currentWaypoint>=path.vectorPath.Count)
        {
            Debug.Log("路径搜索结束");
            return;
        }

        Vector3 dir = Vector3.zero;
        if (path.vectorPath.Count > currentWaypoint+1)
            dir = (path.vectorPath[currentWaypoint+1] - transform.position);
        
        if (Vector3.Distance(transform.position, targetPosition) > blackboard.directRange)
        {
            //确定当前物体方向
            dir = new Vector3(dir.x, 0, dir.z).normalized;
        }
        else
        {
            dir = (targetPosition - transform.position).normalized;
            dir = new Vector3(dir.x, 0, dir.z).normalized;
        }

        //移动
        rb.velocity = dir * blackboard.speed;

        //当前位置与当前的航向点距离小于一个给定值后，转向下一个航向点
        if (Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance)
        {
            Vector3 rotation = new Vector3(0,
                Quaternion.FromToRotation(transform.forward, dir).eulerAngles.y + transform.rotation.eulerAngles.y, 0);
            transform.DOLocalRotate(rotation, blackboard.trunTime);
            currentWaypoint++;
            return;
        }
    }
    /// <summary>
    /// 当寻路结束后调用这个函数
    /// </summary>
    /// <param name="p"></param>
    protected void OnPathComplete(Path p)
    {
        //Debug.Log("发现这个路线"+p.error);
        if (!p.error)
        {
        }
        path = p;
        currentWaypoint = 0;
    }
    public float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, blackboard.player.transform.position);
    }
    public Vector3 DirToPlayer()
    {
        return new Vector3(blackboard.player.transform.position.x - transform.position.x, 
            0,
            blackboard.player.transform.position.z - transform.position.z).normalized;
    }

    protected virtual void EnemyOnDisable()
    {
        wave.currentEnemyCount--;
        wave.currentEnemyHealth-=(blackboard.commonHealth+blackboard.weakHealth);
        transform.position = Vector3.zero;

        scoreManager.AddScore(50f);
        //Debug.Log("敌人死亡")
        //回收核心
        if (hasCore)
        {
            enemyCore.transform.position = corePosition;
            Destroy(enemyCore.gameObject);
            hasCore = false;
        }
    }
    protected virtual void EnemyOnInable()
    {
        wave = waveManager.GetCurrentWave();
        blackboard.player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        wave.currentEnemyCount++;
        wave.currentEnemyMaxHealth+=(blackboard.commonHealth+blackboard.weakHealth);
        wave.currentEnemyHealth+=(blackboard.commonHealth+blackboard.weakHealth);
        Debug.Log("敌人生成");
        
        //生成核心
        if (!hasCore)
        {
            enemyCore = Instantiate(core, transform).GetComponent<EnemyCore>();
            enemyCore.transform.localPosition = corePosition;
            enemyCore.boomDamage = blackboard.boomDamage;
            enemyCore.boomForce = blackboard.boomForce;
            enemyCore.boomRange = blackboard.boomRange;
            enemyCore.boomParticle = blackboard.boom;
            hasCore = true;
        }
    }
    void ReBorn()
    {
        blackboard.currentHealth = blackboard.commonHealth+ blackboard.weakHealth;
    }

    public virtual Transform GetTransform()
    {
        return null;
    }

    public virtual void Grabbed(){}

    public virtual void Released(){}

    public virtual void Fly(Vector3 dir, float force){}

    public virtual bool CanGrab(){return false;}

    public virtual bool CanUse(){return false;}

    public virtual void Use(){}

    public IGrabObject GetGrabObject()
    {
        if (!hasCore)
        {
            return null;
        }
        hasCore = false;
        return enemyCore as IGrabObject;
    }


    public virtual bool OnGround()
    {
        return Physics.Raycast(transform.position, Vector3.down, 2f, LayerMask.GetMask("Ground"));
    }
}
