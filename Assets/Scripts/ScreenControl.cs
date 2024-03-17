using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;


public class ScreenControl : MonoBehaviour
{
    private static ScreenControl instance;
    public static ScreenControl Instance
    {
        get
        {
            if(instance==null)
            {
                instance = Transform.FindObjectOfType<ScreenControl>();
            }
            return instance;
        }
    }
    //noise组件
    public CinemachineVirtualCamera camImpulse;
    private CinemachineBasicMultiChannelPerlin noiseModule;

    //顿帧
    public void FrameFrozen(int frame,float startTimeScale)
    {
        float time = frame/60f;
        DOTween.To(() => Time.timeScale, x => Time.timeScale=x, 1f, time)
            .From(startTimeScale)
            .SetEase(Ease.Linear);//线性变化
    }
    
    
    //相机振动
    public void CamShake(float time,float impulseAmplitude)
    {
        noiseModule = camImpulse.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (noiseModule != null)
        {
            StartCoroutine(StartShake(time,impulseAmplitude));
        }
        else
        {
            Debug.LogWarning("未找到CinemachineBasicMultiChannelPerlin模块。");
        }
    }
    IEnumerator StartShake(float time,float impulseAmplitude)
    {
        noiseModule.m_AmplitudeGain= impulseAmplitude;
        yield return new WaitForSeconds(time);
        noiseModule.m_AmplitudeGain = 0f;
    }
    
    //粒子特效
    public enum ParticleType
    {
        Once,
        Loop,
    }
    public void ParticleRelease(ParticleSystem particle,Vector3 position,Vector3 dir,ParticleType type = ParticleType.Once)
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, dir);
        ParticleSystem particleInstance = Instantiate(particle, position, rotation);
        Destroy(particleInstance.gameObject,particleInstance.main.duration);
    }
    
}
