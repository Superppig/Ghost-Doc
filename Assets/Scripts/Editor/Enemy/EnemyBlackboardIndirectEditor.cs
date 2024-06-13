using System.Security.Policy;
using EditorExtend;
using UnityEditor;

public class EnemyBlackboardIndirectEditor : IndirectEditor
{
    public EnemyType enemyType;
    protected override string DefaultLabel => "敌人状态";

    [AutoProperty] public SerializedProperty commonHealth,
        weakHealth,
        damage,
        commonHitRate,
        criticalStrikeRate,
        speed,
        trunTime,
        hitTime;

    [AutoProperty] public SerializedProperty directRange,
        attackRange,
        attackTime;

    [AutoProperty] public SerializedProperty MaxFireRange,
        MinFireRange,
        fireTime,
        bullet;

    [AutoProperty] public SerializedProperty isAttack,
        hasAttack,
        isHit;

    protected override void MyOnInspectorGUI()
    {
        commonHealth.FloatField("正常血槽");
        weakHealth.FloatField("虚弱血槽");
        damage.FloatField("攻击伤害");
        commonHitRate.FloatField("普通攻击倍率");
        criticalStrikeRate.FloatField("暴击倍率");
        speed.FloatField("移动速度");
        trunTime.FloatField("转向时间");
        hitTime.FloatField("受击间隔");
        switch (enemyType)
        {
            case EnemyType.Zombie:
                directRange.FloatField("直接走向玩家范围范围");
                attackRange.FloatField("攻击范围");
                attackTime.FloatField("攻击间隔");
                break;
            case EnemyType.RemoteEnemy:
                MaxFireRange.FloatField("最大射程");
                MinFireRange.FloatField("逃离范围");
                fireTime.FloatField("攻击间隔");
                bullet.PropertyField("子弹");
                break;
            
        }
        EditorGUI.BeginDisabledGroup(true);
        isAttack.BoolField("攻击中");
        hasAttack.BoolField("完成攻击");
        isHit.BoolField("受击中");
        EditorGUI.EndDisabledGroup();
    }

    public EnemyBlackboardIndirectEditor(SerializedProperty serializedProperty, string label = null) : base(serializedProperty, label)
    {
    }
}