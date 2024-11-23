using System;
using System.Collections.Generic;
using MyTimer;
using Services;
using Services.Event;
using Unity.VisualScripting;
using UnityEngine;

public class ProcedureLevelRun: ProcedureBase
{
    public List<Level> levels = new List<Level>();
    public int levelIndex;
    public int levelMax=2;
    
    private WaveManager _waveManager;
    private UIManager _uiManager;
    private IEventSystem _eventSystem;
    public override void OnInit()
    {
        //导入level
        for (int i = 1; i <= levelMax; i++)
        {
            string path = "Levels/level" + i.ToString();
            levels.Add(Resources.Load<Level>(path));
        }
        levelIndex = 0;
        
        _waveManager = ServiceLocator.Get<WaveManager>();
        _uiManager = ServiceLocator.Get<UIManager>();
        _eventSystem = ServiceLocator.Get<IEventSystem>();
    }

    public override void OnEnter()
    {
        Debug.Log("Current Level Index:"+levelIndex);
        if(levelIndex<levelMax)
        {
            _waveManager.StartLevel(levels[levelIndex]);
            _uiManager.ShowView<LevelUI>();
        }
        else
        {
            CurrentFsm.ChangeState<ProcedureGameOver>();
        }
    }

    public override void OnUpdate()
    {
        if (_waveManager.isRunning)
        {
            return;
        }
        LevelComplete();
        CurrentFsm.ChangeState<ProcedureLevelEnd>();
    }

    public override void OnFixedUpdate()
    {
    }
    public override void OnExit()
    {
        levelIndex++;
        levelIndex = Math.Clamp(levelIndex, 0, levelMax);
        _uiManager.CloseView<LevelUI>();
    }
    public override void OnShutdown()
    {
    }
    public override void OnCheck()
    {
    }
    
    
    void LevelComplete()
    {
        _eventSystem.Invoke(EEvent.LevelComplete);
        _uiManager.ShowView<LevelCompeleteUI>();
    }
}