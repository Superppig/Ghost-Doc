using UnityEngine;
using Player_FSM;
using System;
using System.Collections;
using Cinemachine;
using UnityEngine.Serialization;

[Serializable]
public class PlayerBlackboard : Blackboard
{
    //太繁琐,待自定义editor
    [Header("游戏状态")] 
    public Vector3 dirInput;
    public Vector3 moveDir;
    public Rigidbody m_rigidbody;
    public Transform orientation;
    public Vector3 speed = new Vector3(0, 0, 0); //继承速度
    public float speedMag; //当前动量;
    public Transform camTrans;
    public Camera cam;
    public StateType last;
    public StateType current;
    public StateType next;
    [Space(10)] 
    [Header("行走")] public float walkSpeed; //行走速度

    public float accelerate; //加速度
    public float groundDrag; //地面阻力

    [Header("跳跃")] public float height; //跳跃高度
    public float airTransformAccelerate;

    [Header("下蹲")] public float crouchSpeed; //蹲行速度
    public float crouchYScale; //下蹲时y缩放量

    [Header("按键")] public KeyCode jumpkey = KeyCode.Space; //跳跃按键
    public KeyCode sprintKey = KeyCode.LeftShift; //冲刺按键
    public KeyCode crouchKey = KeyCode.C; //下蹲按键
    public KeyCode slideKey = KeyCode.LeftControl; //滑行按键

    [Header("着地检测")] public float playerHeight; //玩家最低高度
    public LayerMask whatIsGround; //地面图层

    [Header("上坡")] public float maxSlopeAngle; //最大坡度
    [Header("冲刺")] public float sprintSpeed; //冲刺速度
    public float sprintDistance; //冲刺距离
    public float sprintLeaveSpeed;//冲刺结束的速度
    
    public float sprintTime; //冲刺时间(无需暴露)

    [Header("滑行")] public float maxSlideTime; //最大滑行时间
    public float vineLineTime; //速度线时间
    public float slideYScale; //滑行时y缩放度
    public float slideAccelerate; //滑行减速的加速度
    public float startSlideSpeed = 1f; //滑行至少需要的速度

    [Header("贴墙跑")] 
    //墙跑速度改为继承
    public float wallRunSpeed; //墙跑速度
    public LayerMask whatIsWall; //wall的图层
    public float wallRunGRate = 0.1f; //滑墙重力倍率

    //public float maxWallTime; //最大墙跑时间
    public float wallCheckDistance; //墙跑检测距离
    public float wallRunMinDisTance;
    public RaycastHit wallLeftHit;
    public RaycastHit wallRightHit;

    public RaycastHit currentWall;

    public bool rightWall;
    public bool leftWall;

    [Header("墙跳")] 
    public float wallJumpSpeed;
    public bool isWallJump;
    public float exitWallTime;

    [Header("逻辑变量")] 
    public float sprintChangeRate; //冲刺动量转化速率
    public float walkToSlideCovoteTime; //walk状态转化为slide的动量继承土狼时间;
    public float slideToJumpHeightRate = 0.5f; //slide到jump状态,跳跃高度的变化率

    [Space(10)] [Header("武器")] public Transform gunModel;
    public Transform gunTrans;

    [Header("其他效果")] public VLineSummon vineLine;
}

public class Player : MonoBehaviour
{
    private FSM fsm;
    public PlayerBlackboard playerBlackboard;

    private bool grounded;
    private bool jumping;


    private RaycastHit slopeHit; //斜坡检测

    //private bool rightWall;
    //private bool leftWall;//墙壁检测

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

    private void Awake()
    {
        playerBlackboard.m_rigidbody = GetComponent<Rigidbody>();
        fsm = new FSM(playerBlackboard);
        fsm.AddState(StateType.walking, new PlayerWalkingState(playerBlackboard));
        fsm.AddState(StateType.crouching, new PlayerCrouchState(playerBlackboard));
        fsm.AddState(StateType.jumping, new PlayerJumpState(playerBlackboard));
        fsm.AddState(StateType.sprinting, new PlayerSprintingState(playerBlackboard));
        fsm.AddState(StateType.sliding, new PlayerSlideState(playerBlackboard));
        fsm.AddState(StateType.air, new PlayerAirState(playerBlackboard));
        fsm.AddState(StateType.wallRunning, new PlayerWallRunState(playerBlackboard));
    }


    void Start()
    {
        fsm.SwitchState(StateType.walking);
        playerBlackboard.sprintTime = playerBlackboard.sprintDistance / playerBlackboard.sprintSpeed;
    }

