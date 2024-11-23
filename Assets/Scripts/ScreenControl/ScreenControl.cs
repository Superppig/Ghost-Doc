using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using Services;
using Unity.VisualScripting;

public class ScreenControl : Service,IService
{
    public override Type RegisterType => typeof(ScreenControl);

    private bool isFrameFrozen = false;
    private bool isCamShaking = false;
    //noise组件
    public CinemachineVirtualCamera camImpulse;
    private CinemachineBasicMultiChannelPerlin noiseModule;
    
    //camtrans
    public Player player;
    private Transform camTrans;

    //顿帧(改变timeScale)
    public void FrameFrozen(int frame,float startTimeScale,Ease ease = Ease.Linear)
    {
        if (!isFrameFrozen)
        {
            isFrameFrozen = true;
            float time = frame/60f;
            Tween tween=DOTween.To(() => Time.timeScale, x => Time.timeScale=x, 1f, time)
                .From(startTimeScale)
                .SetEase(ease);//线性变化
            tween.OnComplete(() => { isFrameFrozen = false; });
        }
    }
    
    //顿帧(改变animationSpeed)
    public void AnimationFrozen(Animator anim,int frame,float startAnimSpeed)
    {
        if (!isFrameFrozen)
        {
            isFrameFrozen = true;
            float time = frame/60f;
            StartCoroutine(EndAnimationForzen(anim,time,startAnimSpeed));
        }
    }
    IEnumerator EndAnimationForzen(Animator anim,float time,float startAnimSpeed)
    {
        anim.speed = startAnimSpeed;
        yield return new WaitForSeconds(time);
        isFrameFrozen = false;
        anim.speed = 1f;
    }


    //相机振动
    public void CamShake(float time,float impulseAmplitude,float impulseFrequency = 0.3f)
    {
        if (!isCamShaking)
        {
            noiseModule = camImpulse.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noiseModule != null)
            {
                StartCoroutine(StartShake(time,impulseAmplitude,impulseFrequency));
            }
            else
            {
                Debug.LogWarning("未找到CinemachineBasicMultiChannelPerlin模块。");
            }
            isCamShaking= true;
        }
    }
    IEnumerator StartShake(float time,float impulseAmplitude,float impulseFrequency)
    {
        float originalAmplitude = noiseModule.m_AmplitudeGain;
        
        noiseModule.m_AmplitudeGain= impulseAmplitude;
        yield return new WaitForSeconds(time);
        noiseModule.m_AmplitudeGain = originalAmplitude;
        isCamShaking = false;
    }
    
    //粒子特效
    public enum ParticleType
    {
        Once,
        Loop,
    }
    public ParticleSystem ParticleRelease(ParticleSystem particle,Vector3 position,Vector3 dir,Transform trans = null,ParticleType type = ParticleType.Once)
    {
        if (particle == null)
        {
            Debug.LogError("Particle system prefab is null.");
            return null;
        }
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, dir);
        ParticleSystem particleInstance = Instantiate(particle, position, rotation);
        if (trans!=null)
        {
            particleInstance.transform.SetParent(trans);
        }
        switch (type)
        {
            case ParticleType.Once:
                Destroy(particleInstance.gameObject, particleInstance.main.duration);
                return null;
            case ParticleType.Loop:
                return particleInstance;
            default:
                return null;
        }
    }
    
    //相机摆动
    public void CamChange(Vector3 rotate,float time)
    {
        camTrans = player.cameraTransform;
        camTrans.DOLocalRotate(rotate, time);
        player.blackboard.canCamChange = false;
        StartCoroutine(FinishCamChange(time));
    }
    IEnumerator FinishCamChange(float time)
    {
        yield return new WaitForSeconds(time);
        camTrans.DOLocalRotate(Vector3.zero, time);
        player.blackboard.canCamChange = true;
    }
    
    //音效播放
    public void PlaySound(AudioClip clip,Vector3 position)
    {
        AudioSource.PlayClipAtPoint(clip, position);
    }
    
    
    public void CamRegist(CinemachineVirtualCamera camImpulse)
    {
        this.camImpulse = camImpulse;
    }
    public void PlayerRegist(Player player)
    {
        this.player = player;
    }
}
