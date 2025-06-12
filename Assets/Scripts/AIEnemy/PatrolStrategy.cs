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

            _dir    = -1;          // Initial orientation to the left
            _speed  = speed;

            _prevX  = start.x;
            _stuckFrames = 0;
            _flipCooldown = 0f;
        }

        public bool Execute(AIEnemyManager ctx, float dt)
        {
            /* ---------- 1. Line of Sight ---------- */
            if (ctx.PlayerInSight)      // Sees the player → Switch to Chase
                return true;

            /* ---------- 2. Geometric Quantities ---------- */
            var pos  = ctx.transform.position;
            var half = ctx.Body.HalfSize;
            const float δ = 0.03f;      // Small offset

            /* ---------- 3. Probe Sampling ---------- */
            //   3-A  “撞墙探针”抬高到腰部
            Vector2 wallProbe = new Vector2(
                pos.x + _dir * (half.x + δ),
                pos.y + half.y * 0.4f              // Raise 40%
            );
            bool wallAhead = TilemapWorld.I.IsSolid(wallProbe);

            //   3-B “Cliff Probe” remains underfoot
            Vector2 gapProbe = new Vector2(
                wallProbe.x,
                pos.y - half.y - δ
            );
            bool gapAhead = !TilemapWorld.I.IsSolid(gapProbe);

            /* ---------- 4. Wall Fuse ---------- */
            if (Mathf.Abs(pos.x - _prevX) < 0.001f)
                _stuckFrames++;
            else
                _stuckFrames = 0;
            _prevX = pos.x;

            if (_stuckFrames > 10)      // ~0.2 s Stay put
                wallAhead = true;

            /* ---------- 5. Flip Logic ---------- */
            _flipCooldown = Mathf.Max(0, _flipCooldown - dt);

            bool needFlip = false;
            if (_flipCooldown <= 0f)
            {
                // 5-A Touch patrol boundary
                if (_dir < 0 && pos.x <= _leftX  + 0.02f) needFlip = true;
                if (_dir > 0 && pos.x >= _rightX - 0.02f) needFlip = true;

                // 5-B Wall / Cliff
                if (wallAhead || gapAhead) needFlip = true;
            }

            if (needFlip)
            {
                _dir *= -1;
                _flipCooldown = 0.2f;   // 0.2 s cooldown
            }

            /* ---------- 6. Actual Movement ---------- */
            ctx.Body.MoveHoriz(_dir, _speed);
            ctx.SetAnimMove(_dir * _speed, false);   // false=Patrol

            return false;                           // Continue patrolling
        }
    }
}