    void Update()
    {
        //检测
        grounded = jumping ? false : IsGrounded(0.1f);
        //Debug.Log(grounded);


        if (!playerBlackboard.isWallJump)
        {
            WallCheck();
        }


        MyInput();
        fsm.OnCheck();
        fsm.OnUpdate();

        //一些状态的获取
        playerBlackboard.current = fsm.current;
        playerBlackboard.speed = playerBlackboard.m_rigidbody.velocity;
        playerBlackboard.speedMag = playerBlackboard.speed.magnitude;
    }

    private void FixedUpdate()
    {
        fsm.OnFixUpdate();
    }

    private void MyInput()
    {
        playerBlackboard.dirInput =
            new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        //斜坡判定
        SlopJudgement();

        //落地
        if ((playerBlackboard.current == StateType.jumping || playerBlackboard.current == StateType.air) && grounded)
        {
            playerBlackboard.last = playerBlackboard.current;
            playerBlackboard.next = StateType.walking;
            fsm.SwitchState(StateType.walking);
        }

        //下蹲
        if (Input.GetKey(playerBlackboard.crouchKey))
        {
            if (CanSwitch(playerBlackboard.current, StateType.crouching))
            {
                playerBlackboard.last = playerBlackboard.current;
                playerBlackboard.next = StateType.crouching;
                fsm.SwitchState(StateType.crouching);
            }
        }

        if (Input.GetKeyUp(playerBlackboard.crouchKey))
        {
            playerBlackboard.last = playerBlackboard.current;
            playerBlackboard.next = StateType.walking;
            fsm.SwitchState(StateType.walking);
        }


        //空中
        if (!grounded && playerBlackboard.current != StateType.sprinting &&
            !(!IsGrounded(playerBlackboard.wallRunMinDisTance) &&
              (playerBlackboard.rightWall || playerBlackboard.leftWall)))//不是墙跑状态
        {
            if (CanSwitch(playerBlackboard.current, StateType.air))
            {
                playerBlackboard.last = playerBlackboard.current;
                playerBlackboard.next = StateType.air;
                fsm.SwitchState(StateType.air);
            }
        }


        //冲刺
        if (Input.GetKeyDown(playerBlackboard.sprintKey))
        {
            if (CanSwitch(playerBlackboard.current, StateType.sprinting))
            {
                playerBlackboard.last = playerBlackboard.current;
                playerBlackboard.next = StateType.sprinting;
                fsm.SwitchState(StateType.sprinting);
                StartCoroutine(EndState(playerBlackboard.sprintDistance / playerBlackboard.sprintSpeed));
                playerBlackboard.last = StateType.sprinting;
            }
        }

        //滑行
        if (Input.GetKey(playerBlackboard.slideKey) && grounded && playerBlackboard.dirInput.magnitude > 0 &&
            playerBlackboard.speedMag > playerBlackboard.startSlideSpeed)
        {
            if (CanSwitch(playerBlackboard.current, StateType.sliding))
            {
                playerBlackboard.last = playerBlackboard.current;
                playerBlackboard.next = StateType.sliding;
                fsm.SwitchState(StateType.sliding);
            }
        }
        else if(playerBlackboard.current==StateType.sliding)
        {
            if (CanSwitch(playerBlackboard.current, playerBlackboard.last))
            {
                playerBlackboard.next = playerBlackboard.last;
                fsm.SwitchState(playerBlackboard.last);
                playerBlackboard.last = StateType.sliding;
            }
        }

        //跳跃
        if (Input.GetKey(playerBlackboard.jumpkey))
        {
            if (grounded)
            {
                if (CanSwitch(playerBlackboard.current, StateType.jumping))
                {
                    playerBlackboard.last = playerBlackboard.current;
                    StartCoroutine(StartJump(0.2f));
                    playerBlackboard.next = StateType.jumping;
                    fsm.SwitchState(StateType.jumping);
                }
            }
            else if (playerBlackboard.current == StateType.wallRunning)
            {
                StartCoroutine(StartJump(0.2f));
                // 墙跳逻辑
                StartCoroutine(WallJumping(0.3f));
            }
        }

        //滑墙
        if (!IsGrounded(playerBlackboard.wallRunMinDisTance) &&
            (playerBlackboard.rightWall || playerBlackboard.leftWall))
        {
            if (CanSwitch(playerBlackboard.current, StateType.wallRunning))
            {
                playerBlackboard.last = playerBlackboard.current;
                playerBlackboard.next = StateType.wallRunning;
                fsm.SwitchState(StateType.wallRunning);
            }
        }
    }

