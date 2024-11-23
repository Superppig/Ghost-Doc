using System;
using System.Collections.Generic;
using Services;
using Services.Event;
using UnityEngine;


[CreateAssetMenu(fileName = "Level", menuName = "Level")]
public class Level : ScriptableObject
{
    public List<Wave> Waves;
    [HideInInspector] public int currentWaveIndex;
    
    private IEventSystem eventSystem;
    private IEventSystem EventSystem
    {
        get
        {
            eventSystem ??= ServiceLocator.Get<IEventSystem>();
            return eventSystem;
        }
    }
    private float m_LevelProgress;
    public float LevelProgress
    {
        get => m_LevelProgress;
        set
        {
            if (value != m_LevelProgress)
            {
                EventSystem.Invoke(EEvent.LevelProgressChange, value);
                m_LevelProgress = value;
            }
        }
    }
    
    
    public void LevelInit()
    {
        currentWaveIndex = 0;
        foreach (var wave in Waves)
        {
            wave.isStart = false;
            wave.isEnd = false;
            wave.currentEnemyCount = 0;
            wave.currentEnemyCount = 0;
            wave.currentEnemyHealth = 0;
            wave.currentEnemyMaxHealth = 0;
        }
    }

    public void LevelRun()
    {
        LevelProgress = (float)currentWaveIndex / Waves.Count;
        
        if (currentWaveIndex < Waves.Count)
        {
            //初始化波
            if (!Waves[currentWaveIndex].isStart)
            {
                Waves[currentWaveIndex].WaveInit();
                Waves[currentWaveIndex].isStart = true;
            }
            
            //运行波
            Waves[currentWaveIndex].WaveRun();
            
            if (Waves[currentWaveIndex].isEnd)
            {
                currentWaveIndex++;
                currentWaveIndex = Math.Clamp(currentWaveIndex, 0, Waves.Count - 1);
            }
        }
    }

    public bool CheckLevel()
    {
        return currentWaveIndex >= Waves.Count - 1 && Waves[currentWaveIndex].isEnd;
    }
}