using UnityEngine;

namespace Data.PlayerData
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObject/玩家数据", order = 0)]
    public class PlayerData : ScriptableObject
    {
        [Header("行走")] 
        public float walkSpeed; //行走速度
        public float accelerate; //加速度
        public float groundDrag; //地面阻力

        [Header("跳跃")] 
        public float height; //跳跃高度
        public float airTransformAccelerate;

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
        [Header("冲刺")] 
        public float sprintSpeed; //冲刺速度
        public float sprintDistance; //冲刺距离

        public float sprintTime; //冲刺时间(无需暴露)

        [Header("滑行")] 
        public float maxSlideTime; //最大滑行时间
        public float slideYScale; //滑行时y缩放度
        public float slideAccelerate; //滑行减速的加速度
        public float startSlideSpeed = 1f; //滑行至少需要的速度

        [Header("贴墙跑")] 
        //墙跑速度改为继承
        //public float wallRunSpeed; //墙跑速度
        public LayerMask whatIsWall; //wall的图层
        public float wallRunGRate = 0.1f; //滑墙重力倍率

        //public float maxWallTime; //最大墙跑时间
        public float wallCheckDistance; //墙跑检测距离
        public float wallRunMinDisTance;

        [Header("墙跳")] 
        public float wallJumpSpeed;
        public float exitWallTime;
    }
}