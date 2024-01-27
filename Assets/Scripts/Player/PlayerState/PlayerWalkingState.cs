using DG.Tweening;
using UnityEngine;

public class PlayerWalkingState : PlayerStateBase
{
    private float WalkSpeed => settings.walkSettings.walkSpeed;
    private float Accelerate => settings.walkSettings.accelerate;
    private Vector3 MoveDir => blackboard.moveDir;
    private Vector3 DirInput => blackboard.dirInput;

    private float CovoteTime => settings.otherSettings.walkToSlideCovoteTime;
    //private EStateType next;

    //逻辑变量
    private float timer;
    private bool isOverCovote;
    private float firstSpeed;

    public PlayerWalkingState(Player player) : base(player)
    {
    }

    public override void OnEnter()
    {
        //初始化逻辑变量
        timer = 0f;
        isOverCovote = false;
        firstSpeed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
    }
    public override void OnExit()
    {
        //退出时复原视角
        player.cameraTransform.DOLocalRotate(new Vector3(0, 0, 0), 0.2f);
        
        //动量继承
        //next = _playerBlackboard.next;
    }
    public override void OnUpdate()
    {
        Walk();
        //土狼时间
        if (!isOverCovote)
            timer += Time.deltaTime;

        if (timer >= CovoteTime)
            isOverCovote = true;
        SpeedCon();
    }
    public override void OnCheck()
    {
        
    }
    public override void OnFixUpdate()
    {
        
    }
    
    private void Walk()
    {
        rb.velocity += MoveDir * (Time.deltaTime * Accelerate);
        if (DirInput.x < 0)
        {
            player.cameraTransform.DOLocalRotate(new Vector3(0, 0, 1), 0.2f);

        }
        else if (DirInput.x>0)
        {
            player.cameraTransform.DOLocalRotate(new Vector3(0, 0, -1), 0.2f);
        }
        else
        {
            player.cameraTransform.DOLocalRotate(new Vector3(0, 0, 0), 0.2f);
        }
    }
    private void SpeedCon()
    {
        if (isOverCovote)
        {
            if (rb.velocity.magnitude>WalkSpeed)
            {
                rb.velocity = rb.velocity.normalized * WalkSpeed;
            }
        }
        else
        {
            if (rb.velocity.magnitude>firstSpeed)
            {
                rb.velocity = rb.velocity.normalized * firstSpeed;
            }
        }

        if (MoveDir.magnitude<0.1f&&rb.velocity.magnitude>0.1f)
        {
            // 使用 DOVirtual.Float 插值当前速度到0
            DOTween.To(() => rb.velocity, x => rb.velocity = x, Vector3.zero, 0.05f)
                .SetEase(Ease.InOutQuad);
        }
    }
}
