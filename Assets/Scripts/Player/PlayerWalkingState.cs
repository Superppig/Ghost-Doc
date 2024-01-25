using Player_FSM;
using UnityEngine;
using DG.Tweening;

public class PlayerWalkingState : IState
{
    private PlayerBlackboard _playerBlackboard;
    
    private float walkSpeed;
    private float accelerate;
    private Vector3 moveDir;
    private Rigidbody rb;
    private Transform camTrans;
    private Vector3 dirInput;

    private float covoteTime;
    private EStateType next;
    
    //逻辑变量
    private float timer;
    private bool isOverCovote;
    private float firstSpeed;

    public PlayerWalkingState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;
    }
    
    public void OnEnter()
    {
        //读取黑板数据

        rb = _playerBlackboard.otherSettings.m_rigidbody;
        walkSpeed = _playerBlackboard.walkSettings.walkSpeed;
        accelerate = _playerBlackboard.walkSettings.accelerate;
        
        rb.velocity = _playerBlackboard.otherSettings.speed;
        camTrans = _playerBlackboard.otherSettings.camTrans;

        covoteTime = _playerBlackboard.otherSettings.walkToSlideCovoteTime;
        //初始化逻辑变量
        timer = 0f;
        isOverCovote = false;
        firstSpeed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
    }
    public void OnExit()
    {
        //退出时复原视角
        camTrans.DOLocalRotate(new Vector3(0, 0, 0), 0.2f);
        
        //动量继承
        //next = _playerBlackboard.next;
    }
    public void OnUpdate()
    {
        moveDir = _playerBlackboard.otherSettings.moveDir;
        dirInput = _playerBlackboard.otherSettings.dirInput;
        Walk();
        //土狼时间
        if (!isOverCovote)
            timer += Time.deltaTime;

        if (timer >= covoteTime)
            isOverCovote = true;
        SpeedCon();
    }
    public void OnCheck()
    {
        
    }
    public void OnFixUpdate()
    {
        
    }
    
    void Walk()
    {
        rb.velocity += moveDir * (Time.deltaTime * accelerate);
        if (dirInput.x < 0)
        {
            camTrans.DOLocalRotate(new Vector3(0, 0, 1), 0.2f);

        }
        else if (dirInput.x>0)
        {
            camTrans.DOLocalRotate(new Vector3(0, 0, -1), 0.2f);
        }
        else
        {
            camTrans.DOLocalRotate(new Vector3(0, 0, 0), 0.2f);
        }
    }
    void SpeedCon()
    {
        if (isOverCovote)
        {
            if (rb.velocity.magnitude>walkSpeed)
            {
                rb.velocity = rb.velocity.normalized * walkSpeed;
            }
        }
        else
        {
            if (rb.velocity.magnitude>firstSpeed)
            {
                rb.velocity = rb.velocity.normalized * firstSpeed;
            }
        }


        if (moveDir.magnitude<0.1f&&rb.velocity.magnitude>0.1f)
        {
            // 使用 DOVirtual.Float 插值当前速度到0
            DOTween.To(() => rb.velocity, x => rb.velocity = x, Vector3.zero, 0.05f)
                .SetEase(Ease.InOutQuad);
        }
    }
}
