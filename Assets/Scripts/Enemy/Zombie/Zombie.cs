using DG.Tweening;
using UnityEngine;
using Pathfinding;

public class Zombie : Enemy
{
    public float headDamage;
    public float bodyDamage;
    public Player player;
    
    [Header("攻击")]
    public float attackRate;
    public EnemyBullet bullet;
    private float timer;
    
    [Header("寻路相关")]
    private Vector3 targetPosition;
    private Seeker seeker;
    //存储路径
    public Path path;
    //角色移动速度
    public float speed = 100.0f;

    public float turnTime = 0.5f;//转向时间
    //判断玩家与航点的距离
    public float nextWaypointDistance = 3;
    //对当前的航点进行编号
    private int currentWaypoint = 0;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        //注册回调函数，在Astar Path完成后调用此函数
        seeker.pathCallback += OnPathComplete;
        player=GameObject.FindWithTag("Player").GetComponent<Player>();


        timer = 0f;
    }
    
    void Update()
    {
        Attack();
        Dead();
    }
    void FixedUpdate()
    {
        Find();
    }

    private void Find()
    {
        targetPosition = player.transform.position;
        //开始寻路
        seeker.StartPath(transform.position, targetPosition);
        if (path==null)
        {
            return;
        }
        //当前搜索点编号大于等于路径存储的总点数时，路径搜索结束
        if (currentWaypoint>=path.vectorPath.Count)
        {
            //Debug.Log("路径搜索结束");
            return;
        }
        //确定当前物体方向
        Vector3 dir = (path.vectorPath[currentWaypoint+1] - transform.position);
        dir = new Vector3(dir.x, 0, dir.z).normalized;
        //玩家转向

        transform.position += dir * (Time.fixedDeltaTime * speed);
        //玩家当前位置与当前的航向点距离小于一个给定值后，转向下一个航向点
        if (Vector3.Distance(transform.position,path.vectorPath[currentWaypoint])<nextWaypointDistance)
        {
            Vector3 rotation = new Vector3(0, Quaternion.FromToRotation(transform.forward, dir).eulerAngles.y+transform.rotation.eulerAngles.y,0) ;
            transform.DOLocalRotate(rotation, turnTime);
            currentWaypoint++;
            return;
        }
    }

    void Attack()
    {
        float time = 1 / attackRate;
        timer+=Time.deltaTime;
        if (timer>time)
        {
            timer = 0f;
            EnemyBullet bullet1 = Instantiate(bullet, transform.position, Quaternion.identity);
            bullet1.dir = (player.transform.position - transform.position).normalized;
            bullet1.damage = damage;
        }
    }



    /// <summary>
    /// 当寻路结束后调用这个函数
    /// </summary>
    /// <param name="p"></param>
    private void OnPathComplete(Path p)
    {
        //Debug.Log("发现这个路线"+p.error);
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }
    private void OnDisable()
    {
        seeker.pathCallback -= OnPathComplete;
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        //其他效果
    }
}
