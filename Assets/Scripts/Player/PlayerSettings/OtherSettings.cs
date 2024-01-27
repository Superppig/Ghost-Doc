using Player_FSM;
using System;
using UnityEngine;

[Serializable]
public class OtherSettings
{
    public LayerMask groundLayer; //地面图层
    public LayerMask wallLayer;
    public float maxSlopeAngle; //最大坡度

    public float sprintChangeRate; //冲刺动量转化速率
    public float walkToSlideCovoteTime; //walk状态转化为slide的动量继承土狼时间;
    public float slideToJumpHeightRate = 0.5f; //slide到jump状态,跳跃高度的变化率
}
