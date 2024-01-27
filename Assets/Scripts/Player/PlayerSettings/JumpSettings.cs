using System;
using UnityEngine;

[Serializable]
public class JumpSettings
{
    [Header("跳跃")] public float height; //跳跃高度

    [Header("墙跳")]
    public float wallJumpSpeed;
    public bool isWallJump;
    public float exitWallTime;

}
