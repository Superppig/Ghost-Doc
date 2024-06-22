using System;
using System.Collections;
using System.Collections.Generic;
using MyTimer;
using Services;
using Services.ObjectPools;
using UnityEngine;

public enum WaveState
{
    Start,
    Fight, 
    End
}

public enum SwitchWaveType
{
    HealthPercentage,
    EnemyCount
}

public class WaveManager : MonoBehaviour
{
    
    //实例化
    public static WaveManager Instance;
    
    //波数
    public List<Wave> Waves;

    private bool started;
    private bool ended;
    
    public int currentWaveIndex;
    public Wave currentWave;
    //传送门
    public GameObject portal;
    public Vector3 portalPosition;
    
    //当前状态
    public WaveState waveState=WaveState.Start;
    
    //对象池相关
    private IObjectManager objectManager;
    public List<string> enemyTypeToName;

    private void Awake()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        Instance = this;
        
        objectManager = ServiceLocator.Get<IObjectManager>();
    }

    private void Update()
    {
        if (waveState == WaveState.Start)
        {
            waveState = WaveState.Fight;
        }
        else if (waveState == WaveState.Fight)
        {
            if (!started)
            {
                StartWave();
                started = true;
            }
            
            if (Waves.Count == 0)
            {
                waveState = WaveState.End;
            }
        }
        
        else if (waveState == WaveState.End)
        {
            //生成一个传送门
            if (!ended)
            {
                Instantiate(portal, portalPosition, Quaternion.identity);
                ended = true;
            }
        }
    }

    void StartWave()
    {
        currentWave = Instantiate(Waves[0], transform);
        currentWaveIndex = 0;
        StartCoroutine(SwitchWave());
    }

    IEnumerator SwitchWave()
    {
        while (currentWaveIndex < Waves.Count)
        {
            if (currentWave.isEnd)
            {
                currentWaveIndex++;
                if(currentWaveIndex < Waves.Count)
                    currentWave = Instantiate(Waves[currentWaveIndex], transform);
            }
            yield return null;
        }
        waveState = WaveState.End;
    }
    public void EnemySpawn(EnemyType enemyType, Vector3 spawnPoint)
    {
        objectManager.Activate(enemyTypeToName[(int)enemyType], spawnPoint, Vector3.zero, transform);
    }
}
