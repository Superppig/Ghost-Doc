using UnityEngine;
using Player_FSM;
using System;
using System.Collections;

[Serializable]
public class PlayerBlackboard : Blackboard
{
    public WalkSettings walkSettings;
    public JumpSettings jumpSettings;
    public SprintSettings sprintSettings;
    public CrouchSettings crouchSettings;
    public SlidingSettings slidingSettings;
    public AirSettings airSettings;
    public WallRunningSettings wallRunningSettings;
    public KeySettings keySettings;
    public OtherSettings otherSettings;
}

public class Player : MonoBehaviour,IPlayer
{
    private FSM fsm;
    public PlayerBlackboard playerBlackboard;
    private bool grounded;
    private bool jumping;
    private RaycastHit slopeHit; //斜坡检测
    public bool[,] changeMatrix =
    {
        { false, true, true, true, true, true, false },
        { false, true, false, false, false, true, false },
        { true, true, false, false, false, true, true },
        { true, false, false, false, false, false, false },
        { true, true, true, false, false, false, false },
        { true, false, true, false, false, false, true },
        { true, true, false, false, false, true, false }
    }; //状态机转移邻接矩阵


    public float health=100f;
    public float maxHealth=100f;
    public float  energe = 300;
    public float  maxEnerge = 300;



    private void Awake()
    {
        playerBlackboard.otherSettings.m_rigidbody = GetComponent<Rigidbody>();
        fsm = new FSM(playerBlackboard);
        fsm.AddState(EStateType.Walking, new PlayerWalkingState(playerBlackboard));
        fsm.AddState(EStateType.Crouching, new PlayerCrouchState(playerBlackboard));
        fsm.AddState(EStateType.Jumping, new PlayerJumpState(playerBlackboard));
        fsm.AddState(EStateType.Sprinting, new PlayerSprintingState(playerBlackboard));
        fsm.AddState(EStateType.Sliding, new PlayerSlideState(playerBlackboard));
        fsm.AddState(EStateType.Air, new PlayerAirState(playerBlackboard));
        fsm.AddState(EStateType.WallRunning, new PlayerWallRunState(playerBlackboard));
    }


    void Start()
    {
        fsm.SwitchState(EStateType.Walking);
        playerBlackboard.sprintSettings.sprintTime = playerBlackboard.sprintSettings.sprintDistance / playerBlackboard.sprintSettings.sprintSpeed;
    }

    void Update()
    {
        //检测
        grounded = jumping ? false : IsGrounded(0.1f);
        //Debug.Log(grounded);


        if (!playerBlackboard.jumpSettings.isWallJump)
        {
            WallCheck();
        }


        MyInput();
        fsm.OnCheck();
        fsm.OnUpdate();

        //一些状态的获取
        playerBlackboard.otherSettings.current = fsm.current;
        playerBlackboard.otherSettings.speed = playerBlackboard.otherSettings.m_rigidbody.velocity;
        playerBlackboard.otherSettings.speedMag = playerBlackboard.otherSettings.speed.magnitude;
    }

    private void FixedUpdate()
    {
        fsm.OnFixUpdate();
        EnergeRecover();
    }

