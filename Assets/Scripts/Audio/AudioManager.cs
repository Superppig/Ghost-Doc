using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum AudioType
{
    BGM,
    Foot,
    Gun,
    Knife,
    UI,
    Other
}
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioClipAssert _audioClipAssert;
    private void Awake()
    {
        Instance = this;
        _audioClipAssert = Resources.Load<AudioClipAssert>("Audio/AudioClipAssert");
        if (_audioClipAssert == null)
        {
            Debug.LogError("error: AudioClipAssert is null");
        }
    }

    /// <summary>
    /// 播放音效
    /// </summary>

    public void PlaySound(Transform transform,AudioType type, int index, float volume = 1,bool isOverlap=false,bool isLoop = false)
    {
        if (transform == null)
        {
            Debug.LogError("error: transform is null");
            return;
        }
        //获取音频源
        AudioSource audioSource = transform.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("error: audioSource is null");
            return;
        }
        AudioClip clip = type switch
        {
            AudioType.BGM => _audioClipAssert.BGMList[index],
            AudioType.Foot => _audioClipAssert.FootList[index],
            AudioType.Gun => _audioClipAssert.GunList[index],
            AudioType.Knife => _audioClipAssert.KnifeList[index],
            AudioType.UI => _audioClipAssert.UIList[index],
            AudioType.Other => _audioClipAssert.OtherList[index],
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "error: AudioType is not defined")
        };
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = isLoop; 
        if(isOverlap)
            audioSource.PlayOneShot(clip);
        else
            audioSource.Play();
    }
    
    /// <summary>
    /// 停止音效
    /// </summary>
    public void StopSound(Transform transform)
    {
        if (transform == null)
        {
            Debug.LogError("error: transform is null");
            return;
        }
        AudioSource audioSource = transform.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("error: audioSource is null");
            return;
        }
        audioSource.Stop();
    }
}
