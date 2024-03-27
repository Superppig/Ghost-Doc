using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class Enviroment : MonoBehaviour
{
    public Light mainLight=null;
    [Range(0,1)]public float control=0;
    public Material skyMat;
    public Vector3 EulerAngle;
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {
        EulerAngle.x = (int)(90 + control * 360);
        EulerAngle.x %= 180;

        //mainLight.transform.rotation = Quaternion.Euler(EulerAngle.x,mainLight.transform.eulerAngles.y, mainLight.transform.eulerAngles.z);
        mainLight.transform.rotation = Quaternion.Euler(EulerAngle);
        skyMat.SetFloat("_Control",control);
    }
}
