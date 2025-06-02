// Assets/Scripts/AI/DeadStrategy.cs
using UnityEngine;

namespace AIEnemy
{
    public class DeadStrategy : IEnemyStrategy
    {
        public bool Execute(AIEnemyManager ctx, float dt)
        {
            // 做死亡动画 / 粒子 / 掉落 …
            return false;   // 一直停在 Dead
        }
    }
}
