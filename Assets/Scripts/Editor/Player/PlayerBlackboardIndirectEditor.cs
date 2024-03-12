using EditorExtend;
using Player_FSM;
using UnityEditor;

public class PlayerBlackboardIndirectEditor : IndirectEditor
{
    protected override string DefaultLabel => "玩家状态";

    [AutoProperty]
    public SerializedProperty health, energy, maxHealth, maxEnergy;
    [AutoProperty]
    public SerializedProperty isWallJump, dirInput, moveDir, velocity, speed, climbSpeed;
    [AutoProperty]
    public SerializedProperty lastState, currentState, nextState;
    [AutoProperty]
    public SerializedProperty isRight, isLeft, isMeleeAttacking, isBlocking,hasClimbOverTime,isCombo, isHoldingMelee;

    public PlayerBlackboardIndirectEditor(SerializedProperty serializedProperty, string label = null) : base(serializedProperty, label)
    {
    }

    protected override void MyOnInspectorGUI()
    {
        maxHealth.FloatField("最大生命");
        health.FloatField("当前生命");
        maxEnergy.FloatField("最大生命");
        energy.FloatField("当前能量");
        EditorGUI.BeginDisabledGroup(true);
        isWallJump.BoolField("isWallJump");
        dirInput.Vector3Field("输入方向");
        moveDir.Vector3Field("移动方向");
        velocity.Vector3Field("当前速度");
        climbSpeed.FloatField("爬墙继承速度");
        speed.FloatField("当前速率");
        lastState.EnumField<EStateType>("上一个状态");
        currentState.EnumField<EStateType>("当前状态");
        nextState.EnumField<EStateType>("下一个状态");
        isLeft.BoolField("左侧有墙");
        isRight.BoolField("右侧有墙");
        isMeleeAttacking.BoolField("正在进行近战攻击");
        isBlocking.BoolField("正在进行格挡");
        hasClimbOverTime.BoolField("爬墙是否超时");
        isCombo.BoolField("释放组合技");
        isHoldingMelee.BoolField("是否持有近战武器");
        EditorGUI.EndDisabledGroup();
    }
}