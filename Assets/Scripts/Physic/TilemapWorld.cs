using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapWorld : MonoBehaviour
{
    public static TilemapWorld I { get; private set; }

    [Tooltip("引用场景里用来碰撞的 Tilemap")]
    public Tilemap tilemap;

    void Awake()
    {
        if (I && I != this) Destroy(gameObject);
        I = this;
    }

    /// <summary>给世界坐标，返回该点是否为“实心砖”</summary>
    public bool IsSolid(Vector2 worldPos)
    {
        Vector3Int cell = tilemap.WorldToCell(worldPos);
        return tilemap.HasTile(cell);
    }
}
