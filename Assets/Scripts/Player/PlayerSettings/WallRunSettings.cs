using System;

[Serializable]
public class WallRunSettings
{
    //墙跑速度改为继承
    public float wallRunSpeed; //墙跑速度
    public float wallRunGRate = 0.1f; //滑墙重力倍率

    public float maxWallTime; //最大继承动量时间
    public float wallCheckDistance; //墙跑检测距离
    public float wallRunMinDisTance;
    public int rayCount = 8; //射线数量

    public float leaveMaxAngel;//判断玩家想跑墙或者攀爬的最大角度
    public float energyCostPerSecond; //每秒消耗能量
}
