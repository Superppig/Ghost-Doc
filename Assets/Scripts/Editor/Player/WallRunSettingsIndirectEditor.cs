using EditorExtend;
using UnityEditor;

public class WallRunSettingsIndirectEditor : IndirectEditor
{
    protected override string DefaultLabel => "墙跑设置";
    [AutoProperty]
    public SerializedProperty wallRunSpeed, wallRunGRate, wallCheckDistance, wallRunMinDisTance,maxWallTime;

    public WallRunSettingsIndirectEditor(SerializedProperty serializedProperty, string label = null) : base(serializedProperty, label)
    {
    }

    protected override void MyOnInspectorGUI()
    {
        wallRunSpeed.FloatField("移动速度");
        wallRunGRate.FloatField("重力倍率");
        maxWallTime.FloatField("最大继承动量时间");
        wallCheckDistance.FloatField("wallCheckDistance");
        wallRunMinDisTance.FloatField("wallRunMinDisTance");
    }
}