    private void MyInput()
    {
        playerBlackboard.otherSettings.dirInput =
            new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        //斜坡判定
        SlopJudgement();

        //落地
        if ((playerBlackboard.otherSettings.current == EStateType.Jumping || playerBlackboard.otherSettings.current == EStateType.Air) && grounded)
        {
            playerBlackboard.otherSettings.last = playerBlackboard.otherSettings.current;
            playerBlackboard.otherSettings.next = EStateType.Walking;
            fsm.SwitchState(EStateType.Walking);
        }

        //下蹲
        if (Input.GetKey(playerBlackboard.keySettings.crouchKey))
        {
            if (CanSwitch(playerBlackboard.otherSettings.current, EStateType.Crouching))
            {
                playerBlackboard.otherSettings.last = playerBlackboard.otherSettings.current;
                playerBlackboard.otherSettings.next = EStateType.Crouching;
                fsm.SwitchState(EStateType.Crouching);
            }
        }

        if (Input.GetKeyUp(playerBlackboard.keySettings.crouchKey))
        {
            playerBlackboard.otherSettings.last = playerBlackboard.otherSettings.current;
            playerBlackboard.otherSettings.next = EStateType.Walking;
            fsm.SwitchState(EStateType.Walking);
        }


        //空中
        if (!grounded && playerBlackboard.otherSettings.current != EStateType.Sprinting &&
            !(!IsGrounded(playerBlackboard.wallRunningSettings.wallRunMinDisTance) &&
              (playerBlackboard.wallRunningSettings.rightWall || playerBlackboard.wallRunningSettings.leftWall)))//不是墙跑状态
        {
            if (CanSwitch(playerBlackboard.otherSettings.current, EStateType.Air))
            {
                playerBlackboard.otherSettings.last = playerBlackboard.otherSettings.current;
                playerBlackboard.otherSettings.next = EStateType.Air;
                fsm.SwitchState(EStateType.Air);
            }
        }


        //冲刺
        if (Input.GetKeyDown(playerBlackboard.keySettings.sprintKey))
        {
            if (CanSwitch(playerBlackboard.otherSettings.current, EStateType.Sprinting))
            {
                if(energe>100f)
                {
                    TakeEnerge(100);
                    //开始冲刺
                    playerBlackboard.otherSettings.last = playerBlackboard.otherSettings.current;
                    playerBlackboard.otherSettings.next = EStateType.Sprinting;
                    fsm.SwitchState(EStateType.Sprinting);
                    StartCoroutine(EndState(playerBlackboard.sprintSettings.sprintDistance /
                                            playerBlackboard.sprintSettings.sprintSpeed));
                    playerBlackboard.otherSettings.last = EStateType.Sprinting;
                }
                else
                {
                    TakeEnergeFailAudio();
                }
            }
        }

        //滑行
        if (Input.GetKey(playerBlackboard.keySettings.slideKey) && grounded && playerBlackboard.otherSettings.dirInput.magnitude > 0 &&
            playerBlackboard.otherSettings.speedMag > playerBlackboard.slidingSettings.startSlideSpeed)
        {
            if (CanSwitch(playerBlackboard.otherSettings.current, EStateType.Sliding))
            {
                playerBlackboard.otherSettings.last = playerBlackboard.otherSettings.current;
                playerBlackboard.otherSettings.next = EStateType.Sliding;
                fsm.SwitchState(EStateType.Sliding);
            }
        }
        else if(playerBlackboard.otherSettings.current==EStateType.Sliding)
        {
            if (CanSwitch(playerBlackboard.otherSettings.current, playerBlackboard.otherSettings.last))
            {
                playerBlackboard.otherSettings.next = playerBlackboard.otherSettings.last;
                fsm.SwitchState(playerBlackboard.otherSettings.last);
                playerBlackboard.otherSettings.last = EStateType.Sliding;
            }
        }

        //跳跃
        if (Input.GetKey(playerBlackboard.keySettings.jumpkey))
        {
            if (grounded)
            {
                if (CanSwitch(playerBlackboard.otherSettings.current, EStateType.Jumping))
                {
                    playerBlackboard.otherSettings.last = playerBlackboard.otherSettings.current;
                    StartCoroutine(StartJump(0.2f));
                    playerBlackboard.otherSettings.next = EStateType.Jumping;
                    fsm.SwitchState(EStateType.Jumping);
                }
            }
            else if (playerBlackboard.otherSettings.current == EStateType.WallRunning)
            {
                StartCoroutine(StartJump(0.2f));
                // 墙跳逻辑
                StartCoroutine(WallJumping(0.3f));
            }
        }

        //滑墙
        if (!IsGrounded(playerBlackboard.wallRunningSettings.wallRunMinDisTance) &&
            (playerBlackboard.wallRunningSettings.rightWall || playerBlackboard.wallRunningSettings.leftWall))
        {
            if (CanSwitch(playerBlackboard.otherSettings.current, EStateType.WallRunning))
            {
                playerBlackboard.otherSettings.last = playerBlackboard.otherSettings.current;
                playerBlackboard.otherSettings.next = EStateType.WallRunning;
                fsm.SwitchState(EStateType.WallRunning);
            }
        }
    }

