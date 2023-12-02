using System;
using System.Collections;
using System.Collections.Generic;
using Player_FSM;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Player))]
public class ChangeMatrix : Editor
{
    private Player targetPlayer;

    private void OnEnable()
    {
        targetPlayer = target as Player;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("状态邻接矩阵", EditorStyles.boldLabel);

        // 获取 Enum 类型的所有值
        StateType[] states = (StateType[])Enum.GetValues(typeof(StateType));

        EditorGUILayout.BeginHorizontal(); // 开始第一行

        // 空的格子，与状态名对应
        GUILayout.Space(80);

        // 显示列名
        for (int i = 0; i < states.Length; i++)
        {
            EditorGUILayout.LabelField(states[i].ToString(), GUILayout.Width(20));
        }

        EditorGUILayout.EndHorizontal(); // 结束第一行

        // 显示二维数组的状态切换关系
        for (int i = 0; i < states.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // 显示行名
            EditorGUILayout.LabelField(states[i].ToString(), GUILayout.Width(80));

            for (int j = 0; j < states.Length; j++)
            {
                targetPlayer.changeMatrix[i, j] = EditorGUILayout.Toggle(
                    targetPlayer.changeMatrix[i, j],
                    GUILayout.Width(20)
                );
            }

            EditorGUILayout.EndHorizontal();
            
            //不可缺少
            serializedObject.ApplyModifiedProperties();
        }

        // 显示 Save 按钮
        if (GUILayout.Button("Save"))
        {
            // 标记对象为已修改
            EditorUtility.SetDirty(targetPlayer);
        }
    }
}