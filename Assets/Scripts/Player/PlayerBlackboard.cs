using Player_FSM;
using System;
using UnityEngine;

[Serializable]
public class PlayerBlackboard
{
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
}
