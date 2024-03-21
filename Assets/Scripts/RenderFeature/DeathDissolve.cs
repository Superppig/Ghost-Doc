using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DeathDissolve : VolumeComponent, IPostProcessComponent
{
    public BoolParameter UseSceneCamera = new BoolParameter(false);
    public BoolParameter UseThis = new BoolParameter(false);
    public ClampedFloatParameter Control = new ClampedFloatParameter(0, 0, 1);
    public ColorParameter OutEdgeColor=new ColorParameter(Color.black,true,true,true);
    public ColorParameter InEdgeColor=new ColorParameter(Color.black,true,true,true);
    public ColorParameter BackColor=new ColorParameter(Color.black);
    public ClampedFloatParameter BigEdgeWidth = new ClampedFloatParameter(0.03846f, 0, 0.2f);
    public ClampedFloatParameter SmallEdgeWidth = new ClampedFloatParameter(0.1278f, 0, 0.2f);



    public bool IsActive()
    {
        return true;
    }
    public bool IsTileCompatible()
    {
        return false;
    }
}
