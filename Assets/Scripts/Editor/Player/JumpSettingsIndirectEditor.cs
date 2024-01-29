using EditorExtend;
using UnityEditor;

public class JumpSettingsIndirectEditor : IndirectEditor
{
    protected override string DefaultLabel => "跳跃设置";
    [AutoProperty]
    public SerializedProperty height, wallJumpSpeed, exitWallTime;

    protected override void MyOnInspectorGUI()
    {
        height.FloatField("跳跃高度");
        wallJumpSpeed.FloatField("墙跳速度");
        exitWallTime.FloatField("exitWallTime");
    }
}