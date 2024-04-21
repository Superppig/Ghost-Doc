using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using DG.Tweening;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("基础属性")] 
    public float shield;
    public float health;
    public float damage;
    public float wallDamage;//撞墙伤害
    
    [Header("玩家相关")]
    public float headDamage;
    public float bodyDamage;
    public Player player;
    protected Animator anim;
    [Header("子弹攻击")]
    public float attackRate;
    public EnemyBullet bullet;
    protected float timer;
    
    [Header("寻路相关")]
    public float directRange;
    protected Vector3 targetPosition;
    protected Seeker seeker;
    //存储路径
    public Path path;
    //角色移动速度
    public float speed = 100.0f;
    public float turnTime = 0.5f;//转向时间
    //判断玩家与航点的距离
    public float nextWaypointDistance = 3;
    //对当前的航点进行编号
    protected int currentWaypoint = 0;
    [Header("敌人基础")]
    public EnemyBaseState enemyState=EnemyBaseState.Common;
    public enum EnemyBaseState
    {
        Common,
        Move,
        Unbalanced,
        Bomb,
        Dead
    }
    public float UnbalancedTime;
    public float RecoverySpeed;
    public bool canMove;
    protected float unbalancedTimer;
    protected bool isUnbalanced;
    
    [Header("击飞效果相关")]
    [HideInInspector]public Rigidbody rb;
    public float hitMinSpeed;//最小受伤速度
    public float collideDecayRate;//碰撞衰减率
    public bool isStrikToFly;
    public float findAngle;
    [HideInInspector]
    public Transform nextEnemy;
    public float popMinDistance;//爆炸距离
    public float popForce;//爆炸力
    public float popDamage;//爆炸伤害   
    public float popWaitTime;//爆炸等待时间

    protected static List<Transform> enemyList = new List<Transform>();


    protected void Awake()
    {
        enemyList.Add(transform);
    }

    protected virtual void Start()
    {
        seeker = GetComponent<Seeker>();
        //注册回调函数，在Astar Path完成后调用此函数
        seeker.pathCallback += OnPathComplete;
        AstarPath.active.logPathResults = PathLog.None;//关闭寻路日志
        player=GameObject.FindWithTag("Player").GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
        timer = 0f;
    }
    protected virtual void Update()
    {
        Dead();
        Find();
    }
    protected virtual void FixedUpdate()
    {
    }

    protected virtual void StateControl()
    {
        switch (enemyState)
        {
            case EnemyBaseState.Unbalanced:
                //失衡视觉效果
                Unbalanced();
                break;
            case EnemyBaseState.Dead:
                Dead();
                break;
            default:break;
        }
    }

    protected virtual void StateChange()
    {
        if(health<=0&& enemyState!=EnemyBaseState.Dead&&enemyState!=EnemyBaseState.Bomb)
        {
            enemyState = EnemyBaseState.Unbalanced;
        }
    }



    public virtual void TakeDamage(float damage,bool isBomb = false)
    {
        Debug.Log(isBomb);
        if (enemyState == EnemyBaseState.Common)
        {
            shield -= damage;
            shield = shield < 0 ? 0 : shield;
            if (shield <=0)
            {
                enemyState = EnemyBaseState.Move;
            }
        }
        else if (enemyState != EnemyBaseState.Dead && enemyState != EnemyBaseState.Unbalanced)
        {
            health -= damage;
        }
        else if (enemyState == EnemyBaseState.Unbalanced)
        {
            if (isBomb)
            {
                enemyState = EnemyBaseState.Bomb;
                BombState(popWaitTime);
            }
            else
            {
                enemyState = EnemyBaseState.Dead;
            }
        }
    }
    //爆炸状态,用于爆炸敌人
    protected virtual void BombState(float time)
    {
        
    }
    
    protected virtual void Unbalanced()
    {
        if (!isUnbalanced)
        {
            anim.SetTrigger("Unbalanced");
            isUnbalanced = true;
        }
    }

    protected virtual void Dead()
    {
    }

    protected virtual void Hit()
    {
        
    }

    public virtual void BeStrickToFly(Vector3 dir,float speed,float time)
    {
    }

    public virtual bool FindNextEnemy(float length,float angle,Vector3 dir)
    {
        //按照其他敌人与自己的距离进行排序
        enemyList.Sort((a, b) =>
        {
            if (Vector3.Distance(a.position, transform.position) < Vector3.Distance(b.position, transform.position))
            {
                return -1;
            }
            return 1;
        });
        
        
        //for循环找到第一个在视野范围内的敌人
        for (int i = 1; i <enemyList.Count; i++)
        {
            if (enemyList.Count == 1)
            {
                return false;
            }
            if(Vector3.Distance(transform.position,enemyList[i].position)>length)
            {
                return false;
            }
            if(Vector3.Angle(dir,enemyList[i].position-transform.position)<angle)
            {
                nextEnemy = enemyList[i];
                Debug.DrawLine(enemyList[i].position,transform.position,Color.red,10f);
                return true;
            }
        }
        return false;
    }
    
    //寻路
    protected void Find()
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
        Vector3 dir = (path.vectorPath[currentWaypoint+1] - transform.position);
        if (Vector3.Distance(transform.position, player.transform.position) > directRange)
        {
            //确定当前物体方向
            dir = new Vector3(dir.x, 0, dir.z).normalized;
        }
        else
        {
            dir = (player.transform.position - transform.position).normalized;
            dir = new Vector3(dir.x, 0, dir.z).normalized;
        }

        //玩家转向
        transform.position += dir * (Time.fixedDeltaTime * speed);

        //玩家当前位置与当前的航向点距离小于一个给定值后，转向下一个航向点
        if (Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance)
        {
            Vector3 rotation = new Vector3(0,
                Quaternion.FromToRotation(transform.forward, dir).eulerAngles.y + transform.rotation.eulerAngles.y, 0);
            transform.DOLocalRotate(rotation, turnTime);
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
    protected void OnDestroy()
    {
        //删去enemylist中的对应敌人
        enemyList.Remove(transform);
        
        //seeker.pathCallback -= OnPathComplete;
    }
    
    
}
