// // Assets/Scripts/AI/AttackStrategy.cs
// using UnityEngine;

// namespace AIEnemy
// {
//     [System.Serializable]
//     public class AttackStrategy : IEnemyStrategy
//     {
//         [Tooltip("本次攻击扣除玩家手电电量的数值")]
//         [Min(1f)] public float damage = 20f;

//         [Tooltip("每次攻击间隔 (秒)")]
//         public float cooldown = 0.75f;

//         private float _cd;

//         // public bool Execute(AIEnemyManager ctx, float dt)
//         // {
//         //     // 1. 玩家超出攻击范围(带缓冲) → 回Chase
//         //     float dist = Vector2.Distance(ctx.PlayerTf.position, ctx.transform.position);
//         //     if (dist > ctx.attackRange * 1.2f) 
//         //         return true;

//         //                 // 在AttackStrategy中添加
//         //     if (dist <= 0.1f) // 完全重叠时
//         //     {
//         //         _cd = 0; // 立即重置冷却
//         //     }

//         //     // 2. 冷却时间处理
//         //     _cd -= dt;
//         //     if (_cd <= 0f)
//         //     {
//         //         _cd = cooldown;
//         //         ctx.SetAnimCatchPlayer();

//         //         // 3. 立即造成伤害（无延迟）
//         //         var player = ctx.PlayerTf.GetComponent<Player.Player>();
//         //         player?.OnAttacked(damage);
//         //     }



//         //     // 4. 保持在Attack状态
//         //     return false;
//         // }
//         public bool Execute(AIEnemyManager ctx, float dt)
//         {
//             // 1. 计算真实距离（包含垂直分量）
//             float dist = Vector2.Distance(ctx.PlayerTf.position, ctx.transform.position);
            
//             // 2. 玩家完全重叠时强制攻击（无冷却）
//             if (dist <= 0.1f) 
//             {
//                 _cd = 0; // 立即重置冷却
//             }
            
//             // 3. 冷却时间处理
//             _cd -= dt;
//             if (_cd <= 0f)
//             {
//                 _cd = cooldown;
//                 ctx.SetAnimCatchPlayer();
                
//                 // 4. 立即造成伤害
//                 var player = ctx.PlayerTf.GetComponent<Player.Player>();
//                 player?.OnAttacked(damage);
//             }
            
//             // 5. 保持在Attack状态（状态切换由AIEnemyManager处理）
//             return false;
//         }



//     }
// }
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
        
        [Tooltip("攻击范围，超出该范围则退出攻击状态")]
        public float attackRange = 1f;

        private float _cd;
        private float _waitTimer = 0f;
        private const float MAX_WAIT_TIME = 2f; // 在边缘最多等待2秒

        public bool Execute(AIEnemyManager ctx, float dt)
        {
            // 1. 玩家超出攻击范围(带缓冲) → 回Chase
            float dist = Vector2.Distance(ctx.PlayerTf.position, ctx.transform.position);
            if (dist > attackRange * 1.2f) 
                return true;

            // 2. 悬崖/墙壁检测
            bool cliffOrWall = CheckCliffOrWall(ctx);
            if (cliffOrWall)
            {
                // 停在危险边缘
                ctx.Body.MoveHoriz(0, 0);
                ctx.SetAnimMove(0, true);
                
                // 等待计时
                _waitTimer += dt;
                
                // 等待超时后返回巡逻
                if (_waitTimer >= MAX_WAIT_TIME)
                {
                    _waitTimer = 0f;
                    return true;
                }
                
                return false;
            }
            else
            {
                // 重置等待计时器
                _waitTimer = 0f;
            }

            // 3. 玩家完全重叠时强制攻击（无冷却）
            if (dist <= 0.1f) 
            {
                _cd = 0;
            }

            // 4. 冷却时间处理
            _cd -= dt;
            if (_cd <= 0f)
            {
                _cd = cooldown;
                ctx.SetAnimCatchPlayer();

                // 5. 立即造成伤害
                var player = ctx.PlayerTf.GetComponent<Player.Player>();
                player?.OnAttacked(damage);
            }

            // 6. 保持在Attack状态
            return false;
        }
        
        // 检测前方是否有悬崖或墙壁
        private bool CheckCliffOrWall(AIEnemyManager ctx)
        {
            var pos = ctx.transform.position;
            var half = ctx.Body.HalfSize;
            const float δ = 0.03f; // 微小偏移
            int facing = ctx.Facing;
            
            // 悬崖检测探针
            Vector2 cliffProbe = new Vector2(
                pos.x + facing * (half.x + δ), // 前方一个身位
                pos.y - half.y - 0.1f          // 脚下位置
            );
            
            // 墙壁检测探针
            Vector2 wallProbe = new Vector2(
                pos.x + facing * (half.x + δ), // 前方一个身位
                pos.y                          // 中间高度
            );
            
            // 检测前方是否有地面（悬崖）或墙壁
            bool cliffAhead = !TilemapWorld.I.IsSolid(cliffProbe);
            bool wallAhead = TilemapWorld.I.IsSolid(wallProbe);
            
            return cliffAhead || wallAhead;
        }
    }
}