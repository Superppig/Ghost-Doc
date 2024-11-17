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
}