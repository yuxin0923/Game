using UnityEngine;
using UnityEngine.Tilemaps;  // 用到 Tilemap 组件
using Player;              // 引用 Player.Player（请确保 Player.cs 里的命名空间是 Player）
// 假设 SimplePhysicsBody 在全局命名空间或 Physic 命名空间下，这里直接引用

namespace World
{
    /// <summary>
    /// DeathWall：当玩家与“挂在同一 GameObject 上的 Tilemap（deathwall）”的 AABB 区域重叠时
    /// 先把手电量清零（如果有 Flashlight），再调用 Player.Die()。
    ///
    /// 本脚本挂在“deathwall”这个带 Tilemap 组件的 GameObject 上即可。
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    public class DeathWall : MonoBehaviour
    {
        // —— 私有状态 —— 
        private Vector2 wallCenter;    // Tilemap 在世界坐标下的中心
        private Vector2 wallHalfSize;  // Tilemap 在世界坐标下的半宽半高

        private Player.Player playerRef;
        private SimplePhysicsBody playerBody;
        private Flashlight playerFlash;
        private bool hasKilled = false;

        void Awake()
        {
            // 1) 获取挂在同一个对象上的 Tilemap 组件
            Tilemap tm = GetComponent<Tilemap>();
            if (tm == null)
            {
                Debug.LogError("[DeathWall][Awake]：找不到 Tilemap 组件，请确认挂载在带 Tilemap 的 GameObject 上。");
                return;
            }
            Debug.Log("[DeathWall][Awake]：成功获取 Tilemap 组件。");

            // 2) 计算 Tilemap 本地 Bounds 并转换到世界坐标
            tm.CompressBounds();
            Bounds localBounds = tm.localBounds;
            Debug.Log($"[DeathWall][Awake] localBounds.center = {localBounds.center}, localBounds.size = {localBounds.size}");

            // 把本地中心转换到世界坐标
            Vector3 worldCenter3 = tm.transform.TransformPoint(localBounds.center);
            // 把 localBounds.size 按 lossyScale 转换到世界大小
            Vector3 worldSize3 = Vector3.Scale(localBounds.size, tm.transform.lossyScale);

            wallCenter   = new Vector2(worldCenter3.x, worldCenter3.y);
            wallHalfSize = new Vector2(worldSize3.x * 0.5f, worldSize3.y * 0.5f);
            Debug.Log($"[DeathWall][Awake] worldCenter = {wallCenter}, worldSize = {worldSize3}, wallHalfSize = {wallHalfSize}");
        }

        void Start()
        {
            // 初始化玩家引用
            playerRef = FindObjectOfType<Player.Player>();
            if (playerRef == null)
            {
                Debug.LogError("[DeathWall][Start]：找不到 Player.Player，请确认场景中已有挂 Player 脚本的对象。");
                return;
            }
            Debug.Log($"[DeathWall][Start]：成功获取玩家引用 playerRef = {playerRef.name}");

            // 获取 SimplePhysicsBody 组件
            playerBody = playerRef.GetComponent<SimplePhysicsBody>();
            if (playerBody == null)
            {
                Debug.LogError("[DeathWall][Start]：在 Player 上找不到 SimplePhysicsBody 组件，请确认已挂好。");
            }
            else
            {
                Debug.Log($"[DeathWall][Start]：playerBody.halfSize = {playerBody.halfSize}");
            }

            // 获取 Flashlight 组件（可能没有）
            playerFlash = playerRef.GetComponent<Flashlight>();
            if (playerFlash == null)
            {
                Debug.LogWarning("[DeathWall][Start]：Player 身上没有 Flashlight 组件，触发时会直接调用 Die()。");
            }
            else
            {
                Debug.Log($"[DeathWall][Start]：playerFlash.CurrentCharge = {playerFlash.CurrentCharge}");
            }
        }

        void Update()
        {
            if (hasKilled) return;
            if (playerRef == null || playerBody == null) return;

            Vector2 pCenter = playerRef.transform.position;
            Vector2 pHalf = playerBody.halfSize;

            // 调试日志可保留，但实际发布时可移除以减少日志量
            Debug.Log($"[DeathWall][Update] pCenter = {pCenter}, pHalf = {pHalf}; wallCenter = {wallCenter}, wallHalfSize = {wallHalfSize}");

            bool overlapX = Mathf.Abs(pCenter.x - wallCenter.x) <= (pHalf.x + wallHalfSize.x);
            bool overlapY = Mathf.Abs(pCenter.y - wallCenter.y) <= (pHalf.y + wallHalfSize.y);
            bool isIntersect = overlapX && overlapY;

            Debug.Log($"[DeathWall][Update] overlapX = {overlapX}, overlapY = {overlapY}, isIntersect = {isIntersect}");

            if (isIntersect)
            {
                Debug.Log("[DeathWall][Update] 玩家与 DeathWall AABB 相交 → 触发死亡逻辑");

                // ==== 关键修改开始 ====
                // 1. 无论是否有Flashlight组件，都必须调用Die()
                // 2. 调整执行顺序：先清空电量再触发死亡
                if (playerFlash != null)
                {
                    Debug.Log("[DeathWall][Update] 调用 Flashlight.DrainAll()");
                    playerFlash.DrainAll(); // 清空电量
                }
                else
                {
                    Debug.Log("[DeathWall][Update] 没有 Flashlight 组件，直接调用 Die()");
                }
                
                // 确保始终调用死亡方法
                playerRef.Die();
                Debug.Log("[DeathWall][Update] 调用了 playerRef.Die()，玩家应当死亡");
                // ==== 关键修改结束 ====

                hasKilled = true;
            }
        }

        /// <summary>
        /// 通用 AABB 相交检测（此方法已被内联到 Update，用 Debug 输出更直观，保留供复用）：
        /// centerA/halfA：玩家的中心 + 半尺寸
        /// centerB/halfB：死亡墙 Tilemap 的中心 + 半尺寸
        /// </summary>
        private bool IsAABBIntersect(Vector2 centerA, Vector2 halfA, Vector2 centerB, Vector2 halfB)
        {
            bool overlapX = Mathf.Abs(centerA.x - centerB.x) <= (halfA.x + halfB.x);
            bool overlapY = Mathf.Abs(centerA.y - centerB.y) <= (halfA.y + halfB.y);
            return overlapX && overlapY;
        }

#if UNITY_EDITOR
        // 在 Scene 视图中绘制红色半透明方框，方便调试确认覆盖范围
        void OnDrawGizmosSelected()
        {
            // 如果还没 Awake，就用 transform.position + 默认半尺寸画个框
            Vector2 center = wallCenter;
            Vector2 half   = wallHalfSize;
            if (center == Vector2.zero && half == Vector2.zero)
            {
                center = transform.position;
                half   = new Vector2(1f, 1f);
            }

            Vector3 c3   = new Vector3(center.x, center.y, 0f);
            Vector3 size = new Vector3(half.x * 2, half.y * 2, 0f);
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawCube(c3, size);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(c3, size);
        }
#endif
    }
}
