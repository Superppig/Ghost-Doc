using Cinemachine;
using DG.Tweening;
using Player_FSM;
using UnityEngine;

public class PlayerSprintingState : IState
{
    private PlayerBlackboard _playerBlackboard;
    private Rigidbody rb;
    private Transform orientation;
    
    private float sprintSpeed;
    private float firstSpeed;
    
    private Vector3 sprintDir;

    private VLineSummon _vLineSummon;
    
    //相机行为
    private Transform camTrans;
    private Camera cam;

    private StateType next;//下一个状态


    public PlayerSprintingState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;
    }
    
    
    public void OnEnter()
    {
        rb = _playerBlackboard.m_rigidbody;
        sprintSpeed = _playerBlackboard.sprintSpeed;
        orientation = _playerBlackboard.orientation;

        camTrans = _playerBlackboard.camTrans;
        cam = _playerBlackboard.cam;

        firstSpeed = rb.velocity.magnitude;
        sprintDir = _playerBlackboard.moveDir.magnitude>0.1f? _playerBlackboard.moveDir: orientation.forward.normalized;
        

        rb.velocity = Vector3.zero;
        //调试
        _vLineSummon = GameObject.FindWithTag("VLine").GetComponent<VLineSummon>();
        //镜头行为

        if (_playerBlackboard.dirInput.x > 0)
        {
            //镜头晃动
            camTrans.DOLocalRotate(new Vector3(0, 0, -3), 0.2f);
        }
        else if(_playerBlackboard.dirInput.x < 0)
        {
            camTrans.DOLocalRotate(new Vector3(0, 0, 3), 0.2f);
        }
        else
        {
            if (_playerBlackboard.dirInput.z < 0)
            {
                cam.DOFieldOfView(65, 0.2f);
            }
            else
            {
                cam.DOFieldOfView(55, 0.2f);
            }
        }
        _vLineSummon.Summon(orientation.position,sprintDir.normalized);
    }

    public void OnExit()
    {
        next = _playerBlackboard.next;
        //冲刺跳
        if (next != StateType.jumping)
        {
            rb.velocity = sprintDir * firstSpeed;
            _playerBlackboard.speed=sprintDir * firstSpeed;
        }

        camTrans.DOLocalRotate(new Vector3(0, 0, 0), 0.2f);
        cam.DOFieldOfView(60, 0.2f);
    }

    public void OnUpdate()
    {
        rb.velocity = sprintDir * sprintSpeed;
    }

    public void OnCheck()
    {
        
    }

    public void OnFixUpdate()
    {
        
    }
}
