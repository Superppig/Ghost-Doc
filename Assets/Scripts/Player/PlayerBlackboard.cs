using Player_FSM;
using System;
using UnityEngine;

[Serializable]
public class PlayerBlackboard
{
    public float health = 100f;
    public float energy = 300f;

    public bool grounded;
    public bool jumping;
    public bool isWallJump;

    public Vector3 dirInput;
    public Vector3 moveDir;
    public Vector3 velocity;        //继承速度
    public float speed;             //当前速率

    public EStateType lastState;
    public EStateType currentState;
    public EStateType nextState;

    public RaycastHit wallLeftHit;
    public RaycastHit wallRightHit;
    public RaycastHit currentWall;
    public bool isRight;
    public bool isLeft;

    public RaycastHit slopeHit; //斜坡检测
}
