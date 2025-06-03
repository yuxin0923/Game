using UnityEngine;

/// <summary>
/// 100% 自己写的瓦片 Raycast，看 from→to 之间是否被实心瓦片挡住。
/// 算法：整数网格上的 Bresenham Line（也叫 Amanatides & Woo DDA），
/// 每到达一个新栅格就调用 TilemapWorld.I.IsSolid(centerOfCell)。
/// </summary>
public static class LineOfSight
{
    /// <param name="from">世界坐标</param>
    /// <param name="to">世界坐标</param>
    /// <returns>true = 中间无遮挡 / false = 被墙挡住</returns>
    public static bool Clear(Vector2 from, Vector2 to)
    {
        // 1. 把世界坐标 → 瓦片格坐标（整型）
        Vector3Int a = TilemapWorld.I.solidTilemaps[0].WorldToCell(from);
        Vector3Int b = TilemapWorld.I.solidTilemaps[0].WorldToCell(to);

        int x = a.x, y = a.y;
        int endX = b.x, endY = b.y;

        int dx = Mathf.Abs(endX - x);
        int dy = Mathf.Abs(endY - y);

        int stepX = (endX > x) ? 1 : -1;
        int stepY = (endY > y) ? 1 : -1;

        int err = dx - dy;

        while (true)
        {
            // 2. 检查当前格是否实心
            if (TilemapWorld.I.IsSolid(CellCenter(x, y)))
                return false;                          // ↙ 被挡

            if (x == endX && y == endY) break;        // 到终点

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x += stepX; }
            if (e2 <  dx) { err += dx; y += stepY; }
        }
        return true;                                  // ↙ 全程无遮挡
    }

    static Vector2 CellCenter(int x, int y)
    {
        // 取第一张 Tilemap 的 transform，换算回世界中心点
        var map = TilemapWorld.I.solidTilemaps[0];
        Vector3Int cell = new(x, y, 0);
        return map.GetCellCenterWorld(cell);
    }
}
