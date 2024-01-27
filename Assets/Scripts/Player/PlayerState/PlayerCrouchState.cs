using DG.Tweening;
using UnityEngine;

public class PlayerCrouchState : PlayerStateBase
{
    private Vector3 originalScale;

    private float YScale => settings.crouchSettings.crouchYScale;
    private float CrouchSpeed => settings.crouchSettings.crouchSpeed;
    private float Accelerate => settings.walkSettings.accelerate;
    private Vector3 MoveDir => blackboard.moveDir;

    public PlayerCrouchState(Player player) : base(player)
    {
    }
    
    public override void OnEnter()
    {
        originalScale = rbTransform.localScale;
        Crouch();
    }

    public override void OnExit()
    {
        rbTransform.localScale = originalScale;
    }

    public override void OnUpdate()
    {
        Walk();
        SpeedCon();
    }

    public override void OnCheck()
    {
        
    }

    public override void OnFixUpdate()
    {
        
    }

    void Crouch()
    {
        rbTransform.localScale = new Vector3(rbTransform.localScale.x, YScale, rbTransform.localScale.z);
        //给一个瞬间力吸附到地面上
        rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);
    }

    void Walk()
    {
        rb.velocity += MoveDir * (Time.deltaTime * Accelerate);
    }

    void SpeedCon()
    {
        if (rb.velocity.magnitude>CrouchSpeed)
        {
            rb.velocity = rb.velocity.normalized * CrouchSpeed;
        }
        
        if (MoveDir.magnitude<0.1f&&rb.velocity.magnitude>0.1f)
        {
            // 使用 DOVirtual.Float 插值当前速度到0
            DOTween.To(() => rb.velocity, x => rb.velocity = x, Vector3.zero, 0.05f)
                .SetEase(Ease.InOutQuad);
        }
    }
}
