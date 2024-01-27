
using System;
using UnityEngine;

[Serializable]
public class SprintSettings
{
    [Header("冲刺")] public float sprintSpeed; //冲刺速度
    public float sprintDistance; //冲刺距离
    public float sprintLeaveSpeed;//冲刺结束的速度

    public float sprintTime; //冲刺时间(无需暴露)
}
