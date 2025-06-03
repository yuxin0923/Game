// Assets/Scripts/AI/AttackStrategy.cs
using UnityEngine;

namespace AIEnemy
{
    [System.Serializable]
    public class AttackStrategy : IEnemyStrategy
    {
        [Tooltip("本次攻击扣除玩家手电电量的数值")]
        [Min(1f)] public float damage = 20f;

        [Tooltip("每次攻击间隔 (秒)")]
        public float cooldown = 0.75f;

        private float _cd;

        public bool Execute(AIEnemyManager ctx, float dt)
        {
            // 1) 玩家彻底不在视野内，回到 Patrol
            if (!ctx.PlayerInSight) 
                return true;

            // 2) 玩家跑出攻击距离，回到 Chase
            float dist = Mathf.Abs(ctx.PlayerTf.position.x - ctx.transform.position.x);
            if (dist > ctx.attackRange) 
                return true;

            // 3) 剩下情况：玩家在视野内 && 距离 ≤ attackRange，正常“打人”循环
            _cd -= dt;
            if (_cd <= 0f)
            {
                _cd = cooldown;
                // 播放“抓人”动画（可选）
                ctx.SetAnimCatchPlayer();
                // 对玩家造成效果：让 Player 自己处理“被攻击”逻辑
                var player = ctx.PlayerTf.GetComponent<Player.Player>();
                if (player != null)
                    player.OnAttacked(damage);
            }

            // 4) 保持在 Attack
            return false;
        }
    }
}
