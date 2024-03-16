using EditorExtend;
using UnityEditor;

public class WallRunSettingsIndirectEditor : IndirectEditor
{
    protected override string DefaultLabel => "粘墙设置";
    [AutoProperty]
    public SerializedProperty wallRunSpeed, wallRunGRate, wallCheckDistance, wallRunMinDisTance,maxWallTime,rayCount,leaveMaxAngel,energyCostPerSecond;

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
        rayCount.IntField("射线数量");
        leaveMaxAngel.FloatField("判断角度");
        energyCostPerSecond.FloatField("每秒消耗能量");
    }
}