using EditorExtend;
using Player_FSM;
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Player))]
public class PlayerEditor : AutoEditor
{
    private const float WordWidth = 80f;
    private const float BlockWidth = 20f;

    private Player player;
    [AutoProperty]
    public SerializedProperty playerBlackboard;
    private PlayerBlackboardEditor playerBlackboardEditor;

    protected override void OnEnable()
    {
        base.OnEnable();
        player = target as Player;
        playerBlackboardEditor = new PlayerBlackboardEditor();
        playerBlackboardEditor.Initialize(playerBlackboard, "角色数据");
        inherit = true;
    }

    protected override void MyOnInspectorGUI()
    {
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("状态邻接矩阵", EditorStyles.boldLabel);

        // 获取 Enum 类型的所有值
        EStateType[] states = (EStateType[])Enum.GetValues(typeof(EStateType));

        EditorGUILayout.BeginHorizontal(); // 开始第一行

        GUILayout.Space(WordWidth);

        // 显示列名
        for (int i = 0; i < states.Length; i++)
        {
            EditorGUILayout.LabelField(states[i].ToString(), GUILayout.Width(BlockWidth));
        }

        EditorGUILayout.EndHorizontal(); // 结束第一行

        // 显示二维数组的状态切换关系
        for (int i = 0; i < states.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // 显示行名
            EditorGUILayout.LabelField(states[i].ToString(), GUILayout.Width(WordWidth));

            for (int j = 0; j < states.Length; j++)
            {
                player.changeMatrix[i, j] = EditorGUILayout.Toggle(
                    player.changeMatrix[i, j],
                    GUILayout.Width(BlockWidth)
                );
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Save"))
        {
            EditorUtility.SetDirty(player);
        }
    }
}