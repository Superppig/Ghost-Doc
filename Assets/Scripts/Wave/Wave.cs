using System;
using System.Collections.Generic;
using AmplifyShaderEditor;
using MyTimer;
using UnityEngine;
using UnityEngine.UI;

public class Wave : MonoBehaviour
{
    public bool isEnd;
    public List<EnemySpawnPoint> enemySpawnPoints;
    public SwitchWaveType switchWaveType;
    
    public float switchValue;//切换波时的血量百分比
    public int switchEnemyCount;//切换波时的敌人数量
    
    
    public float currentEnemyHealth;
    public float currentEnemyMaxHealth;
    public int currentEnemyCount;

    private void Start()
    {
        isEnd = false;
        //生成敌人
        foreach (var enemySpawnPoint in enemySpawnPoints)
        {
            List<Vector3> points = Tools.RandomTool.RandomPointsInCircle(enemySpawnPoint.spawnRange, enemySpawnPoint.enemyCount);
            foreach (var point in points)
            {
                WaveManager.Instance.EnemySpawn(enemySpawnPoint.enemyType, point+enemySpawnPoint.spawnPoint);
            }
        }
    }
    
    private void Update()
    {
        switch (switchWaveType)
        {
            case SwitchWaveType.HealthPercentage:
                if (currentEnemyHealth / currentEnemyMaxHealth <= switchValue)
                {
                    isEnd = true;
                }
                break;
            case SwitchWaveType.EnemyCount:
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
