using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class FlowCamera : MonoBehaviour
{
    public Camera camera;
    [Min(0.0f)]
    public float depth = 1.0f;


    // Update is called once per frame
    void Update()
    {
        transform.position = camera.transform.position;
        transform.rotation = camera.transform.rotation;
        transform.position += transform.forward * (depth + camera.nearClipPlane);
        float fov = camera.fieldOfView * Mathf.Deg2Rad * 0.5f;
        float aspect = camera.aspect;
        float ySize = Mathf.Tan(fov) * (depth + camera.nearClipPlane) * 2.0f;
        float xSize = ySize * aspect;
        transform.localScale = new Vector3(xSize, ySize, 1);
    }
}
