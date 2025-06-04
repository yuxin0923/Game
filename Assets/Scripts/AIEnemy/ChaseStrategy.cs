// Assets/Scripts/AI/ChaseStrategy.cs
using UnityEngine;

namespace AIEnemy
{
    public class ChaseStrategy : IEnemyStrategy
    {
        public float speed = 3.5f;
        public float attackRange = 1f;
        
        // 新增：悬崖检测相关
        private float _waitTimer = 0f;
        private const float MAX_WAIT_TIME = 2f; // 在边缘最多等待2秒

        public bool Execute(AIEnemyManager ctx, float dt)
        {
            // if (!ctx.PlayerInSight)
            //     return true; // 回Patrol

            // // 使用欧几里得距离替代水平距离
            // float dist = Vector2.Distance(ctx.PlayerTf.position, ctx.transform.position);

            // // 到达攻击距离 → 进入Attack
            // if (dist <= attackRange)
            //     return true;

            // // 计算水平方向用于移动
            // float dx = ctx.PlayerTf.position.x - ctx.transform.position.x;
            // int chaseDir = dx > 0 ? +1 : -1;

                // 1. 计算真实距离（包含垂直分量）
            float dist = Vector2.Distance(ctx.PlayerTf.position, ctx.transform.position);
            
            // 2. 玩家进入攻击范围 → 由AIEnemyManager切换状态
            if (dist <= attackRange) 
                return false; // 不切换状态，等待Manager处理
            
            // 3. 计算水平方向用于移动
            float dx = ctx.PlayerTf.position.x - ctx.transform.position.x;
            int chaseDir = dx > 0 ? +1 : -1;

            // 边缘检测：前方是否有悬崖
            bool cliffAhead = CheckCliffAhead(ctx, chaseDir);
            
            if (cliffAhead)
            {
                // 停在悬崖边
                ctx.Body.MoveHoriz(0, 0);
                ctx.SetAnimMove(0, true); // 速度为0，但仍在追逐状态
                
                // 如果玩家在攻击范围内，准备攻击
                if (Mathf.Abs(dx) <= attackRange * 1.5f)
                {
                    // 保持朝向玩家
                    ctx.SetFacing(chaseDir);
                    return false;
                }
                
                // 等待计时
                _waitTimer += dt;
                
                // 等待超时后返回巡逻
                if (_waitTimer >= MAX_WAIT_TIME)
                {
                    _waitTimer = 0f;
                    return true; // 返回巡逻
                }
                
                return false;
            }
            else
            {
                // 重置等待计时器
                _waitTimer = 0f;
                
                // 正常追逐
                ctx.Body.MoveHoriz(chaseDir, speed);
                ctx.SetAnimMove(chaseDir * speed, true);
                return false;
            }
        }

        // 检查前方是否有悬崖
        private bool CheckCliffAhead(AIEnemyManager ctx, int dir)
        {
            var pos = ctx.transform.position;
            var half = ctx.Body.HalfSize;
            const float δ = 0.03f; // 微小偏移
            
            // 悬崖检测探针
            Vector2 cliffProbe = new Vector2(
                pos.x + dir * (half.x + δ), // 前方一个身位
                pos.y - half.y - 0.1f       // 脚下位置
            );
            
            // 检测前方是否有地面
            bool groundAhead = TilemapWorld.I.IsSolid(cliffProbe);
            
            // 如果没有地面，说明是悬崖
            return !groundAhead;
        }
    }
}
