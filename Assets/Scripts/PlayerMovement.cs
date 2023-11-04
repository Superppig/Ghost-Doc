using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("移动")]
    private float moveSpeed;//当前最大速度
    public float walkSpeed;//行走速度
    public float sprintSpeed;//冲刺速度
    public float slideSpeed;//滑行速度
    public float wallRunSpeed;//墙跑速度
    public float VerForce;//加速所需的力
    
    //用于平缓变化速度
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    
    public float groundDrag;//地面阻力
    
    [Header("跳跃")]
    public float jumpForce;//跳跃时给向上的瞬间力
    public float jumpCooldown;//跳跃冷却时间
    public float airMultiplier;//空气阻力系数
    private bool readyToJump=true;

    [Header("下蹲")] 
    public float crouchSpeed;//蹲行速度
    public float crouchYScale;//下蹲时y缩放量
    private float startYScale;//初始时y缩放量

    [Header("按键")] 
    public KeyCode jumpkey = KeyCode.Space;//跳跃按键
    public KeyCode sprintKey = KeyCode.LeftShift;//冲刺按键
    public KeyCode crouchKey = KeyCode.C;//下蹲按键
    public KeyCode slideKey = KeyCode.LeftControl;//滑行按键
    
    [Header("着地检测")] 
    public float playerHeight;//玩家最低高度
    public LayerMask whatIsGround;//地面图层
    private bool grounded;//是否在地面

    [Header("上坡")] 
    public float maxSlopeAngle;//最大坡度
    private RaycastHit slopeHit;//检测射线
    private bool exitingSlope;//是否在坡上
    [Header("滑行")]
    public float maxSlideTime;//最大滑行时间
    public float slideForce;
    private float slideTimer;//滑行计时器
    public float slideYScale;//滑行时y缩放度
    [Header("贴墙跑")] 
    public float wallWalkForce;//贴墙跑受力
    public LayerMask whatIsWall;//wall的图层
    public float maxWallTime;//最大墙跑时间
    private float wallTimer;//墙跑计数器
    public float wallCheckDistance;//墙跑检测距离
    public float minJumpHeight;//最低高度
    private RaycastHit wallLeftHit;
    private RaycastHit wallRightHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("墙跳")] 
    public float wallJumpUpForce;//墙跳向上力
    public float wallJumpSideForce;//墙跳向下力
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;
    
    
    [Header("bool")]
    public bool wallrunning;
    public bool sliding;//是否在滑行



    public Transform orientation;//方向子物体的transform

    private float horizontalInput;//水平输入
    private float verticalInput;//垂直输入

    private Vector3 moveDirection;//移动方向
    private Rigidbody rb;
    public MovementState state;
    public PlayerCam cam;
    
    //运动状态
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        wallRunning,
        air
    }

    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    public TextMeshProUGUI text3;


    void Start()
    {
        //初始化最初速度
        desiredMoveSpeed = walkSpeed;
        moveSpeed = walkSpeed;
        lastDesiredMoveSpeed = desiredMoveSpeed;
        
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        //以下来自unity文档:如果启用了 freezeRotation，则物理模拟不会修改旋转。 这对于创建第一人称射击游戏非常有用， 因为玩家需要使用鼠标完全控制旋转。
        startYScale = transform.localScale.y;

    }
    private void FixedUpdate()
    {
        MovePlayer();
        if(wallrunning)
        {
            WallRunMovement();
        }

        if (sliding)
        {
            SlideMovement();
        }
    }
    void Update()
    {
        //射线检测是否位于地面
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        MyInput();
        SpeedControl();
        StateHandler();
        
        //在地面时施加阻力
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0f;
        }
        
        //检测墙
        CheckForWall();
        //输出速度值
        text1.text = "ver:" + rb.velocity.magnitude;
        text2.text = "mode:" +state;
        text3.text = "Sliding:" + sliding;

    }
    //输入
    void MyInput()
    {
        //移动输入
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        //其他键输入

        //跳跃
        if (Input.GetKey(jumpkey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            StartCoroutine(FinishJump());
        }
        //下蹲
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down*5f,ForceMode.Impulse);//向下做一个力,使人物贴地
        }
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
        //滑行
        if (Input.GetKeyDown(slideKey)&&(horizontalInput!=0||verticalInput!=0))
        {
            StartSlide();
        }
        if (Input.GetKeyUp(slideKey) && sliding)
        {
            StopSlide();
        }
        //墙跳
        if ((wallLeft||wallRight)&& moveDirection.magnitude>0f&& AboveGround()&& !exitingWall)
        {
            if(!wallrunning)
                StartWallRun();
            if (wallTimer>0)
            {
                wallTimer -= Time.deltaTime;
            }

            if (wallTimer <= 0 && wallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }
            
            //墙跳
            if(Input.GetKeyDown(jumpkey))
                WallJump();
        }
        else if (exitingWall)
        {
            if(wallrunning)
                StopWallRun();
            if (exitWallTimer>0)
            {
                exitWallTimer -= Time.deltaTime;
            }

            if (exitWallTimer<=0)
            {
                exitingWall = false;
            }
        }
        else
        {
            if(wallrunning)
                StopWallRun();
        }
    }
    IEnumerator FinishJump()
    {
        yield return new WaitForSeconds(jumpCooldown);
        ResetJump();
    }
    
    //状态转换
    private void StateHandler()
    {
        if (wallrunning)
        {
            state = MovementState.wallRunning;
            desiredMoveSpeed = wallRunSpeed;
        }
        
        if (sliding)
        {
            state = MovementState.sliding;
            if (OnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;
            else
                desiredMoveSpeed = sprintSpeed;
        }
        //mode crouching
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        //mode Sprinting
        else if (grounded&&Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        //mode walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        //mode aie
        else
        {
            state = MovementState.air;
        }
        //使速度平缓变换
        if (Mathf.Abs(desiredMoveSpeed-lastDesiredMoveSpeed)>4f)
        {
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }
        lastDesiredMoveSpeed = desiredMoveSpeed;
    }
    
    //插值变换
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0f;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time<difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }
    //速度控制
    private void SpeedControl()
    {
        if (OnSlope()&&!exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            //不在斜坡上,只考虑xz平面的速度
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            //限制速度
            if (flatVel.magnitude>moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }
    
    //跳跃
    private void Jump()
    {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up*jumpForce,ForceMode.Impulse);//向上作用一个瞬间力来跳跃
    }

    //重设跳跃
    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    //人物移动
    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;//根据输入确定移动方向
        //在斜坡上时
        if (OnSlope()&&!exitingSlope)
        {
            //将人物限制在斜坡上
            rb.AddForce(GetSlopeNormalDir()*80f,ForceMode.Force);
        }
        
        //根据是否在空中来控制空气阻力
        if(grounded&&!OnSlope())
            rb.AddForce(moveDirection.normalized * (moveSpeed * VerForce),ForceMode.Force);//作用力来加速
        else if(OnSlope())
        {
            rb.AddForce(GetSlopeMoveDir(moveDirection) * (moveSpeed * VerForce),ForceMode.Force);//在斜坡上作用力方向为输入方向在斜坡上的投影
        }
        else
            rb.AddForce(moveDirection.normalized * (moveSpeed * VerForce*airMultiplier),ForceMode.Force);//作用力来加速
        if(!wallrunning)
            rb.useGravity = !OnSlope();
    }
    //判断人物是否在斜坡上
    private bool OnSlope()
    {
        //射线检测
        if (Physics.Raycast(transform.position,Vector3.down,out slopeHit,playerHeight*0.5f+0.5f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }
    //获取在斜坡上的移动方向
    private Vector3 GetSlopeMoveDir(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
    //获取速度在斜坡法线上的投影的单位矢量
    private Vector3 GetSlopeNormalDir()
    {
        return Vector3.Project(rb.velocity, slopeHit.normal).normalized;
    }
    
    //开始滑行
    private void StartSlide()
    {
        sliding = true;
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        rb.AddForce(Vector3.down*5f,ForceMode.Impulse);
        //重置计时器
        slideTimer = maxSlideTime;
    }
    
    //正在滑行
    private void SlideMovement()
    {
        //不在斜坡上或往斜坡上走
        if (!OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(moveDirection.normalized*slideForce,ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }
        //从斜坡往下滑
        else
        {
            rb.AddForce(GetSlopeMoveDir(moveDirection)*slideForce,ForceMode.Force);
            //不计时,使在斜坡上可以一直滑行
        }
        if (slideTimer<=0)
        {
            StopSlide();
        }
    }
    
    //停止滑行
    private void StopSlide()
    {
        sliding = false;
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }

    //左右是否有墙
    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out wallRightHit, wallCheckDistance,whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out wallLeftHit, wallCheckDistance,whatIsWall);
    }
    //是否离地更远
    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }
    //开始墙跑
    private void StartWallRun()
    {
        wallrunning = true;
        wallTimer = maxWallTime;
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        //旋转视角
        cam.DoFov(90f);
        if(wallLeft)
            cam.DoTile(-5f);
        if (wallRight)
            cam.DoTile(5f);
    }
    //墙跑运动
    private void WallRunMovement()
    {
        rb.useGravity = false;
        
        Vector3 wallNormal = wallRight ? wallRightHit.normal: wallLeftHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;
        
        
        //向前的力
        rb.AddForce(wallForward*wallWalkForce,ForceMode.Force);
        
        //向墙的力
        if(!(wallLeft&& horizontalInput>0)&& !(wallRight && horizontalInput<0))
            rb.AddForce(-wallNormal*100f,ForceMode.Force);
    }
    //停止墙跑
    private void StopWallRun()
    {
        wallrunning = false;
        rb.useGravity = true;
        cam.DoFov(80f);
        cam.DoTile(0f);
    }

    private void WallJump()
    {
        StopWallRun();
        exitingWall = true;
        exitWallTimer = exitWallTime;
        
        Vector3 wallNormal = wallRight ? wallRightHit.normal: wallLeftHit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        //施加跳跃力
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(forceToApply,ForceMode.Impulse);
    }

}