    private bool CanSwitch(EStateType current, EStateType next)
    {
        /*if (current == StateType.walking)
        {
            if (next == StateType.crouching || next == StateType.sprinting || next == StateType.jumping ||
                next == StateType.sliding || next == StateType.air)
                return true;
        }

        else if (current == StateType.jumping)
        {
            if (next == StateType.air || next == StateType.jumping)
                return true;
        }

        else if (current == StateType.sprinting)
        {
            if (next == StateType.walking || next == StateType.air || next == StateType.sliding||next== StateType.jumping)
                return true;
        }

        else if (current == StateType.crouching)
        {
            if (next == StateType.walking)
                return true;
        }

        else if (current == StateType.sliding)
        {
            if (next == StateType.walking || next == StateType.sprinting)
                return true;
        }

        else if (current == StateType.air)
        {
            if (next == StateType.walking || next == StateType.wallRunning || next == StateType.sprinting)
                return true;
        }

        else if (current == StateType.wallRunning)
        {
            if (next == StateType.air || next == StateType.jumping || next == StateType.walking)
                return true;
        }

        return false;*/
        return changeMatrix[(int)current, (int)next];
    }

    //协程结束某一状态,并返回last state
    IEnumerator EndState(float time)
    {
        yield return new WaitForSeconds(time);

        // 添加额外条件，检查当前状态是否为跳跃且仍然在空中
        if (playerBlackboard.otherSettings.current == EStateType.Jumping && !grounded)
        {
            // 如果仍然在空中，不要切回先前的状态
            yield break;
        }

        if (playerBlackboard.otherSettings.current == EStateType.Sprinting)
        {
            fsm.SwitchState(EStateType.Walking);
        }
        else
        {
            fsm.SwitchState(playerBlackboard.otherSettings.current);
        }
    }

    //墙跳中
    IEnumerator WallJumping(float time)
    {
        playerBlackboard.jumpSettings.isWallJump = true;
        playerBlackboard.wallRunningSettings.leftWall = false;
        playerBlackboard.wallRunningSettings.rightWall = false;
        playerBlackboard.otherSettings.last = playerBlackboard.otherSettings.current;

        playerBlackboard.otherSettings.next = EStateType.WallRunning;
        fsm.SwitchState(EStateType.Jumping);
        yield return new WaitForSeconds(time);
        playerBlackboard.jumpSettings.isWallJump = false;
    }

    IEnumerator StartJump(float time)
    {
        jumping = true;
        yield return new WaitForSeconds(time);
        jumping = false;
    }

    private void SlopJudgement()
    {
        playerBlackboard.otherSettings.moveDir = (playerBlackboard.otherSettings.dirInput.x * playerBlackboard.otherSettings.orientation.right +
                                    playerBlackboard.otherSettings.dirInput.z * playerBlackboard.otherSettings.orientation.forward).normalized;
        if (OnSlope())
        {
            playerBlackboard.otherSettings.moveDir = Vector3.ProjectOnPlane(playerBlackboard.otherSettings.moveDir, slopeHit.normal).normalized;
        }
    }

    //检测是否在斜坡上
    private bool OnSlope()
    {
        //射线检测
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit,
                playerBlackboard.otherSettings.playerHeight * 0.5f + 0.5f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < playerBlackboard.otherSettings.maxSlopeAngle && angle != 0;
        }

