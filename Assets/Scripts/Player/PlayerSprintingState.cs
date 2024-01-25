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
    private float leaveSpeed;
    private float firstSpeed;
    
    private Vector3 sprintDir;

    private VLineSummon _vLineSummon;

    private float changeRate;
    private float sprintTime;
    
    //相机行为
    private Transform camTrans;
    private Camera cam;



    //逻辑变量
    private EStateType next;//下一个状态
    private float timer;
    
    //冲刺消耗
    private IPlayer iplayer;
    
    
    public PlayerSprintingState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;
    }
    
    
    public void OnEnter()
    {
        rb = _playerBlackboard.otherSettings.m_rigidbody;
        sprintSpeed = _playerBlackboard.sprintSettings.sprintSpeed;
        orientation = _playerBlackboard.otherSettings.orientation;

        camTrans = _playerBlackboard.otherSettings.camTrans;
        cam = _playerBlackboard.otherSettings.cam;

        leaveSpeed = _playerBlackboard.sprintSettings.sprintLeaveSpeed;
        firstSpeed = rb.velocity.magnitude;
        
        sprintDir = _playerBlackboard.otherSettings.moveDir.magnitude>0.1f? _playerBlackboard.otherSettings.moveDir: orientation.forward.normalized;
        
        rb.velocity = Vector3.zero;

        sprintTime = _playerBlackboard.sprintSettings.sprintTime;
        changeRate = _playerBlackboard.otherSettings.sprintChangeRate;

        iplayer = GameObject.FindWithTag("Player").GetComponent<IPlayer>();

        
        //调试
        _vLineSummon = _playerBlackboard.otherSettings.vineLine;
        //镜头行为

        if (_playerBlackboard.otherSettings.dirInput.x > 0)
        {
            //镜头晃动
            camTrans.DOLocalRotate(new Vector3(0, 0, -3), 0.2f);
        }
        else if(_playerBlackboard.otherSettings.dirInput.x < 0)
        {
            camTrans.DOLocalRotate(new Vector3(0, 0, 3), 0.2f);
        }
        else
        {
            if (_playerBlackboard.otherSettings.dirInput.z < 0)
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
        next = _playerBlackboard.otherSettings.next;
        //冲刺跳
        float rate=leaveSpeed;//正常为冲刺前速度
        if (next == EStateType.Jumping)
        {
            if (iplayer.GetEnerge() > 100)
            {
                iplayer.TakeEnerge(100);
                rate = changeRate * ((timer / sprintTime < 1 ? timer / sprintTime : 1)*(sprintSpeed-firstSpeed)+firstSpeed);//在first和sprint速度之间线性取值
            }
            else
            {
                //消耗失败
                iplayer.TakeEnergeFailAudio();
            }
        }
        
        
        rb.velocity = sprintDir * rate;
        _playerBlackboard.otherSettings.speed=sprintDir * rate;
        _playerBlackboard.otherSettings.speedMag = rate;
        

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
