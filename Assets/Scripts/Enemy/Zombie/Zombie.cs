using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Zombie : Enemy
{
    public float headDamage;
    public float bodyDamage;
    public GameObject player;
    private Vector3 targetPosition;

    private Seeker seeker;
    
    //存储路径
    public Path path;
    //角色移动速度
    public float speed = 100.0f;
    public float turnSpeed = 5f;
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
        player=GameObject.FindWithTag("Player");
    }
    void FixedUpdate()
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
            Debug.Log("路径搜索结束");
            return;
        }

        Vector3 dir = (path.vectorPath[currentWaypoint+1] - transform.position);//.normalized;
        dir *= speed * Time.fixedDeltaTime;
       
        //玩家转向
        transform.Translate(Vector3.forward*Time.fixedDeltaTime*speed);
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        //玩家当前位置与当前的航向点距离小于一个给定值后，转向下一个航向点
        if (Vector3.Distance(transform.position,path.vectorPath[currentWaypoint])<nextWaypointDistance)
        {
            currentWaypoint++;
            return;
        }

    }
    /// <summary>
    /// 当寻路结束后调用这个函数
    /// </summary>
    /// <param name="p"></param>
    private void OnPathComplete(Path p)
    {
        Debug.Log("发现这个路线"+p.error);
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

    // Update is called once per frame
    void Update()
    {
        Dead();
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        //其他效果
    }
}
