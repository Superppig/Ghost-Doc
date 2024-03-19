using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SphereBlur : VolumeComponent, IPostProcessComponent
{
    public BoolParameter UseSceneCamera = new BoolParameter(false);
    public ClampedIntParameter DownSample = new ClampedIntParameter(1, 1, 10);
    public FloatParameter Radius = new ClampedFloatParameter(0.64f,0.0f,5.0f);
    public IntParameter Loop = new ClampedIntParameter(3, 1, 10);
    public Vector2Parameter Center = new Vector2Parameter(new Vector2(0.5f, 0.5f));
    public Vector4Parameter DistortPara = new Vector4Parameter(new Vector4(74.0f,0.112f,0.01f, 1.02f));
    public FloatParameter FlowSpeed = new FloatParameter(0.1f);
    public Vector3Parameter TargetPos = new Vector3Parameter(Vector3.zero);
    public Vector2Parameter MaskPara = new Vector2Parameter(new Vector2(0, 1));
    public BoolParameter OutputMask = new BoolParameter(false);



    public bool IsActive()
    {
        return true;
    }
    public bool IsTileCompatible()
    {
        return false;
    }
}
