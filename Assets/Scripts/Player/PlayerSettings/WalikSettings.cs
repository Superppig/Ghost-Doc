using System;
using UnityEngine;

[Serializable]
public class WalkSettings
{
    [Header("行走")] public float walkSpeed; //行走速度

    public float accelerate; //加速度
    public float groundDrag; //地面阻力
}