        return false;
    }

    //检测人物是否离地面一定高度
    private bool IsGrounded(float height)
    {
        return Physics.Raycast(transform.position, Vector3.down, playerBlackboard.otherSettings.playerHeight * 0.5f + height,
            playerBlackboard.otherSettings.whatIsGround);
    }

    //检测墙壁
    private void WallCheck()
    {
        //新增墙壁检测
        playerBlackboard.wallRunningSettings.rightWall = Physics.Raycast(transform.position, playerBlackboard.otherSettings.orientation.right,
            out playerBlackboard.wallRunningSettings.wallRightHit, playerBlackboard.wallRunningSettings.wallCheckDistance,
            playerBlackboard.wallRunningSettings.whatIsWall);
        if (!playerBlackboard.wallRunningSettings.rightWall)
        {
            Vector3 dir = (playerBlackboard.otherSettings.orientation.right+playerBlackboard.otherSettings.orientation.forward).normalized;
            playerBlackboard.wallRunningSettings.rightWall = Physics.Raycast(transform.position, dir,
                out playerBlackboard.wallRunningSettings.wallRightHit, playerBlackboard.wallRunningSettings.wallCheckDistance,
                playerBlackboard.wallRunningSettings.whatIsWall);
        }
        else if(!playerBlackboard.wallRunningSettings.rightWall)
        {
            Vector3 dir = (playerBlackboard.otherSettings.orientation.right-playerBlackboard.otherSettings.orientation.forward).normalized;
            playerBlackboard.wallRunningSettings.rightWall = Physics.Raycast(transform.position, dir,
                out playerBlackboard.wallRunningSettings.wallRightHit, playerBlackboard.wallRunningSettings.wallCheckDistance,
                playerBlackboard.wallRunningSettings.whatIsWall);
        }
        playerBlackboard.wallRunningSettings.leftWall = Physics.Raycast(transform.position, -playerBlackboard.otherSettings.orientation.right,
            out playerBlackboard.wallRunningSettings.wallLeftHit, playerBlackboard.wallRunningSettings.wallCheckDistance,
            playerBlackboard.wallRunningSettings.whatIsWall);
        if (!playerBlackboard.wallRunningSettings.leftWall)
        {
            Vector3 dir = (-playerBlackboard.otherSettings.orientation.right+playerBlackboard.otherSettings.orientation.forward).normalized;
            playerBlackboard.wallRunningSettings.leftWall = Physics.Raycast(transform.position, dir,
                out playerBlackboard.wallRunningSettings.wallLeftHit, playerBlackboard.wallRunningSettings.wallCheckDistance,
                playerBlackboard.wallRunningSettings.whatIsWall);
        }
        else if(!playerBlackboard.wallRunningSettings.leftWall)
        {
            Vector3 dir = (-playerBlackboard.otherSettings.orientation.right-playerBlackboard.otherSettings.orientation.forward).normalized;
            playerBlackboard.wallRunningSettings.leftWall = Physics.Raycast(transform.position, dir,
                out playerBlackboard.wallRunningSettings.wallLeftHit, playerBlackboard.wallRunningSettings.wallCheckDistance,
                playerBlackboard.wallRunningSettings.whatIsWall);
        }

        if (playerBlackboard.wallRunningSettings.rightWall)
            playerBlackboard.wallRunningSettings.currentWall = playerBlackboard.wallRunningSettings.wallRightHit;
        if (playerBlackboard.wallRunningSettings.leftWall)
            playerBlackboard.wallRunningSettings.currentWall = playerBlackboard.wallRunningSettings.wallLeftHit;
    }
    //气槽回复
    private void EnergeRecover()
    {
        EStateType now = playerBlackboard.otherSettings.current;
        if(now!=EStateType.Sliding&&now!=EStateType.WallRunning&&now!=EStateType.Sprinting)
            TakeEnerge(-100*Time.fixedDeltaTime);
    }
    
    
    //IPlayer接口实现
    public void TakeDamage(float damage)
    {
        health -= damage;
        health = health < 0 ? 0 : health;
        health = health > maxHealth ? maxHealth : health;
    }

    public void TakeEnerge(float energe)
    {
        this.energe -= energe;
        this.energe = this.energe < 0 ? 0 : this.energe;
        this.energe = this.energe > maxEnerge ? maxEnerge : this.energe;
    }
    public float GetHealth()
    {
        return health;
    }
    public float GetEnerge()
    {
        return energe;
    }
    public void TakeEnergeFailAudio()
    {
        
    }
    public Quaternion GetOriRotation()
    {
        return playerBlackboard.otherSettings.orientation.rotation;
    }

    public Vector3 GetSpeed()
    {
        return playerBlackboard.otherSettings.speed;
    }
}