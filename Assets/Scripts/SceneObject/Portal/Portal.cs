using System;
using UnityEngine;

public class Portal:MonoBehaviour
{
    private SceneLoadUIController screenLoadUIController;
    public String nextSceneName;
    
    private bool isPlayerInPortal;
    
    private void Awake()
    {
        screenLoadUIController = FindObjectOfType<SceneLoadUIController>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isPlayerInPortal)
            {
                screenLoadUIController.LoadSceneAsync(nextSceneName);
                isPlayerInPortal = true;
            }
        }
    }
}