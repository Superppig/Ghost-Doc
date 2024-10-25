using UnityEngine;

public class CrazeBiteEnemyBody:MonoBehaviour,IEnemyBeHit
{
    private Enemy enemy;
    private float rate;
    private HitInfo lastHitInfo;
    private void Awake()
    {
        enemy = transform.root.GetComponent<Enemy>();
        rate = enemy.blackboard.commonHitRate;
    }
    public bool CanBeHit()
    {
        return !enemy.blackboard.isHit;
    }

    public void HitEnemy(HitInfo hitInfo)
    {
        enemy.TakeDamage(rate*hitInfo.damage);
        Debug.Log("身体被击中");
    }
}