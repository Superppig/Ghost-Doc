using Services;
using Services.Event;
using System;
using UnityEngine;

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
    
    
    public float directRange = 10f;//直接走向玩家范围范围

    //逻辑变量
    public bool isAttack;
    public bool hasAttack;
    public bool isHit;
    
    public Player player;
    
    
    public LayerMask groundLayer;
    
    
    [Header("爆炸相关")]
    public float boomForce = 10f;
    public float boomDamage = 5f;
    public float boomRange = 4f;
    public ParticleSystem boom;
    
}