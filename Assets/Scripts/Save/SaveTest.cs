using Services;
using Services.Save;
using UnityEngine;

public class SaveTest : MonoBehaviour
{
#if UNITY_EDITOR

    private SaveGroupController controller;

    private void Start()
    {
        controller = ServiceLocator.Get<ISaveManager>().GetGroup(0);
    }

    private void Update()
    {
        Load();
        Save();
    }

    public void Load()
    {
        if (Input.GetKey(KeyCode.O) && Input.GetKeyDown(KeyCode.L))
        {
            controller.Read();
            controller.Load();
        }
    }

    public void Save()
    {
        if (Input.GetKey(KeyCode.O) && Input.GetKeyDown(KeyCode.S))
        {
            controller.Save();
            controller.Write();
        }
    }
#endif
}
