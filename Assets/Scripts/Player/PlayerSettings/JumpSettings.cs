using System;
using UnityEngine;

[Serializable]
public class JumpSettings
{
    public float height; //跳跃高度

    public float wallJumpSpeed;
    public float wallUpSpeed;
    public float exitWallTime;
    
    public float jumpBufferTime;
}
