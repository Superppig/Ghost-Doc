using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSceneEffect : MonoBehaviour
{
    public SceneEffectSetting setting;
    SceneEffectUtils sceneEffect=new SceneEffectUtils();
    // Update is called once per frame
    private void Start()
    {
        sceneEffect.Setup(setting);
    }

    void Update()
    {
        sceneEffect.Update();
        if (Input.GetKeyUp(KeyCode.T))
        {
            float f = Random.value;
            sceneEffect.SetPlayerBloodDirty(f, 1.0f);
        }
        sceneEffect.SetMoodLight(Mathf.Abs(Mathf.Sin(Time.time)));
    }
}
