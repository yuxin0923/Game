using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// World-class Tilemap collision query center (supports multiple Tilemaps).
/// Keep only 1 in the scene - drag all the Tilemaps 
/// that you want to participate in “collision determination” to the inspector's List.
/// </summary>
public class TilemapWorld : MonoBehaviour
{
    /* -------- Singleton Entry -------- */
    public static TilemapWorld I { get; private set; }

    /* -------- Drag multiple Tilemaps into the Inspector -------- */
    [Tooltip("All Tilemaps that need to participate in collision; can drag multiple")]
    public List<Tilemap> solidTilemaps = new();

    /* ---------- TilemapWorld.cs  ---------- */
    [System.Serializable]                      // Expandable Structures in Inspector
    public struct SurfaceLayer
    {
        public Tilemap tilemap;               // This layer's tiles
        public PhysicsMaterial material;      // Corresponding physics material
    }

    [Tooltip("Different friction materials' Tilemap list")]
    public List<SurfaceLayer> surfaceLayers = new();

    /// <summary>Returns the PhysicsMaterial of the tile at worldPos; returns null if not found</summary>
    public PhysicsMaterial GetMaterial(Vector2 worldPos)
    {
        foreach (var layer in surfaceLayers)
        {
            if (layer.tilemap == null || layer.material == null) continue;
            Vector3Int cell = layer.tilemap.WorldToCell(worldPos);
            if (layer.tilemap.HasTile(cell))
                return layer.material;
        }
        return null;                          // No special material found, use default
    }

    /* -------- Lifecycle -------- */
    void Awake()
    {
        if (I && I != this)
        {
            Debug.LogWarning("[TilemapWorld] Another instance exists in the scene, automatically destroying duplicate script.");
            Destroy(gameObject);
            return;
        }
        I = this;
    }

    /* -------- External API: Given a world position, determine if it's a "solid brick" -------- */
    public bool IsSolid(Vector2 worldPos)
    {
        if (solidTilemaps == null || solidTilemaps.Count == 0) return false;

        foreach (var map in solidTilemaps)
        {
            if (map == null) continue;                          // anti-aircraft defense
            Vector3Int cell = map.WorldToCell(worldPos);
            if (map.HasTile(cell)) return true;                 // Any tile present means solid
        }
        return false;
    }

#if UNITY_EDITOR
    /* -------- Scene View Real-time Preview in Editor: Green = Empty, Red = Solid -------- */
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Vector2 pos = Camera.current.ScreenToWorldPoint(
                           new Vector3(Screen.width * .5f, Screen.height * .5f));
        bool solid = IsSolid(pos);
        Gizmos.color = solid ? Color.red : Color.green;
        Gizmos.DrawWireSphere(pos, .2f);
    }
#endif
}
