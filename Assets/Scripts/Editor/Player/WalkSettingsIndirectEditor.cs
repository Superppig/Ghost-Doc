using EditorExtend;
using UnityEditor;

public class WalkSettingsIndirectEditor : IndirectEditor
{
    protected override string DefaultLabel => "行走设置";
    [AutoProperty]
    public SerializedProperty walkSpeed, accelerate, groundDrag;
    protected override void MyOnInspectorGUI()
    {
        walkSpeed.FloatField("最大速度");
        accelerate.FloatField("加速度");
        groundDrag.FloatField("groundDrag");
    }
}