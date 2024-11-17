
    public class CrazyBiteEnemyChaseState: CrazyBiteStateBase
    {
        public CrazyBiteEnemyChaseState(Enemy enemy) : base(enemy)
        {
        }

        public override void OnInit()
        {
        }

        public override void OnEnter()
        {
        }

        public override void OnExit()
        {
        }

        public override void OnShutdown()
        {
        }

        public override void OnUpdate()
        {
            Chase();
            
            //死亡
            if(blackboard.currentHealth<= 0f)
            {
                CurrentFsm.ChangeState<CrazyBiteEnemyDeadState>();
            }
        
            if (blackboard.currentHealth<= blackboard.weakHealth&&blackboard.currentHealth>0)
            {
                CurrentFsm.ChangeState<CrazyBiteEnemyStaggerState>();
            }
            else if(blackboard.isHit)
            {
                CurrentFsm.ChangeState<CrazyBiteEnemyHitState>();
            }
            
            if(blackboard.distanceToPlayer<= CurrentFsm.Owner.biteRange)
            {
                CurrentFsm.ChangeState<CrazyBiteEnemyAttackState>();
            }
        }

        public override void OnCheck()
        {
        }

        public override void OnFixedUpdate()
        {
        }
        void Chase()
        {
            enemy.Find(blackboard.player.transform.position);
        }
    }
