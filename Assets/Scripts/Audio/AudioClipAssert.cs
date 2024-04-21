using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New AudioClipAssert", menuName = "Audio/AudioClipAssert")]
public class AudioClipAssert : ScriptableObject
{
    public List<AudioClip> BGMList = new List<AudioClip>();
    public List<AudioClip> FootList = new List<AudioClip>();
    public List<AudioClip> GunList = new List<AudioClip>();
    public List<AudioClip> KnifeList = new List<AudioClip>();
    public List<AudioClip> UIList = new List<AudioClip>();
    public List<AudioClip> OtherList = new List<AudioClip>();
}
