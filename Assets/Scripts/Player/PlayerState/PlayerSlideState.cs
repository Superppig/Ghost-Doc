using DG.Tweening;
using UnityEngine;

public class PlayerSlideState : PlayerStateBase
{
    private float YScale => settings.slideSettings.slideYScale;

    private Vector3 MoveDir => blackboard.moveDir;

    private float VinelineTime => settings.slideSettings.vineLineTime;
    private float Accelerate => settings.slideSettings.slideAccelerate;

    private float slideSpeed;
    private Vector3 originalScale;

    private float timer;
    
    private ParticleSystem slideParticle;

    public PlayerSlideState(Player player): base(player)
    {
    }
    public override void OnEnter()
    {
        rb.velocity = blackboard.velocity;
        slideSpeed = blackboard.speed;//继承向量
        originalScale = rbTransform.localScale;
        StartSlide();

        Camera.main.DOFieldOfView(63, 0.2f);
        timer = 0f;
        
        
        //粒子效果
        slideParticle = ScreenControl.Instance.ParticleRelease(settings.otherSettings.SlideParticle,player.GetGround().point, player.GetGround().normal,rbTransform,ScreenControl.ParticleType.Loop);
    }

    public override void OnExit()
    {
        rbTransform.localScale = originalScale;
        Camera.main.DOFieldOfView(60, 0.2f);

        //去除粒子效果
        GameObject.Destroy(slideParticle.gameObject);
    }

    public override void OnUpdate()
    {
        Slide();
        //速度线
        timer+=Time.deltaTime;
        if (timer> VinelineTime)
        {
            player.vineLine.Summon(player.transform.position,new Vector3(player.rb.velocity.x,0,player.rb.velocity.z));
            timer = 0f;
        }
    }

    public override void OnCheck()
    {
    }

    public override void OnFixUpdate()
    {
    }

    private void StartSlide()
    {
        rbTransform.localScale = new Vector3(rbTransform.localScale.x, YScale, rbTransform.localScale.z);
        //给一个瞬间力吸附到地面上
        rb.AddForce(Vector3.down*10f,ForceMode.Impulse);
    }

    private void Slide()
    {
        slideSpeed -= Accelerate * Time.deltaTime;
        slideSpeed = slideSpeed > 0 ? slideSpeed : 0;
        rb.velocity = MoveDir * slideSpeed;
    }
}