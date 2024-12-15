﻿using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    [SerializeField] private List<MAnimClip> clips = new List<MAnimClip>();

    private Dictionary<string, int> clipDics = new Dictionary<string, int>();
    private PlayableGraph graph;
    private AnimationMixerPlayable mixer;

    private string currentClipName;

    private Tween tween;
    private void Awake()
    {
        graph = PlayableGraph.Create("AnimationGraph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        var playableOutput = AnimationPlayableOutput.Create(graph, "AnimationOutput", GetComponent<Animator>());

        //根据clips数量创建一个mix
        mixer = AnimationMixerPlayable.Create(graph, clips.Count);
        //将mixer连接到output
        playableOutput.SetSourcePlayable(mixer);
        //创建每一个clip的playable
        for (int i = 0; i < clips.Count; i++)
        {
            var clipPlayable = AnimationClipPlayable.Create(graph, clips[i].clip);
            //将clipPlayable连接到mixer
            graph.Connect(clipPlayable, 0, mixer, i);
            //设置clipPlayable的权重
            mixer.SetInputWeight(i, 0);
            
            //将clip的名字和index存入字典
            clipDics.Add(clips[i].name, i);
        }
        graph.Play();
    }


    public void Play(string clipName)
    {
        if (clipDics.ContainsKey(clipName))
        {
            currentClipName = clipName;
            var index = clipDics[clipName];
            //设置当前clip的权重为1
            for (int i = 0; i < clips.Count; i++)
            {
                if (i == index)
                {
                    mixer.SetInputWeight(i, 1f);
                    mixer.GetInput(i).SetTime(0f);
                }
                else
                {
                    mixer.SetInputWeight(i, 0f);
                }
            }
        }
    }

    public void TransmitPlay(string clipName, float transmittime)
    {
        if (clipDics.ContainsKey(clipName))
        {
            tween?.Kill();
            var cur = clipDics[currentClipName];
            var index = clipDics[clipName];
            //设置当前clip的权重为1
            for (int i = 0; i < clips.Count; i++)
            {
                if (i == index)
                {
                    tween = DOTween.To(() => graph.GetRootPlayable(0).GetInputWeight(i),
                        x => graph.GetRootPlayable(0).SetInputWeight(i, x), 1, transmittime);
                }
                else
                {
                    graph.GetRootPlayable(0).SetInputWeight(i, 0);
                }
            }
        }
    }

    private void OnDestroy()
    {
        graph.Destroy();
    }
}

[Serializable]
public class MAnimClip
{
    public AnimationClip clip;
    public string name;
}