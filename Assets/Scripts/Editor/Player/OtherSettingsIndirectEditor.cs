using EditorExtend;
using UnityEditor;

public class OtherSettingsIndirectEditor : IndirectEditor
{
    protected override string DefaultLabel => "其他设置";
    [AutoProperty]
    public SerializedProperty groundLayer, wallLayer, maxSlopeAngle, sprintChangeRate, walkToSlideCovoteTime, slideToJumpHeightRate;

    protected override void MyOnInspectorGUI()
    {
        groundLayer.PropertyField("地面Layer");
        wallLayer.PropertyField("墙壁Layer");
        maxSlopeAngle.FloatField("最大爬坡角");
        sprintChangeRate.FloatField("sprintChangeRate");
        walkToSlideCovoteTime.FloatField("walkToSlideCovoteTime");
        slideToJumpHeightRate.FloatField("walkToSlideCovoteTime");
    }
}