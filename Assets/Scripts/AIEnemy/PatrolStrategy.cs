// Assets/Scripts/AIEnemy/PatrolStrategy.cs
using UnityEngine;

namespace AIEnemy
{
    public class PatrolStrategy : IEnemyStrategy
    {
        float _leftX, _rightX;
        int   _dir;
        float _speed;

        float _prevX;
        int   _stuckFrames;
        float _flipCooldown;

        public void Init(Vector2 start, float halfDist, float speed)
        {
            _leftX  = start.x - halfDist;
            _rightX = start.x + halfDist;

            _dir    = -1;          // 初始朝向向左
            _speed  = speed;

            

            _prevX  = start.x;
            _stuckFrames = 0;
            _flipCooldown = 0f;
        }

        public bool Execute(AIEnemyManager ctx, float dt)
        {
            /* ---------- 1. 视野检测 ---------- */
            if (ctx.PlayerInSight)      // 看到玩家 → 切 Chase
                return true;

            /* ---------- 2. 几何量 ---------- */
            var pos  = ctx.transform.position;
            var half = ctx.Body.HalfSize;
            const float δ = 0.03f;      // 微小偏移

            /* ---------- 3. 探针取样 ---------- */
            //   3-A  “撞墙探针”抬高到腰部
            Vector2 wallProbe = new Vector2(
                pos.x + _dir * (half.x + δ),
                pos.y + half.y * 0.4f              // 抬高 40 %
            );
            bool wallAhead = TilemapWorld.I.IsSolid(wallProbe);

            //   3-B  “悬崖探针”仍放脚下
            Vector2 gapProbe = new Vector2(
                wallProbe.x,
                pos.y - half.y - δ
            );
            bool gapAhead = !TilemapWorld.I.IsSolid(gapProbe);

            /* ---------- 4. 卡墙保险丝 ---------- */
            if (Mathf.Abs(pos.x - _prevX) < 0.001f)
                _stuckFrames++;
            else
                _stuckFrames = 0;
            _prevX = pos.x;

            if (_stuckFrames > 10)      // ~0.2 s 原地未动
                wallAhead = true;

            /* ---------- 5. 翻面逻辑 ---------- */
            _flipCooldown = Mathf.Max(0, _flipCooldown - dt);

            bool needFlip = false;
            if (_flipCooldown <= 0f)
            {
                // 5-A 触到巡逻边界
                if (_dir < 0 && pos.x <= _leftX  + 0.02f) needFlip = true;
                if (_dir > 0 && pos.x >= _rightX - 0.02f) needFlip = true;

                // 5-B 撞墙 / 悬崖
                if (wallAhead || gapAhead) needFlip = true;
            }

            if (needFlip)
            {
                _dir *= -1;
                _flipCooldown = 0.2f;   // 0.2 s 冷却
            }

            /* ---------- 6. 实际移动 ---------- */
            ctx.Body.MoveHoriz(_dir, _speed);
            ctx.SetAnimMove(_dir * _speed, false);   // false=Patrol

            return false;                           // 继续留在 Patrol
        }
    }
}
