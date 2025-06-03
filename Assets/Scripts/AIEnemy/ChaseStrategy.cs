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
            return true; // 走回 Patrol

        // 1. 算水平方向：目标在我左边→dir=-1，在右边→dir=+1
        float dx = ctx.PlayerTf.position.x - ctx.transform.position.x;
        int chaseDir = dx > 0 ? +1 : -1;

        // 2. 判是否进入攻击范围
        if (Mathf.Abs(dx) <= attackRange)
        {
            // 进入 Attack 状态
            return true;
        }

        // 3. 移动 & 动画
        ctx.Body.MoveHoriz(chaseDir, speed);
        //    传 chaseDir*speed 给 SetAnimMove，第二个参数标记为“正在追击”
        ctx.SetAnimMove(chaseDir * speed, true);

        return false; // 仍旧留在 Chase
    }
}

}