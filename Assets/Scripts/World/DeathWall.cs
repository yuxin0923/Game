using UnityEngine;
using UnityEngine.Tilemaps;  // 用到 Tilemap 组件
using Player;              // 引用 Player.Player（请确保 Player.cs 里的命名空间是 Player）
                 // 引用 SimplePhysicsBody（请确保它在 Physic 命名空间下）

namespace World
{
    /// <summary>
    /// DeathWall：当玩家与“挂在同一 GameObject 上的 Tilemap（deathwall）”的 AABB 区域重叠时
    /// 先把手电量清零（如果有 Flashlight），再调用 Player.Die()（延迟/禁用方式或直接销毁都可）。
    ///
    /// 使用方法：将此脚本挂到“deathwall”这个 Tilemap GameObject 上即可。
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
                Debug.LogError("[DeathWall]：在 Awake 中找不到 Tilemap 组件，请确认挂载在带 Tilemap 的 GameObject 上。");
                return;
            }
            tm.CompressBounds();          // ← 加这一句

            // 2) 读取 Tilemap 的本地 Bounds（localBounds），然后把它转为世界坐标下的 AABB
            // localBounds.center 是 Tilemap 本地坐标下的中心（以格子为单位）
            // localBounds.size   是 Tilemap 本地坐标下的宽度/高度（以格子为单位）
            Bounds localBounds = tm.localBounds;

            // 把本地中心转换到世界坐标
            Vector3 worldCenter3 = tm.transform.TransformPoint(localBounds.center);
            // 把本地大小（size）按 lossyScale 转换到世界大小
            Vector3 worldSize3   = Vector3.Scale(localBounds.size, tm.transform.lossyScale);

            // 只取 X/Y 分量
            wallCenter   = new Vector2(worldCenter3.x, worldCenter3.y);
            wallHalfSize = new Vector2(worldSize3.x * 0.5f, worldSize3.y * 0.5f);
        }

        void Start()
        {
            // 初始化玩家引用
            playerRef = FindObjectOfType<Player.Player>();
            if (playerRef == null)
            {
                Debug.LogError("[DeathWall]：Start 中找不到 Player.Player，请确认场景中有一个挂了 Player 脚本的对象。");
                return;
            }

            playerBody = playerRef.GetComponent<SimplePhysicsBody>();
            if (playerBody == null)
            {
                Debug.LogError("[DeathWall]：Start 中在 Player 上找不到 SimplePhysicsBody 组件，请确认已挂好。");
            }

            playerFlash = playerRef.GetComponent<Flashlight>();
            if (playerFlash == null)
            {
                // 没有 Flashlight 也行，只是没法预先清零电量
                Debug.LogWarning("[DeathWall]：Player 身上没有 Flashlight 组件，会直接调用 Die() 而不先 DrainAll()。");
            }
        }

        void Update()
        {
            // 如果已经触发一次死亡，就不再重复判断
            if (hasKilled) return;
            if (playerRef == null || playerBody == null) return;

            // 3) 构造玩家 AABB：中心 + 半尺寸
            Vector2 pCenter = playerRef.transform.position;
            Vector2 pHalf   = playerBody.halfSize;

            // 4) 判断是否与 Tilemap AABB 重叠
            if (IsAABBIntersect(pCenter, pHalf, wallCenter, wallHalfSize))
            {
                // 5) 如果挂了 Flashlight，先把电量清零
                if (playerFlash != null)
                    playerFlash.DrainAll();

                // 6) 调用新的 Die()（该方法中会做“禁用+延迟销毁”处理）
                playerRef.Die();

                hasKilled = true;  // 只触发一次
            }
        }

        /// <summary>
        /// 通用 AABB 相交检测：
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

            Vector3 c3 = new Vector3(center.x, center.y, 0f);
            Vector3 size3 = new Vector3(half.x * 2, half.y * 2, 0f);
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawCube(c3, size3);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(c3, size3);
        }
#endif
    }
}
