using System;
using System.Collections.Generic;
using Services;
using Services.ObjectPools;
using Unity.VisualScripting;
using UnityEngine;

public enum SwitchWaveType
{
    HealthPercentage,
    EnemyCount
}

public class WaveManager : Service, IService
{
    public override Type RegisterType => typeof(WaveManager);

    //对象池相关
    private IObjectManager objectManager;
    private List<string> enemyTypeToName= new List<string>()
    {
        "Zombie",
        "RemoteEnemy",
        "CrazyBiteEnemy",
    };
    
    //传送门
    public GameObject portal;
    public Vector3 portalPosition;
    
    private Level currentLevel;
    public bool isRunning;

    protected internal override void Init()
    {
        base.Init();
        
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        objectManager = ServiceLocator.Get<IObjectManager>();
    }

    private void Update()
    {
        if (isRunning)
        {
            if (currentLevel.CheckLevel())
            {
                currentLevel = null;
                isRunning = false;
            }
            else
            {
                currentLevel.LevelRun();
            }
        }
    }

    /// <summary>
    /// 开始关卡
    /// </summary>
    /// <param name="level"></param>
    public void StartLevel(Level level)
    {
        Debug.Log("LevelInit");
        //初始化关卡
        currentLevel = level;
        isRunning = true;
        currentLevel.LevelInit();
    }
    
    public void EnemySpawn(EnemyType enemyType, Vector3 spawnPoint)
    {
        Debug.Log("EnemySpawn");
        objectManager.Activate(enemyTypeToName[(int)enemyType], spawnPoint, Vector3.zero, transform);
    }

    public Wave GetCurrentWave()
    {
        return currentLevel.Waves[currentLevel.currentWaveIndex];
    }
}
