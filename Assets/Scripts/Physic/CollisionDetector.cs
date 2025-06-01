using UnityEngine;

/// <summary>
/// CollisionDetector：纯静态类，负责所有与 AABB 和瓦片地图之间的碰撞检测逻辑。
///    • AABBOverlap：判断两个矩形 (center+halfSize) 是否相交
///    • ResolveTilemapCollision：给定一个想要移动到的目标中心 (desiredCenter) 及碰撞盒 halfSize，
///      再给出当前旧中心 (oldCenter)，返回一个修正后的最终坐标（保证不会与“实心瓦片”重叠）
///    • 内部通过 TilemapWorld.I.IsSolid(...) 来检测“某个点是否落在实心瓦片上”
/// </summary>
public static class CollisionDetector
{
    /// <summary>
    /// 判断 AABB(centerA, halfA) 与 AABB(centerB, halfB) 是否相交
    /// </summary>
    public static bool AABBOverlap(Vector2 centerA, Vector2 halfA, Vector2 centerB, Vector2 halfB)
    {
        bool overlapX = Mathf.Abs(centerA.x - centerB.x) <= (halfA.x + halfB.x);
        bool overlapY = Mathf.Abs(centerA.y - centerB.y) <= (halfA.y + halfB.y);
        return overlapX && overlapY;
    }

    /// <summary>
    /// 瓦片地图碰撞修正：
    ///   1. 将物体从 oldCenter 想要移动到 desiredCenter
    ///   2. 先按照 X 轴方向尝试，如果 (desiredCenter.x, oldCenter.y) 不与瓦片重叠，则 oldCenter.x = desiredCenter.x
    ///   3. 再按照 Y 轴方向尝试，如果 (oldCenter.x, desiredCenter.y) 不与瓦片重叠，则 oldCenter.y = desiredCenter.y
    ///   4. 返回修正后的 oldCenter
    ///
    /// world：TilemapWorld 单例引用，内部调用 world.IsSolid(...) 来检测任意点是否落在实心瓦片上
    /// desiredCenter：理想位置（oldCenter + velocity*dt）
    /// halfSize：碰撞盒半宽半高
    /// oldCenter：当前位置
    /// 返回：一个不会与瓦片重叠的最终中心坐标
    /// </summary>
    public static Vector2 ResolveTilemapCollision(TilemapWorld world, Vector2 desiredCenter, Vector2 halfSize, Vector2 oldCenter)
    {
        // —— 1. 水平 X 轴方向先尝试 —— 
        Vector2 tempCenterX = new Vector2(desiredCenter.x, oldCenter.y);
        if (!IsOverlappingAnyTile(world, tempCenterX, halfSize))
        {
            // X 方向可以移动
            oldCenter.x = tempCenterX.x;
        }
        // else：X 方向会撞到瓦片，保持 oldCenter.x 不变

        // —— 2. 垂直 Y 轴方向再尝试 —— 
        Vector2 tempCenterY = new Vector2(oldCenter.x, desiredCenter.y);
        if (!IsOverlappingAnyTile(world, tempCenterY, halfSize))
        {
            // Y 方向可以移动
            oldCenter.y = tempCenterY.y;
        }
        // else：Y 方向会碰到瓦片，保持 oldCenter.y 不变

        // 返回最终不与瓦片重叠的位置
        return oldCenter;
    }

    /// <summary>
    /// 检查 AABB(center, halfSize) 是否与瓦片地图中任意实心瓦片重叠。
    ///   实现方式：在 AABB 的四个“角”上各取一个采样点，调用 TilemapWorld.I.IsSolid(...) 来判断是否落在实心瓦片中。
    ///   如果任意一个采样点是实心瓦片，就判定为重叠。
    /// </summary>
    private static bool IsOverlappingAnyTile(TilemapWorld world, Vector2 center, Vector2 halfSize)
    {
        // 计算 AABB 在世界坐标系下的四个顶点
        Vector2 topLeft     = new Vector2(center.x - halfSize.x, center.y + halfSize.y);
        Vector2 topRight    = new Vector2(center.x + halfSize.x, center.y + halfSize.y);
        Vector2 bottomLeft  = new Vector2(center.x - halfSize.x, center.y - halfSize.y);
        Vector2 bottomRight = new Vector2(center.x + halfSize.x, center.y - halfSize.y);

        // 只要四个角里任意一个落在实心瓦片上，就认为 AABB 与瓦片重叠
        if (world.IsSolid(topLeft)    ||
            world.IsSolid(topRight)   ||
            world.IsSolid(bottomLeft) ||
            world.IsSolid(bottomRight))
        {
            return true;
        }
        return false;
    }
}
