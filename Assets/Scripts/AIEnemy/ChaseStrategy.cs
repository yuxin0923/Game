// Assets/Scripts/AI/ChaseStrategy.cs
using UnityEngine;

namespace AIEnemy
{
    public class ChaseStrategy : IEnemyStrategy
    {
        public float speed = 3.5f;
        public float attackRange = 1f;

        public bool Execute(AIEnemyManager ctx, float dt)
        {
            if (!ctx.PlayerInSight)
                return true; // 玩家跑出视野 → 回 Patrol

            float dx = ctx.PlayerTf.position.x - ctx.transform.position.x;
            int chaseDir = dx > 0 ? +1 : -1;

            // 如果已经到达攻击距离，就进入 Attack（返回 true，让 FSM 切换）
            if (Mathf.Abs(dx) <= attackRange)
                return true;

            // 否则继续追逐
            ctx.Body.MoveHoriz(chaseDir, speed);
            ctx.SetAnimMove(chaseDir * speed, true);
            return false;
        }
    }
}
