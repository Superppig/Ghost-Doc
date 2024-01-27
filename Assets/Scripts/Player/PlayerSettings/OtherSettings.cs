using Player_FSM;
using System;
using UnityEngine;

[Serializable]
public class OtherSettings
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
    public EStateType last;
    public EStateType current;
    public EStateType next;

    [Header("着地检测")] public float playerHeight; //玩家最低高度
    public LayerMask whatIsGround; //地面图层

    [Header("上坡")] public float maxSlopeAngle; //最大坡度

    [Header("逻辑变量")]
    public float sprintChangeRate; //冲刺动量转化速率
    public float walkToSlideCovoteTime; //walk状态转化为slide的动量继承土狼时间;
    public float slideToJumpHeightRate = 0.5f; //slide到jump状态,跳跃高度的变化率

    [Space(10)][Header("武器")] public Transform gunModel;
    public Transform gunTrans;

    [Header("其他效果")] public VLineSummon vineLine;
}
