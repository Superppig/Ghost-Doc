﻿using System;
using System.Collections.Generic;
using Services;
using UnityEngine;

public enum ScoreLevel
{
    S,
    A,
    B,
    C,
}
public class ScoreManager:Service,IService
{
    public override Type RegisterType => typeof(ScoreManager);
    public float scoreReduceRate = 0.1f;
    
    public float currentScore = 0;
    public ScoreLevel currentScoreLevel;

    //time
    public float reduceScoreTime = 3;
    private float timer;
    
    public List<float> scoreLevels = new List<float>(){75,50,25,0};
    public List<Material> scoreMaterials = new List<Material>();
    public List<float> materialValues = new List<float>(){0.8f,0.5f,0.25f,0f};

    protected internal override void Init()
    {
        base.Init();
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer > reduceScoreTime)
        {
            currentScore -= scoreReduceRate;
            currentScore = Mathf.Max(0, currentScore);
        }

        MaterialControl();
    }
    void MaterialControl()
    {
        for (int i = 0; i < scoreLevels.Count; i++)
        {
            if (currentScore >= scoreLevels[i])
            {
                currentScoreLevel = (ScoreLevel)i;
                ChangeMaterial("_Scale", materialValues[i]);
                return;
            }
        }
    }

    void ChangeMaterial(string name, float value)
    {
        foreach (var material in scoreMaterials)
        {
            material.SetFloat(name, value);
        }
    }

    public void AddScore(float score)
    {
        currentScore += score;
        timer = 0f;
    }
    public void AddMaterial(Material material)
    {
        scoreMaterials.Add(material);
    }

    public string GetCurScore()
    {
        return currentScoreLevel.ToString();
    }
}