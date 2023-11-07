using UnityEngine;
using Player_FSM;
using System;

[Serializable]
public class PlayerBlackboard : Blackboard
{
    [Header("玩家属性")] 
    public Vector3 moveDir;
    public Rigidbody m_rigidbody;
    [Header("移动")] 
    public float walkSpeed; //行走速度
    public float sprintSpeed; //冲刺速度
    public float slideSpeed; //滑行速度
    public float wallRunSpeed; //墙跑速度
    public float VerForce; //加速所需的力
    public float groundDrag; //地面阻力
    
    [Header("跳跃")] 
    public float jumpForce; //跳跃时给向上的瞬间力
    public float jumpCooldown; //跳跃冷却时间
    public float airMultiplier; //空气阻力系数

    [Header("下蹲")] 
    public float crouchSpeed; //蹲行速度
    public float crouchYScale; //下蹲时y缩放量
    
    [Header("按键")] 
    public KeyCode jumpkey = KeyCode.Space; //跳跃按键
    public KeyCode sprintKey = KeyCode.LeftShift; //冲刺按键
    public KeyCode crouchKey = KeyCode.C; //下蹲按键
    public KeyCode slideKey = KeyCode.LeftControl; //滑行按键
    
    [Header("着地检测")] 
    public float playerHeight; //玩家最低高度
    public LayerMask whatIsGround; //地面图层
    
    [Header("上坡")] 
    public float maxSlopeAngle; //最大坡度
    
    [Header("滑行")] 
    public float maxSlideTime; //最大滑行时间
    public float slideForce;
    public float slideYScale; //滑行时y缩放度
    
    [Header("贴墙跑")] 
    public float wallWalkForce; //贴墙跑受力
    public LayerMask whatIsWall; //wall的图层
    public float maxWallTime; //最大墙跑时间
    public float wallCheckDistance; //墙跑检测距离
    public float minJumpHeight; //最低高度

    [Header("墙跳")] 
    public float wallJumpUpForce; //墙跳向上力
    public float wallJumpSideForce; //墙跳向下力
    public float exitWallTime;
    
}
public class Player : MonoBehaviour
{
    private FSM fsm;
    public PlayerBlackboard playerBlackboard;
    private bool grounded;

    private void Awake()
    {
        playerBlackboard.m_rigidbody = GetComponent<Rigidbody>();
        fsm = new FSM(playerBlackboard);
        fsm.AddState(StateType.walking,new PlayerWalkingState(playerBlackboard));
        fsm.AddState(StateType.crouching,new PlayerCrouchState(playerBlackboard));
        fsm.AddState(StateType.jumping,new PlayerJumpState(playerBlackboard));

    }

    void Start()
    {
        fsm.SwitchState(StateType.walking);
    }

    void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerBlackboard.playerHeight * 0.5f + 0.2f, playerBlackboard.whatIsGround);
        MyInput();
        fsm.OnCheck();
        fsm.OnUpdate();
    }

    private void FixedUpdate()
    {
        fsm.OnFixUpdate();
    }

    private void MyInput()
    {
        playerBlackboard.moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (Input.GetKeyDown(playerBlackboard.crouchKey))
        {
            fsm.SwitchState(StateType.crouching);
        }
        if (Input.GetKeyUp(playerBlackboard.crouchKey))
        {
            fsm.SwitchState(StateType.walking);
        }
    }
}
