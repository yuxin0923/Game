// Assets/Scripts/AI/PatrolStrategy.cs
using UnityEngine;

namespace AIEnemy
{
public class PatrolStrategy : IEnemyStrategy
{
    private float _leftX, _rightX;   // 巡逻边界
    private int   _dir;              // 当前朝向：+1=右, -1=左
    private float speed;

    // 在初始化里，你会根据 Enemy 初始位置算 _leftX/_rightX、设定 _dir 初值
    public void Init(Vector2 startPos, float halfDistance, float speed)
    {
        _leftX  = startPos.x - halfDistance;
        _rightX = startPos.x + halfDistance;
        _dir    = -1;      // 初始朝左；你根据美术默认朝向调这个值
        this.speed = speed;
    }

    public bool Execute(AIEnemyManager ctx, float dt)
    {
        // 1. 看到玩家就跳出，给 AIManager 切到 Chase
        if (ctx.PlayerInSight)
            return true;

        // 2. 获取当前坐标和碰撞盒半尺寸
        Vector2 pos  = (Vector2)ctx.transform.position;
        Vector2 half = ctx.Body.HalfSize;
        const float δ = 0.05f;

        // 3. 到巡逻边界掉头
        if ((_dir < 0 && pos.x <= _leftX) || (_dir > 0 && pos.x >= _rightX))
        {
            _dir *= -1;
        }

        // 4. 前置传感器：墙与悬崖检测
        //    探测点1：前方微出碰撞盒 ((half.x + δ) * _dir, 0)
        Vector2 wallProbe = pos + new Vector2((_dir > 0 ? half.x : -half.x) + δ, 0);
        //    探测点2：前方微出，向下 ((half.y + δ))
        Vector2 groundProbe = wallProbe + new Vector2(0, -(half.y + δ));

        bool wallAhead = TilemapWorld.I.IsSolid(wallProbe);
        bool gapAhead  = !TilemapWorld.I.IsSolid(groundProbe);

        if (wallAhead || gapAhead)
        {
            _dir *= -1;
        }

        // 5. 移动 & 动画：注意 MoveHoriz 用的 dir 一定要等于面朝方向
        ctx.Body.MoveHoriz(_dir, speed);
        //    SetAnimMove 的第一个参数 vx = _dir * speed，本质上 vx>0→朝右，vx<0→朝左
        ctx.SetAnimMove(_dir * speed, false);

        return false; // 仍旧留在 Patrol 状态
    }
}

}