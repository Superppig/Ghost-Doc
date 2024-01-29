using EditorExtend;
using UnityEditor;

public class JumpSettingsIndirectEditor : IndirectEditor
{
    protected override string DefaultLabel => "跳跃设置";
    [AutoProperty]
    public SerializedProperty height, wallJumpSpeed, exitWallTime;

    public JumpSettingsIndirectEditor(SerializedProperty serializedProperty, string label) : base(serializedProperty, label)
    {
    }

    public JumpSettingsIndirectEditor(SerializedProperty serializedProperty) : base(serializedProperty)
    {
    }

    protected override void MyOnInspectorGUI()
    {
        height.FloatField("跳跃高度");
        wallJumpSpeed.FloatField("墙跳速度");
        exitWallTime.FloatField("exitWallTime");
    }
}