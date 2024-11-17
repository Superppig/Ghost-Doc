using System;
using System.Collections.Generic;
using MyTimer;
using Services;
using UnityEngine;

public class ProcedureLevelRun: ProcedureBase
{
    public List<Level> levels = new List<Level>();
    public int levelIndex;
    public int levelMax=1;
    
    private WaveManager _waveManager;

    public override void OnInit()
    {
        //导入level
        for (int i = 1; i <= levelMax; i++)
        {
            string path = "Levels/level" + i.ToString();
            levels.Add(Resources.Load<Level>(path));
            Debug.Log(path);
        }

        _waveManager = ServiceLocator.Get<WaveManager>();
    }

    public override void OnEnter()
    {
        _waveManager.StartLevel(levels[levelIndex]);
    }

    public override void OnUpdate()
    {
        if (_waveManager.isRunning)
        {
            return;
        }
        CurrentFsm.ChangeState<ProcedureLevelEnd>();
    }

    public override void OnFixedUpdate()
    {
    }
    public override void OnExit()
    {
        levelIndex++;
        levelIndex = Math.Clamp(levelIndex, 0, levelMax - 1);
    }
    public override void OnShutdown()
    {
    }
    public override void OnCheck()
    {
    }
}