using System;
using UnityEngine;

[Serializable]
public class KeySettings
{
    [Header("按键")] public KeyCode jumpkey = KeyCode.Space; //跳跃按键
    public KeyCode sprintKey = KeyCode.LeftShift; //冲刺按键
    public KeyCode crouchKey = KeyCode.C; //下蹲按键
    public KeyCode slideKey = KeyCode.LeftControl; //滑行按键
}
