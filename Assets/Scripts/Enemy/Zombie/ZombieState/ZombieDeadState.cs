using UnityEngine;
public class ZombieDeadState:EnemyStateBase
{
    public ZombieDeadState(Enemy enemy) : base(enemy)
    {
    }

    public override void OnEnter()
    {
        GameObject.Destroy(enemy.gameObject);
    }

    public override void OnExit()
    {
    }

    public override void OnUpdate()
    {
    }

    public override void OnCheck()
    {
    }

    public override void OnFixUpdate()
    {
    }
}
