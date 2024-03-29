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
    [Header("Fog")]
    [Range(0.01f,2.0f)]public float HeightFogDensity;
    public Color ButtomFogColor;
    [Range(0.01f, 2.0f)] public float DistanceFogDensity;
    public Color FarFogColor;
    [Range(0,1)]public float InscatterIntensity;
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

        Shader.SetGlobalColor("_ButtomFogColor", ButtomFogColor);
        Shader.SetGlobalFloat("_HeightFogDensity", HeightFogDensity*0.01f);
        Shader.SetGlobalColor("_FarFogColor", FarFogColor);
        Shader.SetGlobalFloat("_DistanceFogDensity", DistanceFogDensity * 0.01f);
        Shader.SetGlobalFloat("_InscatterIntensity", 1-InscatterIntensity);
    }
}
