using Services.Save;
using System;
using UnityEngine;

public class SaveTargetController_Debug : SaveTargetController
{
    protected override void Bind(string identifier, UnityEngine.Object obj)
    {
        Group.Bind<SaveData_Debug>(identifier, obj);
    }
}

public class SaveData_Debug : SaveData
{
    public DateTime time;

    public override void Load()
    {
        Debug.Log(time);
    }

    public override void Save()
    {
        time = DateTime.Now;
    }
}
