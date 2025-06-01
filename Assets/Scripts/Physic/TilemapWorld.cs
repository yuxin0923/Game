using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 世界级 Tilemap 碰撞查询中心（支持多张 Tilemap）。
/// 场景里只保留 1 个即可 —— 把要参与“碰撞判断”的 Tilemap
/// 全部拖到 inspector 的 List 里。
/// </summary>
public class TilemapWorld : MonoBehaviour
{
    /* -------- 单例入口 -------- */
    public static TilemapWorld I { get; private set; }

    /* -------- 在 Inspector 里拖入多张 Tilemap -------- */
    [Tooltip("所有需要参与碰撞的 Tilemap；可拖多张")]
    public List<Tilemap> solidTilemaps = new();

    /* -------- 生命周期 -------- */
    void Awake()
    {
        if (I && I != this)
        {
            Debug.LogWarning("[TilemapWorld] 场景里已有实例，自动销毁重复脚本。");
            Destroy(gameObject);
            return;
        }
        I = this;
    }

    /* -------- 对外 API：给世界坐标，判断是否是“实心砖” -------- */
    public bool IsSolid(Vector2 worldPos)
    {
        if (solidTilemaps == null || solidTilemaps.Count == 0) return false;

        foreach (var map in solidTilemaps)
        {
            if (map == null) continue;                          // 防空
            Vector3Int cell = map.WorldToCell(worldPos);
            if (map.HasTile(cell)) return true;                 // 任意一张有砖即实心
        }
        return false;
    }

#if UNITY_EDITOR
    /* 在编辑器里 Scene 视图实时预览：绿色=空，红色=实心 */
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
