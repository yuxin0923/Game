using UnityEngine;

/*
 LineOfSight.cs — Bresenham Tile Raycast
 ---------------------------------------
 Lightweight utility that answers: “Is the straight-line path between two
 world points free of solid tiles?”  It converts both positions to tile
 coordinates and walks the grid using the integer Bresenham / Amanatides-Woo
 Digital Differential Analyzer, calling `TilemapWorld.I.IsSolid()` on every
 cell encountered.

 Returns
   true  → line of sight is clear
   false → blocked by at least one solid tile
*/


public static class LineOfSight
{
    /// <param name="from">World position</param>
    /// <param name="to">World position</param>
    /// <returns>true = unobstructed / false = blocked</returns>
    public static bool Clear(Vector2 from, Vector2 to)
    {
        // 1. World coordinates to tile grid coordinates (integer)
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
            // 2. Check if the current frame is solid
            if (TilemapWorld.I.IsSolid(CellCenter(x, y)))
                return false;                          // ↙ blocked

            if (x == endX && y == endY) break;        // terminate

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x += stepX; }
            if (e2 < dx) { err += dx; y += stepY; }
        }
        return true;                                  // ↙ unobstructed
    }

    static Vector2 CellCenter(int x, int y)
    {
        // Take the transform of the first Tilemap and convert it back to the center of the world.
        var map = TilemapWorld.I.solidTilemaps[0];
        Vector3Int cell = new(x, y, 0);
        return map.GetCellCenterWorld(cell);
    }
}
