using Player_FSM;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerSettings settings;
    public PlayerBlackboard blackboard;

    public Transform cameraTransform;
    public VLineSummon vineLine;
    public Transform orientation;
    
    public PlayerCam playerCam;
    
    public Collider playerCollider;

    private FSM fsm;
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


    public Transform gunModel;
    public Transform gunTrans;
    public Rigidbody rb;

    private bool canClimb;

    private void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();
        fsm = new FSM(this);
        fsm.AddState(EStateType.Walking, new PlayerWalkingState(this));
        fsm.AddState(EStateType.Crouching, new PlayerCrouchState(this));
        fsm.AddState(EStateType.Jumping, new PlayerJumpState(this));
        fsm.AddState(EStateType.Sprinting, new PlayerSprintingState(this));
        fsm.AddState(EStateType.Sliding, new PlayerSlideState(this));
        fsm.AddState(EStateType.Air, new PlayerAirState(this));
        fsm.AddState(EStateType.WallRunning, new PlayerWallRunState(this));
    }


    private void Start()
    {
        Reborn();
        fsm.SwitchState(EStateType.Walking);
    }

    private void Update()
    {
        //检测
        blackboard.grounded = !blackboard.jumping && IsGrounded(0.1f);

        if (!blackboard.isWallJump)
        {
            WallCheck();
        }


        MyInput();
        fsm.OnCheck();
        fsm.OnUpdate();

        //一些状态的获取
        blackboard.currentState = fsm.current;
        blackboard.velocity = rb.velocity;
        blackboard.speed = blackboard.velocity.magnitude;
    }

    private void FixedUpdate()
    {
        fsm.OnFixUpdate();
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

        if (blackboard.isCombo)
        {
            //组合技则直接变为air状态并停止输入
            blackboard.lastState = blackboard.currentState;
            blackboard.nextState = EStateType.Air;
            fsm.SwitchState(EStateType.Air);
        }
        else
        {
            canClimb = !IsGrounded(settings.wallRunSettings.wallRunMinDisTance) &&
                       (blackboard.isWall
                           ? (Vector3.Angle(blackboard.wallHit.normal , blackboard.moveDir) > 90f&& blackboard.moveDir.magnitude>0.1f)
                           : false)
                       && !blackboard.hasClimbEnergyOut;
            
            //落地
            if ((blackboard.currentState == EStateType.Jumping || blackboard.currentState == EStateType.Air) &&
                blackboard.grounded)
            {
                blackboard.lastState = blackboard.currentState;
                blackboard.nextState = EStateType.Walking;
                fsm.SwitchState(EStateType.Walking);
            }

            //下蹲
            if (Input.GetKey(settings.keySettings.crouchKey)&& (!blackboard.isHoldingMelee||(blackboard.meleeState==Melee.WeaponState.Retracking&&!Input.GetMouseButton(1))))
            {
                if (CanSwitch(blackboard.currentState, EStateType.Crouching))
                {
                    blackboard.lastState = blackboard.currentState;
                    blackboard.nextState = EStateType.Crouching;
                    fsm.SwitchState(EStateType.Crouching);
                }
            }

            if (Input.GetKeyUp(settings.keySettings.crouchKey))
            {
                blackboard.lastState = blackboard.currentState;
                blackboard.nextState = EStateType.Walking;
                fsm.SwitchState(EStateType.Walking);
            }


            //空中
            if (!blackboard.grounded && blackboard.currentState != EStateType.Sprinting &&
                !canClimb)//不是墙跑状态
            {
                if (CanSwitch(blackboard.currentState, EStateType.Air))
                {
                    blackboard.lastState = blackboard.currentState;
                    blackboard.nextState = EStateType.Air;
                    fsm.SwitchState(EStateType.Air);
                }
            }


            //冲刺
            if (Input.GetKeyDown(settings.keySettings.sprintKey)&& (!blackboard.isHoldingMelee||(blackboard.meleeState==Melee.WeaponState.Retracking&&!Input.GetMouseButton(1))))
            {
                
                if (CanSwitch(blackboard.currentState, EStateType.Sprinting))
                {
                    if (blackboard.Energy > 100f)
                    {
                        UseEnerge(100);
                        Debug.Log("冲刺");
                        //开始冲刺
                        blackboard.lastState = blackboard.currentState;
                        blackboard.nextState = EStateType.Sprinting;
                        fsm.SwitchState(EStateType.Sprinting);
                        StartCoroutine(EndState(settings.sprintSettings.sprintDistance /
                                                settings.sprintSettings.sprintSpeed));
                        blackboard.lastState = EStateType.Sprinting;
                    }
                    else
                    {
                        TakeEnergeFailAudio();
                    }
                }
            }

            //滑行
            if (Input.GetKey(settings.keySettings.slideKey) && blackboard.grounded &&
                blackboard.dirInput.magnitude > 0 &&
                blackboard.speed > settings.slideSettings.startSlideSpeed)
            {
                if (CanSwitch(blackboard.currentState, EStateType.Sliding))
                {
                    blackboard.lastState = blackboard.currentState;
                    blackboard.nextState = EStateType.Sliding;
                    fsm.SwitchState(EStateType.Sliding);
                }
            }
            else if (blackboard.currentState == EStateType.Sliding)
            {
                if (CanSwitch(blackboard.currentState, blackboard.lastState))
                {
                    blackboard.nextState = blackboard.lastState;
                    fsm.SwitchState(blackboard.lastState);
                    blackboard.lastState = EStateType.Sliding;
                }
            }

            //跳跃
            if (Input.GetKey(settings.keySettings.jumpkey)&& (!blackboard.isHoldingMelee||(blackboard.meleeState==Melee.WeaponState.Retracking&&!Input.GetMouseButton(1))))
            {
                if (blackboard.grounded)
                {
                    if (CanSwitch(blackboard.currentState, EStateType.Jumping))
                    {
                        blackboard.lastState = blackboard.currentState;
                        StartCoroutine(StartJump(0.2f));
                        blackboard.nextState = EStateType.Jumping;
                        fsm.SwitchState(EStateType.Jumping);
                    }
                }
                else if (blackboard.currentState == EStateType.WallRunning)
                {
                    StartCoroutine(StartJump(0.2f));
                    // 墙跳逻辑
                    StartCoroutine(WallJumping(settings.jumpSettings.exitWallTime));
                }
            }

            //爬墙
            if (canClimb)
            {
                if (CanSwitch(blackboard.currentState, EStateType.WallRunning))
                {
                    blackboard.lastState = blackboard.currentState;
                    blackboard.nextState = EStateType.WallRunning;
                    
                    //获取爬墙方向
                    blackboard.climbXZDir = new Vector3(blackboard.velocity.x,0, blackboard.velocity.z).normalized;

                    fsm.SwitchState(EStateType.WallRunning);
                }
            }
        }
    }

    private bool CanSwitch(EStateType current, EStateType next)
    {
        return changeMatrix[(int)current, (int)next];
    }

    //协程结束某一状态,并返回last state
    IEnumerator EndState(float time)
    {
        yield return new WaitForSeconds(time);

        // 添加额外条件，检查当前状态是否为跳跃且仍然在空中
        if (blackboard.currentState == EStateType.Jumping && !blackboard.grounded)
        {
            // 如果仍然在空中，不要切回先前的状态
            yield break;
        }

        if (!blackboard.sprintingPause)
        {
            if (blackboard.currentState == EStateType.Sprinting)
            {
            
                fsm.SwitchState(EStateType.Walking);
            }
            else
            {
                fsm.SwitchState(blackboard.currentState);
            }
        }
        else
        {
            blackboard.sprintingPause= false;
        }
    }

    //墙跳中
    IEnumerator WallJumping(float time)
    {
        blackboard.isWallJump = true;
        blackboard.isWall = false;
        blackboard.lastState = blackboard.currentState;

        blackboard.nextState = EStateType.WallRunning;
        fsm.SwitchState(EStateType.Jumping);
        yield return new WaitForSeconds(time);
        blackboard.isWallJump = false;
    }

    IEnumerator StartJump(float time)
    {
        blackboard.jumping = true;
        yield return new WaitForSeconds(time);
        blackboard.jumping = false;
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
        if (Physics.Raycast(transform.position, Vector3.down, out blackboard.slopeHit,
                settings.airSettings.playerHeight * 0.5f + 0.5f))
        {
            float angle = Vector3.Angle(Vector3.up, blackboard.slopeHit.normal);
            return angle < settings.otherSettings.maxSlopeAngle && angle != 0;
        }

        return false;
    }

    //检测人物是否离地面一定高度
    public bool IsGrounded(float height)
    {
        return Physics.Raycast(transform.position, Vector3.down, settings.airSettings.playerHeight * 0.5f + height,
            settings.otherSettings.groundLayer);
    }
    //返回地面
    public RaycastHit GetGround()
    {
        RaycastHit hit = new RaycastHit();
        Physics.Raycast(transform.position, Vector3.down, out hit,
            settings.airSettings.playerHeight * 0.5f + 0.5f, settings.otherSettings.groundLayer);
        return hit;
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
            if (blackboard.isWall)
            {
                break;
            }
        }
    }
    //气槽回复
    private void EnergeRecover()
    {
        EStateType now = blackboard.currentState;
        if(now!=EStateType.Sliding&&now!=EStateType.WallRunning&&now!=EStateType.Sprinting)
            UseEnerge(-100*Time.fixedDeltaTime);
        
        //爬墙能量回满
        if(blackboard.maxEnergy-blackboard.Energy<0.1f)
        {
            blackboard.hasClimbEnergyOut= false;
        }
    }
    
    
    public void TakeDamage(float damage)
    {
        blackboard.Health -= damage;
        blackboard.Health = Mathf.Clamp(blackboard.Health - damage, 0, blackboard.maxHealth);
    }

    public void UseEnerge(float energy)
    {
        blackboard.Energy = Mathf.Clamp(blackboard.Energy - energy, 0, blackboard.maxEnergy);
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
}