    private bool CanSwitch(StateType current, StateType next)
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
        if (playerBlackboard.current == StateType.jumping && !grounded)
        {
            // 如果仍然在空中，不要切回先前的状态
            yield break;
        }

        if (playerBlackboard.current == StateType.sprinting)
        {
            fsm.SwitchState(StateType.walking);
        }
        else
        {
            fsm.SwitchState(playerBlackboard.current);
        }
    }

    //墙跳中
    IEnumerator WallJumping(float time)
    {
        playerBlackboard.isWallJump = true;
        playerBlackboard.leftWall = false;
        playerBlackboard.rightWall = false;
        playerBlackboard.last = playerBlackboard.current;

        playerBlackboard.next = StateType.wallRunning;
        fsm.SwitchState(StateType.jumping);
        yield return new WaitForSeconds(time);
        playerBlackboard.isWallJump = false;
    }

    IEnumerator StartJump(float time)
    {
        jumping = true;
        yield return new WaitForSeconds(time);
        jumping = false;
    }

    private void SlopJudgement()
    {
        playerBlackboard.moveDir = (playerBlackboard.dirInput.x * playerBlackboard.orientation.right +
                                    playerBlackboard.dirInput.z * playerBlackboard.orientation.forward).normalized;
        if (OnSlope())
        {
            playerBlackboard.moveDir = Vector3.ProjectOnPlane(playerBlackboard.moveDir, slopeHit.normal).normalized;
        }
    }

    //检测是否在斜坡上
    private bool OnSlope()
    {
        //射线检测
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit,
                playerBlackboard.playerHeight * 0.5f + 0.5f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < playerBlackboard.maxSlopeAngle && angle != 0;
        }

        return false;
    }

    //检测人物是否离地面一定高度
    private bool IsGrounded(float height)
    {
        return Physics.Raycast(transform.position, Vector3.down, playerBlackboard.playerHeight * 0.5f + height,
            playerBlackboard.whatIsGround);
    }

    //检测墙壁
    private void WallCheck()
    {
        //新增墙壁检测
        playerBlackboard.rightWall = Physics.Raycast(transform.position, playerBlackboard.orientation.right,
            out playerBlackboard.wallRightHit, playerBlackboard.wallCheckDistance,
            playerBlackboard.whatIsWall);
        if (!playerBlackboard.rightWall)
        {
            Vector3 dir = (playerBlackboard.orientation.right+playerBlackboard.orientation.forward).normalized;
            playerBlackboard.rightWall = Physics.Raycast(transform.position, dir,
                out playerBlackboard.wallRightHit, playerBlackboard.wallCheckDistance,
                playerBlackboard.whatIsWall);
        }
        else if(!playerBlackboard.rightWall)
        {
            Vector3 dir = (playerBlackboard.orientation.right-playerBlackboard.orientation.forward).normalized;
            playerBlackboard.rightWall = Physics.Raycast(transform.position, dir,
                out playerBlackboard.wallRightHit, playerBlackboard.wallCheckDistance,
                playerBlackboard.whatIsWall);
        }
        
        
        
        playerBlackboard.leftWall = Physics.Raycast(transform.position, -playerBlackboard.orientation.right,
            out playerBlackboard.wallLeftHit, playerBlackboard.wallCheckDistance,
            playerBlackboard.whatIsWall);
        if (!playerBlackboard.leftWall)
        {
            Vector3 dir = (-playerBlackboard.orientation.right+playerBlackboard.orientation.forward).normalized;
            playerBlackboard.leftWall = Physics.Raycast(transform.position, dir,
                out playerBlackboard.wallLeftHit, playerBlackboard.wallCheckDistance,
                playerBlackboard.whatIsWall);
        }
        else if(!playerBlackboard.leftWall)
        {
            Vector3 dir = (-playerBlackboard.orientation.right-playerBlackboard.orientation.forward).normalized;
            playerBlackboard.leftWall = Physics.Raycast(transform.position, dir,
                out playerBlackboard.wallLeftHit, playerBlackboard.wallCheckDistance,
                playerBlackboard.whatIsWall);
        }

        if (playerBlackboard.rightWall)
            playerBlackboard.currentWall = playerBlackboard.wallRightHit;
        if (playerBlackboard.leftWall)
            playerBlackboard.currentWall = playerBlackboard.wallLeftHit;
    }
}