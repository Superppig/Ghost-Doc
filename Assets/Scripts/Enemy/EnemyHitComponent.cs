using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyHitComponent : MonoBehaviour, IEnemyBeHit
{
    private Enemy enemy;
    public bool isCriticalStrike;
    private float rate;
    private HitInfo lastHitInfo;
    
    private void Awake()
    {
        enemy = transform.parent.GetComponent<Enemy>();
        rate = isCriticalStrike ? enemy.blackboard.criticalStrikeRate : enemy.blackboard.commonHitRate;
    }
    public bool CanBeHit()
    {
        return !enemy.blackboard.isHit;
    }

    public void HitEnemy(HitInfo hitInfo)
    {
        enemy.TakeDamage(rate*hitInfo.damage);
    }
}