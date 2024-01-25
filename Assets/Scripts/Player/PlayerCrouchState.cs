using DG.Tweening;
using Player_FSM;
using UnityEngine;

public class PlayerCrouchState : IState
{
    private PlayerBlackboard _playerBlackboard;
    
    private Rigidbody rb;
    private Transform tr;
    private float LastYScale;
    private float YScale;
    private float crouchSpeed;
    private float accelerate;
    private Vector3 moveDir;
    public PlayerCrouchState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;
    }
    public void OnEnter()
    {
        rb = _playerBlackboard.otherSettings.m_rigidbody;
        rb.velocity = _playerBlackboard.otherSettings.speed;

        YScale = _playerBlackboard.crouchSettings.crouchYScale;
        crouchSpeed = _playerBlackboard.crouchSettings.crouchSpeed;
        accelerate = _playerBlackboard.walkSettings.accelerate;
        tr = rb.GetComponent<Transform>();
        LastYScale = tr.localScale.y;
        Crouch();
    }

    public void OnExit()
    {
        tr.localScale = new Vector3(tr.localScale.x, LastYScale, tr.localScale.z);
    }

    public void OnUpdate()
    {
        moveDir = _playerBlackboard.otherSettings.moveDir;

        Walk();
        SpeedCon();
    }

    public void OnCheck()
    {
        
    }

    public void OnFixUpdate()
    {
        
    }

    void Crouch()
    {
        tr.localScale = new Vector3(tr.localScale.x, YScale, tr.localScale.z);
        //给一个瞬间力吸附到地面上
        rb.AddForce(Vector3.down*10f,ForceMode.Impulse);
    }

    void Walk()
    {
        rb.velocity += moveDir * (Time.deltaTime * accelerate);
    }

    void SpeedCon()
    {
        if (rb.velocity.magnitude>crouchSpeed)
        {
            rb.velocity = rb.velocity.normalized * crouchSpeed;
        }
        
        if (moveDir.magnitude<0.1f&&rb.velocity.magnitude>0.1f)
        {
            // 使用 DOVirtual.Float 插值当前速度到0
            DOTween.To(() => rb.velocity, x => rb.velocity = x, Vector3.zero, 0.05f)
                .SetEase(Ease.InOutQuad);
        }
    }
}
