using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneEffectUtils
{
    static int bloodDirtyId = Shader.PropertyToID("_BloodDirty");
    static int moodLightColorId = Shader.PropertyToID("_MoodLightColor");

    float transitionStartTime = 0;

    float transitionDuration = 0;

    float currentDirty = 0;

    float targetDirty;

    SceneEffectSetting setting;

    void SetDirty(float dirty)
    {
        Shader.SetGlobalFloat(bloodDirtyId, dirty);
    }

    public void Setup(SceneEffectSetting setting)
    {
        this.setting = setting;
    }

    /// <summary>
    /// 如果在其他方法中使用了变换时间的参数，则需要每帧调用Update
    /// </summary>
    public void Update()
    {
        if(transitionDuration > 0)
        {
            float factor = (Time.time - transitionStartTime)/transitionDuration;
            float current = Mathf.Lerp(currentDirty, targetDirty, factor);
            SetDirty(current);
            if (factor > 1)
            {
                transitionDuration = 0;
                currentDirty = current;
            }
        }
    }
    /// <summary>
    /// 设置Player的血渍量，用到参数t时，需要Update方法
    /// </summary>
    /// <param name="target">需要设置的血渍量[0,1]</param>
    /// <param name="t">从当前状态变换到目标所用时间</param>
    public void SetPlayerBloodDirty(float target,float t)
    {
        if (transitionDuration != 0)
        {
            float factor = (Time.time - transitionStartTime) / transitionDuration;
            float current = Mathf.Lerp(currentDirty, targetDirty, factor);
            currentDirty = current;
            targetDirty = Mathf.Clamp01(target);
            transitionDuration = t;
            transitionStartTime = Time.time;
        }
        else
        {
            targetDirty = Mathf.Clamp01(target);
            transitionDuration = t;
            transitionStartTime = Time.time;
        }
    }

    /// <summary>
    /// 直接设置血渍量，不需要Update
    /// </summary>
    /// <param name="target">需要设置的血渍量[0,1]</param>
    public void SetPlayerBloodDirty(float target)
    {
        currentDirty = Mathf.Clamp01(target);
        SetDirty(currentDirty);
    }

    /// <summary>
    /// 设置场景氛围灯，mood范围[0,1]，方法没有设置过渡，氛围值本身应该是连续的
    /// </summary>
    /// <param name="mood">氛围值</param>
    public void SetMoodLight(float mood)
    {
        Color c = setting.SceneEffectKey.Evaluate(mood);
        Shader.SetGlobalColor(moodLightColorId, c.linear);
    }

}
