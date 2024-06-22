using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UI;

public class SceneLoadUIController : MonoBehaviour
{

    public Image SceneLoadMask;
    public Slider SceneLoadSlider;
    
    public float maskTime = 0.5f;

    public void Start()
    {
        StartCoroutine(EndSceneLoadMask());
        SceneLoadMask.enabled= true;
        SceneLoadSlider.enabled = false;
    }

    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }
    IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        SceneLoadSlider.enabled = true;
        StartCoroutine(StartSceneLoadMask());
        
        AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;
        while (!asyncOperation.isDone)
        {
            SceneLoadSlider.value = asyncOperation.progress;
            if (asyncOperation.progress >= 0.9f)
            {
                SceneLoadSlider.value = 1;
                ChangeSceneLoadMask(1);
                asyncOperation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
    
    IEnumerator StartSceneLoadMask()
    {
        float time = 0;
        while (time < maskTime)
        {
            time += Time.deltaTime;
            float value =Mathf.Clamp(time / maskTime*255,0f,255f);
            ChangeSceneLoadMask(value);
            yield return null;
        }
    }
    IEnumerator EndSceneLoadMask()
    {
        float time = maskTime;
        while (time > 0)
        {
            time -= Time.deltaTime;
            float value =Mathf.Clamp(time / maskTime,0f,1f);
            ChangeSceneLoadMask(value);
            yield return null;
        }
    }

    void ChangeSceneLoadMask(float value)
    {
        SceneLoadMask.color = new Color(0, 0, 0, value);
    }
}
