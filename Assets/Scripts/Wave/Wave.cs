﻿using System;
using System.Collections.Generic;
using Services;
using Services.Event;
using UnityEngine;

[Serializable]
public class Wave
{
    [HideInInspector]public bool isStart;
    [HideInInspector]public bool isEnd;
    public List<EnemySpawnPoint> enemySpawnPoints;
    public SwitchWaveType switchWaveType;
    
    public float switchValue;//切换波时的血量百分比
    public int switchEnemyCount;//切换波时的敌人数量
    
    
    [HideInInspector]public float currentEnemyHealth;
    [HideInInspector]public float currentEnemyMaxHealth;
    [HideInInspector]public int currentEnemyCount;
    
    private IEventSystem eventSystem;
    private IEventSystem EventSystem
    {
        get
        {
            eventSystem ??= ServiceLocator.Get<IEventSystem>();
            return eventSystem;
        }
    }
    
    private float m_WaveProgress;
    public float WaveProgress
    {
        get => m_WaveProgress;
        set
        {
            if (value != m_WaveProgress)
            {
                EventSystem.Invoke(EEvent.WaveProgressChange, value);
                m_WaveProgress = value;
            }
        }
    }

    public void WaveInit()
    {
        Debug.Log("WaveInit");
        WaveManager waveManager = ServiceLocator.Get<WaveManager>();
        foreach (var enemySpawnPoint in enemySpawnPoints)
        {
            for (int i = 0; i < enemySpawnPoint.enemyCount; i++)
            {
                //获取随机点
                Vector3 randomPoint = Tools.RandomTool.RandomVector2();
                waveManager.EnemySpawn(enemySpawnPoint.enemyType,
                    new Vector3(randomPoint.x, 0, randomPoint.y) * enemySpawnPoint.spawnRange + enemySpawnPoint.spawnPoint);
            }
        }
    }
    
    public void WaveRun()
    {
        switch (switchWaveType)
        {
            case SwitchWaveType.HealthPercentage:
                if (currentEnemyMaxHealth > 0f)
                {
                    WaveProgress = 1f - currentEnemyHealth / currentEnemyMaxHealth;
                    if ( currentEnemyHealth / currentEnemyMaxHealth <= switchValue)
                    {
                        isEnd = true;
                    }
                }
                break;
            case SwitchWaveType.EnemyCount:
                WaveProgress = 1f - (float)currentEnemyCount / switchEnemyCount;
                if (currentEnemyCount <= switchEnemyCount)
                {
                    isEnd = true;
                }
                break;
        }
    }
}


[Serializable]
public class EnemySpawnPoint
{
    public int enemyCount;
    public EnemyType enemyType;
    public Vector3 spawnPoint;
    public float spawnRange;
}
