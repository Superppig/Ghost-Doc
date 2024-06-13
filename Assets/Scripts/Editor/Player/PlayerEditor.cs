using EditorExtend;
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Player))]
public class PlayerEditor : AutoEditor
{
    private StateMatrixDrawer drawer;
    [AutoProperty]
    public SerializedProperty settings, blackboard;

    [AutoProperty]
    public SerializedProperty cameraTransform, vineLine, orientation,playerCam,playerCollider;

    public PlayerSettingsIndirectEditor set;
    public PlayerBlackboardIndirectEditor board;

    protected override void OnEnable()
    {
        base.OnEnable();
        drawer = new StateMatrixDrawer(target as Player);
        set = new PlayerSettingsIndirectEditor(settings);
        board = new PlayerBlackboardIndirectEditor(blackboard);
    }

    protected override void MyOnInspectorGUI()
    {
        set.OnInspectorGUI();
        board.OnInspectorGUI();
        cameraTransform.PropertyField("cameraTransform");
        orientation.PropertyField("orientation");
        vineLine.PropertyField("vineLine");
        playerCam.PropertyField("playerCam");
        playerCollider.PropertyField("playerCollider");
        drawer.Draw();
    }
}

public class StateMatrixDrawer
{
    private const float WordWidth = 80f;
    private const float BlockWidth = 20f;
    private const float Offset = 1f;

    private Vector2 nameSize;
    private bool foldout;
    private readonly EStateType[] stateNames;

    private readonly Player player;

    public StateMatrixDrawer(Player player)
    {
        this.player = player;
        nameSize = new Vector2(WordWidth, BlockWidth);
        stateNames = (EStateType[])Enum.GetValues(typeof(EStateType));
        foldout = true;
    }

    public void Draw()
    {
        static Rect GetControlRect(float width, float height)
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