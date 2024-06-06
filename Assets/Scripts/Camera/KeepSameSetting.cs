using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepSameSetting : MonoBehaviour
{
    public Camera parentCam;
    private Camera thisCam;
    void Start()
    {
        thisCam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        KeepSame();
    }

    void KeepSame()
    {
        thisCam.fieldOfView = parentCam.fieldOfView;
        thisCam.nearClipPlane = parentCam.nearClipPlane;
        thisCam.farClipPlane = parentCam.farClipPlane;
    }
}
