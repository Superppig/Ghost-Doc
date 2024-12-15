using System;
using Services;
using Services.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EStateType
{
    Walking =0,
    Jumping= 1,
    Sprinting=2,
    Crouching=3,
    Sliding=4,
    Air=5,
    WallRunning=6,
}
public class Player : MonoBehaviour
{
    public PlayerSettings settings;
    public PlayerBlackboard blackboard;

    public Transform cameraTransform;
    public VLineSummon vineLine;
    public Transform orientation;
    
    public PlayerCam playerCam;
    
    public Collider playerCollider;

    
    
    //新的状态机
    private FsmManager fsmManager;
    private Fsm<Player> fsm;

    
    private BuffSystem buffSystem;
    
    public Transform gunModel;
    public Transform gunTrans;
    public Rigidbody rb;

    private bool canClimb;
    
    private bool lastGrounded;

    private IAudioPlayer audioPlayer;
    
    
    private RaycastHit floorHit;

    private void Awake()
    {
        audioPlayer = ServiceLocator.Get<IAudioPlayer>();
        rb = GetComponentInChildren<Rigidbody>();
        
        //初始化状态机
        
        fsmManager = ServiceLocator.Get<FsmManager>();
        
        List<FsmState<Player>> states = new List<FsmState<Player>>()
        {
            new PlayerIdelState(this),
            new PlayerWalkingState(this),
            new PlayerJumpState(this),
            new PlayerSprintingState(this),
            new PlayerCrouchState(this),
            new PlayerSlideState(this),
            new PlayerAirState(this),
            new PlayerWallRunState(this),
        };

        fsm = fsmManager.CreateFsm(this, states.ToArray());
        fsm.Start<PlayerIdelState>();
        
        
        buffSystem = ServiceLocator.Get<BuffSystem>();
        buffSystem.AddPlayer(this);
        
        
        ServiceLocator.Get<ScreenControl>().PlayerRegist(this);
    }


    private void Start()
    {
        Reborn();
        fsm.ChangeState<PlayerIdelState>();
    }

    private void Update()
    {
        //检测
        blackboard.grounded = IsGrounded(0.1f);

        if (!blackboard.isWallJump)
        {
            WallCheck();
        }
        MyInput();
        fsm.OnCheck();
        fsm.OnUpdate();
        //一些状态的获取
        blackboard.velocity = rb.velocity;
        blackboard.speed = blackboard.velocity.magnitude;
    }

    private void FixedUpdate()
    {
        fsm.OnFixedUpdate();
        EnergeRecover();
    }

    public void Reborn()
    {
        blackboard.Health = blackboard.maxHealth;
        blackboard.Energy = blackboard.maxEnergy;
    }

    private void MyInput()
    {
        blackboard.dirInput =
            new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        //斜坡判定
        SlopJudgement();

        if(Input.GetKeyDown(settings.keySettings.jumpkey))
        {
            //先停止原有的协程,再开启新的协程
            StopCoroutine("JumpBuffer");
            StartCoroutine("JumpBuffer");
        }
    }

    private void SlopJudgement()
    {
        blackboard.moveDir = (blackboard.dirInput.x * orientation.right +
                                    blackboard.dirInput.z * orientation.forward).normalized;
        if (OnSlope())
        {
            blackboard.moveDir = Vector3.ProjectOnPlane(blackboard.moveDir, blackboard.slopeHit.normal).normalized;
        }
    }

    //检测是否在斜坡上
    private bool OnSlope()
    {
        //射线检测
        blackboard.isSlope = false;

        float angleStep = 360f/ settings.wallRunSettings.rayCount;
        for (int i = 0; i < settings.wallRunSettings.rayCount; i++)
        {
            Vector3 dir = Quaternion.Euler(0,angleStep * i, 0) * orientation.forward + Vector3.down;
            if (Physics.Raycast(transform.position, dir,
                    out blackboard.slopeHit, dir.magnitude,
                    settings.otherSettings.groundLayer))
            {
                float angle = Vector3.Angle(Vector3.up, blackboard.slopeHit.normal);
                blackboard.isSlope= angle < settings.otherSettings.maxSlopeAngle && angle != 0;
                return blackboard.isSlope;
            }
        }
        blackboard.isSlope= false;
        return false;
    }

    //检测人物是否离地面一定高度
    public bool IsGrounded(float height)
    {
        bool cur = Physics.Raycast(transform.position, Vector3.down, out floorHit,settings.airSettings.playerHeight * 0.5f + height,
            settings.otherSettings.groundLayer)||blackboard.isSlope;
        if (cur&&cur != lastGrounded)
        {
            audioPlayer.CreateAudioByGroup("Landing", transform.position, -1);
            blackboard.doubleJump = false;
        }
        lastGrounded = cur;
        return cur;
    }
    //返回地面
    public RaycastHit GetGround()
    {
        return floorHit;
    }

    //检测墙壁
    private void WallCheck()
    {
        //检测墙壁
        float angleStep = 360f/ settings.wallRunSettings.rayCount;
        for (int i = 0; i < settings.wallRunSettings.rayCount; i++)
        {
            Vector3 dir = Quaternion.Euler(0,angleStep * i, 0) * orientation.forward;
            blackboard.isWall = Physics.Raycast(transform.position, dir,
                out blackboard.wallHit, settings.wallRunSettings.wallCheckDistance,
                settings.otherSettings.wallLayer);
            if (!blackboard.isWall)
            {
                blackboard.isWall = Physics.Raycast(transform.position, dir,
                    out blackboard.wallHit, settings.wallRunSettings.wallCheckDistance,
                    settings.otherSettings.groundLayer);
            }
            if (blackboard.isWall)
            {
                break;
            }
        }
    }
    //气槽回复
    private void EnergeRecover()
    {
        UseEnerge(-100*Time.fixedDeltaTime);
        //爬墙能量回满
        if(blackboard.maxEnergy-blackboard.Energy<0.1f)
        {
            blackboard.hasClimbEnergyOut= false;
        }
    }
    //跳跃预输入
    IEnumerator JumpBuffer()
    {
        blackboard.isJumpBuffer = true;
        yield return new WaitForSeconds(settings.jumpSettings.jumpBufferTime);
        blackboard.isJumpBuffer = false;
    }


    public void TakeDamage(float damage)
    {
        blackboard.Health -= damage;
        blackboard.Health = Mathf.Clamp(blackboard.Health - damage, 0, blackboard.maxHealth);
    }

    public bool UseEnerge(float energy)
    {
        if(blackboard.Energy-energy<0)
        {
            return false;
        }
        blackboard.Energy = Mathf.Clamp(blackboard.Energy - energy, 0, blackboard.maxEnergy);
        return true;
    }
    public float GetHealth()
    {
        return blackboard.Health;
    }
    public float GetEnerge()
    {
        return blackboard.Energy;
    }
    public void TakeEnergeFailAudio()
    {
        
    }
    public Quaternion GetOriRotation()
    {
        return orientation.rotation;
    }
    public Vector3 GetSpeed()
    {
        return blackboard.velocity;
    }
    public void RecoverHealth(float health)
    {
        blackboard.Health = Mathf.Clamp(blackboard.Health + health, 0, blackboard.maxHealth);
    }
    
    //重力修正
    public void GravityFix()
    {
        if (blackboard.isSlope)
        {
            //给一个反向重力的力和沿着斜坡的力
            Vector3 dir = Vector3.down + blackboard.slopeHit.normal;
            rb.AddForce(dir * Physics.gravity.y, ForceMode.Force);
        }
    }
}