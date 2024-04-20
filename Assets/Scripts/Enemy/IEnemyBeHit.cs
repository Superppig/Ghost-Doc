using System;
using UnityEngine;

[Serializable]
public struct HitInfo
{
    //击飞
    public bool isHitFly;
    [HideInInspector]public Vector3 dir;
    public float speed;
    public float time;
    //爆炸
    public bool isBomb;
    //基本信息
    public float rate;
}

public interface IEnemyBeHit
{
    public bool CanBeHit();
    public void HitEnemy(HitInfo hitInfo);
}