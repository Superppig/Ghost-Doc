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

    private float changeRate;
    private float sprintTime;
    
    //相机行为
    private Transform camTrans;
    private Camera cam;



    //逻辑变量
    private StateType next;//下一个状态
    private float timer;
    
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

        sprintTime = _playerBlackboard.sprintTime;
        changeRate = _playerBlackboard.sprintChangeRate;
        
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
        
        //初始滑逻辑变量
        timer = 0f;
    }

    public void OnExit()
    {
        next = _playerBlackboard.next;
        //冲刺跳
        float rate=firstSpeed;//正常为冲刺前速度
        if (next == StateType.jumping)
        {
            rate = changeRate * ((timer / sprintTime < 1 ? timer / sprintTime : 1)*(sprintSpeed-firstSpeed)+firstSpeed);//在first和sprint速度之间线性取值
        }
        
        
        rb.velocity = sprintDir * rate;
        _playerBlackboard.speed=sprintDir * rate;
        _playerBlackboard.speedMag = rate;
        

        camTrans.DOLocalRotate(new Vector3(0, 0, 0), 0.2f);
        cam.DOFieldOfView(60, 0.2f);
    }

    public void OnUpdate()
    {
        rb.velocity = sprintDir * sprintSpeed;
        timer += Time.deltaTime;
    }

    public void OnCheck()
    {
        
    }

    public void OnFixUpdate()
    {
        
    }
}
