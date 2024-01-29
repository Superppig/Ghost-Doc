using EditorExtend;
using UnityEditor;

public class CrouchSettingIndirectsEditor : IndirectEditor
{
    protected override string DefaultLabel => "蹲行设置";
    [AutoProperty]
    public SerializedProperty crouchSpeed, crouchYScale;

    protected override void MyOnInspectorGUI()
    {
        crouchSpeed.FloatField("移动速度");
        crouchYScale.FloatField("Y轴缩放量");
    }
}