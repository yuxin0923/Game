// Assets/Scripts/AI/ChaseStrategy.cs
using UnityEngine;

namespace AIEnemy
{
    public class ChaseStrategy : IEnemyStrategy
    {
        public float speed = 3.5f;      // 追击速度
        public float attackRange = 1f;  // 进入攻击的距离

        public bool Execute(AIEnemyManager ctx, float dt)
        {
            if (!ctx.PlayerInSight) return true;         // 看不见 → 回 Patrol

            float dx = ctx.PlayerTf.position.x - ctx.transform.position.x;
            if (Mathf.Abs(dx) <= attackRange) return true; // 够近 → Attack

            ctx.Body.MoveHoriz(Mathf.Sign(dx), speed);
            ctx.SetAnimMove(speed);

            return false;
        }
    }
}
