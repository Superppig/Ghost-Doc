using System;

[Serializable]
public class SlideSettings
{
    public float maxSlideTime; //最大滑行时间
    public float vineLineTime; //速度线时间
    public float slideYScale; //滑行时y缩放度
    public float slideAccelerate; //滑行减速的加速度
    public float startSlideSpeed = 1f; //滑行至少需要的速度
}
