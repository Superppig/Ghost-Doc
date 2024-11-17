using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Level", menuName = "Level")]
public class Level : ScriptableObject
{
    public List<Wave> Waves;
    [HideInInspector] public int currentWaveIndex;
    
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