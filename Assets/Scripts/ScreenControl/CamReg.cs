using Cinemachine;
using Services;
using UnityEngine;

public class CamReg : MonoBehaviour
{
    private void Awake()
    {
        ServiceLocator.Get<ScreenControl>().CamRegist(GetComponent<CinemachineVirtualCamera>());
    }
}