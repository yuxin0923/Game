using UnityEngine;

/// <summary>
/// CollisionDetector: purely static class responsible for all collision detection logic with AABBs and tilemaps.
/// - AABBOverlap: determines if two rectangles (center+halfSize) intersect. 
/// - ResolveTilemapCollision: given a desiredCenter to move to and a halfSize of the collision box, /// then the current oldCenter halfSize, 
/// and the current oldCenter, returning a corrected final coordinate (to make sure it doesn't overlap with the “solid tile”) 
/// - Internally, the "Is a point solid? I.IsSolid(...) to check if a point falls on a solid tile.
/// </summary>
public static class CollisionDetector
{
    /// <summary>
    /// Determines if AABB(centerA, halfA) intersects with AABB(centerB, halfB)
    /// </summary>
    public static bool AABBOverlap(Vector2 centerA, Vector2 halfA, Vector2 centerB, Vector2 halfB)
    {
        bool overlapX = Mathf.Abs(centerA.x - centerB.x) <= (halfA.x + halfB.x);
        bool overlapY = Mathf.Abs(centerA.y - centerB.y) <= (halfA.y + halfB.y);
        return overlapX && overlapY;
    }

    /// <summary>
    /// Tilemap collision resolution:
    ///   1. Move the object from oldCenter to desiredCenter
    ///   2. First try the X-axis direction. If (desiredCenter.x, oldCenter.y) does not overlap with the tile, then oldCenter.x = desiredCenter.x
    ///   3. Then try the Y-axis direction. If (oldCenter.x, desiredCenter.y) does not overlap with the tile, then oldCenter.y = desiredCenter.y
    ///   4. Return the corrected oldCenter
    ///
    /// world: TilemapWorld singleton reference that internally calls world.IsSolid(...) to check if an arbitrary point falls on a solid tile 
 /// desiredCenter: desired position (oldCenter + velocity*dt) 
 /// halfSize: the half-width and half-height of the collision box 
 /// oldCenter: the current position 
 /// Returns: a final center coordinate that doesn't overlap with the tile.
    /// </summary>
    public static Vector2 ResolveTilemapCollision(TilemapWorld world, Vector2 desiredCenter, Vector2 halfSize, Vector2 oldCenter)
    {
        // —— 1. Horizontal X-axis first —— 
        Vector2 tempCenterX = new Vector2(desiredCenter.x, oldCenter.y);
        if (!IsOverlappingAnyTile(world, tempCenterX, halfSize))
        {
            // X direction can move
            oldCenter.x = tempCenterX.x;
        }
     

        // —— 2. Vertical Y-axis direction try again —— 
        Vector2 tempCenterY = new Vector2(oldCenter.x, desiredCenter.y);
        if (!IsOverlappingAnyTile(world, tempCenterY, halfSize))
        {
            // Y direction can move
            oldCenter.y = tempCenterY.y;
        }

        return oldCenter;
    }

    /// <summary>
    /// Checks if AABB(center, halfSize) overlaps with any solid tile in the tilemap.
    /// Implementation: Sample one point at each of the four "corners" of the AABB,
    /// and call TilemapWorld.I.IsSolid(...) to see if it falls on a solid tile.
    /// If any sample point is solid, we consider it overlapping.
    /// </summary>
    private static bool IsOverlappingAnyTile(TilemapWorld world, Vector2 center, Vector2 halfSize)
    {
        // Calculate the four corners of the AABB in world coordinates
        Vector2 topLeft     = new Vector2(center.x - halfSize.x, center.y + halfSize.y);
        Vector2 topRight    = new Vector2(center.x + halfSize.x, center.y + halfSize.y);
        Vector2 bottomLeft  = new Vector2(center.x - halfSize.x, center.y - halfSize.y);
        Vector2 bottomRight = new Vector2(center.x + halfSize.x, center.y - halfSize.y);

        // If any of the four corners falls on a solid tile, we consider the AABB overlapping
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
