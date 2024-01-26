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
    private const float Offset = 1f;

    private Vector2 nameSize;
    private bool foldout;
    EStateType[] stateNames;

    private Player player;
    [AutoProperty]
    public SerializedProperty playerBlackboard;

    protected override void OnEnable()
    {
        base.OnEnable();
        player = target as Player;
        appendInspector = true;
        nameSize = new Vector2(WordWidth, BlockWidth);
        stateNames = (EStateType[])Enum.GetValues(typeof(EStateType));
        foldout = true;
    }

    protected override void MyOnInspectorGUI()
    {
        Rect GetControlRect(float width, float height)
            => EditorGUILayout.GetControlRect(GUILayout.Width(width), GUILayout.Height(height));

        foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "状态邻接矩阵");
        EditorGUILayout.EndFoldoutHeaderGroup();
        if (foldout)
        {
            GUIStyle wordStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight,
                fixedWidth = WordWidth,
                fixedHeight = BlockWidth,
            };

            // 显示列名
            Matrix4x4 origin = GUI.matrix;

            Rect columns = GetControlRect(WordWidth + BlockWidth * stateNames.Length, WordWidth);
            Rect currentRect = new Rect(columns.min + new Vector2(0f, WordWidth - BlockWidth + Offset), nameSize);
            for (int i = 0; i < stateNames.Length; i++)
            {
                GUIUtility.RotateAroundPivot(90f, currentRect.max);
                EditorGUI.LabelField(currentRect, stateNames[i].ToString(), wordStyle);
                GUI.matrix = origin;
                currentRect.x += BlockWidth;    //注意不能用xMin属性
            }

            GUI.matrix = origin;
            // 显示行
            for (int i = 0; i < stateNames.Length; i++)
            {
                Rect row = GetControlRect(columns.width, BlockWidth);
                currentRect = new Rect(row.min, nameSize);
                EditorGUI.LabelField(currentRect, stateNames[i].ToString(), wordStyle);
                currentRect = new Rect(currentRect.min + new Vector2(WordWidth, 0f), currentRect.size);
                for (int j = 0; j < stateNames.Length; j++)
                {
                    player.changeMatrix[i, j] = EditorGUI.Toggle(currentRect, player.changeMatrix[i, j]);
                    currentRect.x += BlockWidth;
                }
            }

            if (GUILayout.Button("Save"))
            {
                EditorUtility.SetDirty(player);
            }
        }
    }
}