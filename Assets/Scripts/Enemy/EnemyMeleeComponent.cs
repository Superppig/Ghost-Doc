using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyMeleeComponent: MonoBehaviour,IBlock
{
    private Enemy enemy;
    private float damage;
    private void Awake()
    {
        enemy = transform.parent.parent.GetComponent<Enemy>();
        damage = enemy.blackboard.damage;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FindObjectOfType<Player>().TakeDamage(damage);
        }
    }
    public void BeBlocked()
    {
    }
}