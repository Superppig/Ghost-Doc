using Services;
using Services.Event;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class EnemyBlackboard
{
    private IEventSystem eventSystem;
    private IEventSystem EventSystem
    {
        get
        {
            eventSystem ??= ServiceLocator.Get<IEventSystem>();
            return eventSystem;
        }
    }
    
    public IEnemyState current=IEnemyState.Idle;
    public float distanceToPlayer;
    public Vector3 dirToPlayer;
    public float currentHealth = 100f;


    public float commonHealth = 70f;//正常血槽
    public float weakHealth = 30f;//虚弱血槽
    public float damage = 30f;//攻击伤害
    
    public float commonHitRate=1f;//普通攻击倍率
    public float criticalStrikeRate = 2f;//暴击倍率
    
    
    public float speed = 0.1f;//移动速度
    public float trunTime = 0.5f;//转向时间
    
    public float hitTime = 0.5f;//受击间隔

    //Zombie
    public float directRange = 10f;//直接走向玩家范围范围
    public float attackRange = 2f;//攻击范围
    public float attackTime = 1f;//攻击间隔
    
    //RemoteEnemy
    public float MaxFireRange = 15f;//最大射程
    public float MinFireRange = 10f;//逃离范围
    public float fireTime = 1f;//攻击间隔
    public EnemyBullet bullet;//子弹
    
    //逻辑变量
    public bool isAttack;
    public bool hasAttack;
    public bool isHit;
    
    public Player player;
}