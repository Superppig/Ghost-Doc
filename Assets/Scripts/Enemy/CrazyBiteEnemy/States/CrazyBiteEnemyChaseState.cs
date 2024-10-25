
    public class CrazyBiteEnemyChaseState: EnemyStateBase
    {
        public CrazyBiteEnemyChaseState(Enemy enemy) : base(enemy)
        {
        }

        public override void OnEnter()
        {
        }

        public override void OnExit()
        {
        }

        public override void OnUpdate()
        {
            Chase();
        }

        public override void OnCheck()
        {
        }

        public override void OnFixUpdate()
        {
        }
        void Chase()
        {
            enemy.Find(blackboard.player.transform.position);
        }
    }
