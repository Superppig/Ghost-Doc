using EditorExtend;
using UnityEditor;

[CustomEditor(typeof(SlideSettings))]
public class SlideSettingsIndirectEditor : IndirectEditor
{
    protected override string DefaultLabel => "滑行设置";
    [AutoProperty]
    public SerializedProperty maxSlideTime, vineLineTime, slideYScale, slideAccelerate, startSlideSpeed;

    public SlideSettingsIndirectEditor(SerializedProperty serializedProperty, string label = null) : base(serializedProperty, label)
    {
    }

    protected override void MyOnInspectorGUI()
    {
        maxSlideTime.FloatField("最大滑行时间");
        vineLineTime.FloatField("特效时间");
        slideAccelerate.FloatField("减速的加速度");
        startSlideSpeed.FloatField("滑行所需速度");
    }
}