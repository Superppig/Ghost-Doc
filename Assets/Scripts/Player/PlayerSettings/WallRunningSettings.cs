using System;
using UnityEngine;

[Serializable]
public class WallRunningSettings
{
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
}
