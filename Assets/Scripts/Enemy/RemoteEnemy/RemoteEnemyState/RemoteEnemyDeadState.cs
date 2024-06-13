using UnityEngine;
public class RemoteEnemyDeadState:EnemyStateBase
{
    public RemoteEnemyDeadState(Enemy enemy) : base(enemy)
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
