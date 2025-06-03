// Assets/Scripts/AIEnemy/PatrolStrategy.cs
using UnityEngine;

namespace AIEnemy
{
    public class PatrolStrategy : IEnemyStrategy
    {
        private float _leftX, _rightX;
        private int   _dir;
        private float speed;

        // 保险丝
        private float _prevX;
        private int   _stuckFrames;

        public void Init(Vector2 startPos, float halfDistance, float speed)
        {
            _leftX  = startPos.x - halfDistance;
            _rightX = startPos.x + halfDistance;
            _dir    = -1;
            this.speed = speed;

            _prevX = startPos.x;
            _stuckFrames = 0;
        }

        public bool Execute(AIEnemyManager ctx, float dt)
        {
            /* --- 切到追逐 --- */
            if (ctx.PlayerInSight) return true;

            Vector2 pos  = ctx.transform.position;
            Vector2 half = ctx.Body.HalfSize;
            const  float δ = 0.12f;                    // NEW: 加大 δ

            /* --- 预测下一帧想去哪里 --- */
            float nextX      = pos.x + _dir * speed * dt * 1.5f;      // 乘 1.5 给点余量
            Vector2 nextLead = new(nextX + (_dir > 0 ?  half.x : -half.x) + δ, pos.y);

            bool wallAhead =
                TilemapWorld.I.IsSolid(nextLead + new Vector2(0,  half.y - δ)) ||
                TilemapWorld.I.IsSolid(nextLead)                               ||
                TilemapWorld.I.IsSolid(nextLead + new Vector2(0, -half.y + δ));

            Vector2 footNext = nextLead + new Vector2(0, -(half.y + δ));
            bool gapAhead = !TilemapWorld.I.IsSolid(footNext);
            #if UNITY_EDITOR
            Debug.DrawLine(pos,       nextLead, Color.red);     // 红：预测墙探测线
            Debug.DrawLine(nextLead,  footNext, Color.yellow);  // 黄：预测悬崖探测线
            #endif

            /* --- 卡墙保险丝：2 帧没动也翻向 --- */
            if (Mathf.Abs(pos.x - _prevX) < 0.005f)   // NEW: 阈值扩大
            {
                _stuckFrames++;
                if (_stuckFrames >= 2) wallAhead = true;   // 叠加到检测里
            }
            else _stuckFrames = 0;                         // 恢复计数
            _prevX = pos.x;

            /* --- 巡逻边界 --- */
            if ((_dir < 0 && pos.x <= _leftX) || (_dir > 0 && pos.x >= _rightX))
                _dir *= -1;

            /* --- 真正翻向 --- */
            if (wallAhead || gapAhead)
                _dir *= -1;

            /* --- 移动 & 动画 --- */
            ctx.Body.MoveHoriz(_dir, speed);
            ctx.SetAnimMove(_dir * speed, false);
            return false;
        }

    }


}
