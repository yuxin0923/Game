// Assets/Scripts/AI/PatrolStrategy.cs
using UnityEngine;

namespace AIEnemy
{
    [System.Serializable]
    public class PatrolStrategy : IEnemyStrategy
    {
        [Tooltip("巡逻半程距离 (m)")] public float halfDistance = 3f;
        [Tooltip("巡逻速度 (m/s)")]   public float speed        = 2f;

        private float _leftX, _rightX;
        private int   _dir = 1;          // +1 向右，-1 向左

        /// 进入状态时由 AIEnemyManager 调用
        public void Init(Vector2 origin, float newHalfDist, float newSpeed)
        {
            halfDistance = newHalfDist;
            speed        = newSpeed;

            _leftX  = origin.x - halfDistance;
            _rightX = origin.x + halfDistance;
            _dir    = Random.value > .5f ? 1 : -1;   // 随机起始方向
        }

        public bool Execute(AIEnemyManager ctx, float dt)
        {
            /* ------ 若发现玩家，则让 FSM 切到 Chase ------ */
            if (ctx.PlayerInSight) return true;

            /* ------ 往返巡逻核心算法 ------ */
            var pos = ctx.transform.position;

            // 到边界就掉头
            if (_dir < 0 && pos.x <= _leftX)  _dir = +1;
            if (_dir > 0 && pos.x >= _rightX) _dir = -1;

            // 移动 & 动画
            ctx.Body.MoveHoriz(_dir, speed);

            ctx.SetAnimMove(_dir * speed);

            return false;   // 仍在本状态
        }
    }
}
