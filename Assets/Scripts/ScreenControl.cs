using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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

    //顿帧
    public void FrameFrozen(int frame,float startTimeScale)
    {
        float time = frame/60f;
        DOTween.To(() => Time.timeScale, x => Time.timeScale=x, 1f, time)
            .From(startTimeScale)
            .SetEase(Ease.Linear);//线性变化
    }
}
