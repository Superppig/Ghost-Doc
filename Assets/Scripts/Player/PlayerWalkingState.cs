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

    public PlayerWalkingState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;
    }
    
    public void OnEnter()
    {
        //读取黑板数据

        rb = _playerBlackboard.m_rigidbody;
        walkSpeed = _playerBlackboard.walkSpeed;
        accelerate = _playerBlackboard.accelerate;
        
        rb.velocity = _playerBlackboard.speed;
        camTrans = _playerBlackboard.camTrans;
    }
    public void OnExit()
    {
        _playerBlackboard.speed = rb.velocity;
    }
    public void OnUpdate()
    {
        moveDir = _playerBlackboard.moveDir;
        dirInput = _playerBlackboard.dirInput;
        Walk();
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
        if (rb.velocity.magnitude>walkSpeed)
        {
            rb.velocity = rb.velocity.normalized * walkSpeed;
        }

        if (moveDir.magnitude<0.1f&&rb.velocity.magnitude>0.1f)
        {
            // 使用 DOVirtual.Float 插值当前速度到0
            DOTween.To(() => rb.velocity, x => rb.velocity = x, Vector3.zero, 0.05f)
                .SetEase(Ease.InOutQuad);
        }
    }
}
