using EditorExtend;
using UnityEditor;

public class JumpSettingsIndirectEditor : IndirectEditor
{
    protected override string DefaultLabel => "跳跃设置";
    [AutoProperty]
    public SerializedProperty height, wallJumpSpeed,wallUpSpeed, exitWallTime;

    public JumpSettingsIndirectEditor(SerializedProperty serializedProperty, string label = null) : base(serializedProperty, label)
    {
    }

    protected override void MyOnInspectorGUI()
    {
        height.FloatField("跳跃高度");
        wallJumpSpeed.FloatField("最小墙跳速度");
        wallUpSpeed.FloatField("爬墙跳跃速度");
        exitWallTime.FloatField("蹬墙冷却");
    }
}