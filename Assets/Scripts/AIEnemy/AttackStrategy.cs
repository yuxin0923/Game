// Assets/Scripts/AI/AttackStrategy.cs
using UnityEngine;

namespace AIEnemy
{
    [System.Serializable]
    public class AttackStrategy : IEnemyStrategy
    {
        [Tooltip("造成的伤害"), Min(1)] public float damage = 1f;
        [Tooltip("每次攻击间隔 (秒)")] public float cooldown = 0.75f;

        private float _cd;

        public bool Execute(AIEnemyManager ctx, float dt)
        {
            if (!ctx.PlayerInSight) return true;             // 玩家跑了 → Idle

            _cd -= dt;
            if (_cd <= 0f)
            {
                _cd = cooldown;
                ctx.PlayerTf.GetComponent<Player.Player>()?.Die(); // 示范：秒杀
            }
            return false;                                       // 持续攻击
        }
    }
}