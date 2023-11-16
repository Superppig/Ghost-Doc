using UnityEngine;
using Player_FSM;
using System;
using System.Collections;

[Serializable]
public class PlayerBlackboard : Blackboard
{
    [Header("玩家属性")] public Vector3 moveDir;
    public Rigidbody m_rigidbody;
    public Transform oritation;
    public Vector3 speed = new Vector3(0, 0, 0); //继承速度
    [Header("移动")] public float walkSpeed; //行走速度
    public float sprintSpeed; //冲刺速度
    public float sprintDistance; //冲刺距离
    public float slideSpeed; //滑行速度
    public float wallRunSpeed; //墙跑速度
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

    [Header("滑行")] public float maxSlideTime; //最大滑行时间
    public float slideYScale; //滑行时y缩放度

    [Header("贴墙跑")] public LayerMask whatIsWall; //wall的图层

    public float wallRunGRate = 0.1f; //滑墙重力倍率

    //public float maxWallTime; //最大墙跑时间
    public float wallCheckDistance; //墙跑检测距离
    public float wallRunMinDisTance;
    public RaycastHit wallLeftHit;
    public RaycastHit wallRightHit;

    public RaycastHit currentWall;

    public bool rightWall;
    public bool leftWall;

    [Header("墙跳")] public float wallJumpSpeed;
    public bool isWallJump;
    public float exitWallTime;
}

public class Player : MonoBehaviour
{
    private FSM fsm;
    public PlayerBlackboard playerBlackboard;
    private bool grounded;


    public StateType current;
    private StateType last;

    private RaycastHit slopeHit; //斜坡检测

    //private bool rightWall;
    //private bool leftWall;//墙壁检测

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
    }

    void Update()
    {
        //检测
        grounded = IsGrounded(0.2f);
        //Debug.Log(grounded);


        if (!playerBlackboard.isWallJump)
        {
            WallCheck();
        }


        MyInput();
        fsm.OnCheck();
        fsm.OnUpdate();
        //调试
        current = fsm.current;
        playerBlackboard.speed = playerBlackboard.m_rigidbody.velocity;
    }

    private void FixedUpdate()
    {
        fsm.OnFixUpdate();
    }

    private void MyInput()
    {
        playerBlackboard.moveDir =
            new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        //下蹲
        if (Input.GetKeyDown(playerBlackboard.crouchKey))
        {
            if (CanSwitch(current, StateType.crouching))
            {
                last = current;
                fsm.SwitchState(StateType.crouching);
            }
        }

        if (Input.GetKeyUp(playerBlackboard.crouchKey))
        {
            last = current;
            fsm.SwitchState(StateType.walking);
        }

        //跳跃
        if (Input.GetKeyDown(playerBlackboard.jumpkey))
        {
            if (grounded)
            {
                if (CanSwitch(current, StateType.jumping))
                {
                    last = current;
                    fsm.SwitchState(StateType.jumping);
                }
            }
            else if (current == StateType.wallRunning)
            {
                // 墙跳逻辑
                StartCoroutine(WallJumping(0.3f));
            }
        }

        //空中
        if (!grounded && current != StateType.sprinting &&
                 !(playerBlackboard.rightWall || playerBlackboard.leftWall))
        {
            if (CanSwitch(current, StateType.air))
            {
                last = current;
                fsm.SwitchState(StateType.air);
            }
        }

        //落地
        if ((current == StateType.jumping || current == StateType.air) && grounded)
        {
            last = current;
            fsm.SwitchState(StateType.walking);
        }

        //冲刺
        if (Input.GetKeyDown(playerBlackboard.sprintKey))
        {
            if (CanSwitch(current, StateType.sprinting))
            {
                last = current;
                fsm.SwitchState(StateType.sprinting);
                StartCoroutine(EndState(last, playerBlackboard.sprintDistance / playerBlackboard.sprintSpeed));
                last = StateType.sprinting;
            }
        }

        //滑行
        if (Input.GetKeyDown(playerBlackboard.slideKey) && grounded)
        {
            if (CanSwitch(current, StateType.sliding))
            {
                last = current;
                fsm.SwitchState(StateType.sliding);
            }
        }

        if (Input.GetKeyUp(playerBlackboard.slideKey))
        {
            if (CanSwitch(current, last))
            {
                fsm.SwitchState(last);
                last = StateType.sliding;
            }
        }

        //滑墙
        if (!IsGrounded(playerBlackboard.wallRunMinDisTance) &&
                 (playerBlackboard.rightWall || playerBlackboard.leftWall))
        {
            if (CanSwitch(current, StateType.wallRunning))
            {
                last = current;
                fsm.SwitchState(StateType.wallRunning);
            }
        }

        SlopJudgement();
    }

    //状态机逻辑
    private bool CanSwitch(StateType current, StateType next)
    {
        if (current == StateType.walking)
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
            if (next == StateType.walking || next == StateType.air || next == StateType.sliding)
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

        return false;
    }

    //协程结束某一状态
    IEnumerator EndState(StateType stateType, float time)
    {
        yield return new WaitForSeconds(time);

        // 添加额外条件，检查当前状态是否为跳跃且仍然在空中
        if (stateType == StateType.jumping && !grounded)
        {
            // 如果仍然在空中，不要切回先前的状态
            yield break;
        }

        fsm.SwitchState(stateType);
    }

    //墙跳中
    IEnumerator WallJumping(float time)
    {
        playerBlackboard.isWallJump = true;
        playerBlackboard.leftWall = false;
        playerBlackboard.rightWall = false;
        last = current;
        fsm.SwitchState(StateType.jumping);
        yield return new WaitForSeconds(time);
        playerBlackboard.isWallJump = false;
    }

    private void SlopJudgement()
    {
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
        playerBlackboard.rightWall = Physics.Raycast(transform.position, playerBlackboard.oritation.right,
            out playerBlackboard.wallRightHit, playerBlackboard.wallCheckDistance,
            playerBlackboard.whatIsWall);
        playerBlackboard.leftWall = Physics.Raycast(transform.position, -playerBlackboard.oritation.right,
            out playerBlackboard.wallLeftHit, playerBlackboard.wallCheckDistance,
            playerBlackboard.whatIsWall);

        if (playerBlackboard.rightWall)
            playerBlackboard.currentWall = playerBlackboard.wallRightHit;
        if (playerBlackboard.leftWall)
            playerBlackboard.currentWall = playerBlackboard.wallLeftHit;
    }
}