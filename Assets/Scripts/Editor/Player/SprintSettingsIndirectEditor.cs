using EditorExtend;
using UnityEditor;

public class SprintSettingsIndirectEditor : IndirectEditor
{
    protected override string DefaultLabel => "冲刺设置";
    [AutoProperty]
    public SerializedProperty sprintSpeed, sprintDistance, sprintLeaveSpeed;

    public SprintSettingsIndirectEditor(SerializedProperty serializedProperty, string label = null) : base(serializedProperty, label)
    {
    }

    protected override void MyOnInspectorGUI()
    {
        sprintSpeed.FloatField("冲刺速度");
        sprintDistance.FloatField("冲刺距离");
        sprintLeaveSpeed.FloatField("冲刺结束速度");
    }
}