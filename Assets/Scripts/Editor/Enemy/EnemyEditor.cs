using EditorExtend;
using UnityEditor;



[CustomEditor(typeof(Enemy))]
public class EnemyEditor: AutoEditor
{
    [AutoProperty]
    public SerializedProperty blackboard;

    public EnemyBlackboardIndirectEditor board;
    protected override void OnEnable()
    {
        board = new EnemyBlackboardIndirectEditor(blackboard);
        board.enemyType = EnemyType.Zombie;
    }
    protected override void MyOnInspectorGUI()
    {
        board.OnInspectorGUI();
    }
}
