using EditorExtend;
using UnityEditor;

[CustomEditor(typeof(AirBlower))]
public class AirBlowerEditor : AutoEditor
{
    [AutoProperty]
    public SerializedProperty impulse, mockTime, mockTarget, mass;
    private bool foldout;

    protected override void MyOnInspectorGUI()
    {
        impulse.FloatField("冲量(N·s)");
        foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "模拟设置");
        if(foldout)
        {
            EditorGUI.indentLevel++;
            mockTarget.PropertyField("起点");
            mass.Slider("质量(kg)", 0.1f, 10f);
            mockTime.FloatField("时间(s)");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}