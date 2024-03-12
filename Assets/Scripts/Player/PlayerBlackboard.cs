using Player_FSM;
using Services;
using Services.Event;
using System;
using UnityEngine;

[Serializable]
public class PlayerBlackboard
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

    public float maxHealth = 100f;
    [SerializeField]
    private float health;
    public float Health
    {
        get => health;
        set
        {
            if (value != health)
            {
                EventSystem.Invoke(EEvent.PlayerHPChange, value, maxHealth);
                health = value;
            }
        }
    }

    public float maxEnergy = 300f;
    [SerializeField]
    private float energy;
    public float Energy
    {
        get => energy;
        set
        {
            if (value != energy)
            {
                EventSystem.Invoke(EEvent.PlayerEnergyChange, value, maxEnergy);
                energy = value;
            }
        }
    }

    public bool grounded;
    public bool jumping;
    public bool isWallJump;

    public Vector3 dirInput;
    public Vector3 moveDir;
    public Vector3 velocity;        //继承速度
    public float speed;             //当前速率
    public float climbSpeed;        //爬墙继承速度

    public EStateType lastState;
    public EStateType currentState;
    public EStateType nextState;

    public RaycastHit wallLeftHit;
    public RaycastHit wallRightHit;
    public RaycastHit currentWall;
    public bool isRight;
    public bool isLeft;

    public bool isMeleeAttacking;
    public bool isBlocking;
    
    public bool hasClimbOverTime;//爬墙是否超时

    public bool isHoldingMelee;//是否持有近战武器
    public bool isCombo;//释放组合技


    public RaycastHit slopeHit; //斜坡检测
}
