using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class Enviroment : MonoBehaviour
{
    public Light MainLight=null;
    public AnimationCurve LightIntensityCurve;
    public float LightIntensityMultiply = 1.0f;
    [Range(0,1)]public float control=0;
    public Material skyMat;
    public Vector3 EulerAngle;
    [Header("Fog")]
    [Range(0.01f,5.0f)]public float FogIntensity;
    [Range(0.01f,5.0f)]public float HeightFogDensity;
    public Color ButtomFogColor;
    [Range(0.01f, 5.0f)] public float DistanceFogDensity;
    public Color FarFogColor;
    [Range(0.01f, 20.0f)] public float FogContrast = 1.0f;
    [Range(0,1)]public float InscatterIntensity;
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {
        EulerAngle.x = (int)(90 + control * 360);
        EulerAngle.x %= 180;

        //MainLight.transform.rotation = Quaternion.Euler(EulerAngle.x,MainLight.transform.eulerAngles.y, MainLight.transform.eulerAngles.z);
        MainLight.transform.rotation = Quaternion.Euler(EulerAngle);
        skyMat.SetFloat("_Control",control);
       
        Shader.SetGlobalColor("_ButtomFogColor", ButtomFogColor);
        Shader.SetGlobalFloat("_HeightFogDensity", HeightFogDensity*0.01f*FogIntensity);
        Shader.SetGlobalColor("_FarFogColor", FarFogColor);
        Shader.SetGlobalFloat("_DistanceFogDensity", DistanceFogDensity * 0.01f*FogIntensity);
        Shader.SetGlobalFloat("_InscatterIntensity", 1-InscatterIntensity);
        Shader.SetGlobalFloat("_FogContrast", FogContrast);
        float factor = LightIntensityCurve.Evaluate(control);
        MainLight.intensity = LightIntensityMultiply* factor;
    }
}
