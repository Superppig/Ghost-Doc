using EditorExtend;
using UnityEditor;

public class AirSettingsIndirectEditor : IndirectEditor
{
    protected override string DefaultLabel => "空中设置";
    [AutoProperty]
    public SerializedProperty airTransformAccelerate, playerHeight;

    protected override void MyOnInspectorGUI()
    {
        airTransformAccelerate.FloatField("加速度");
        playerHeight.FloatField("离地高度标准");
    }
}