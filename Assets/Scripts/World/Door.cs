// Assets/Scripts/World/Door.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Player;   // 引用 Player 命名空间，以便访问 Player.Player
 // 引用你自写的物理引擎命名空间，以便访问 SimplePhysicsBody

namespace World
{
    /// <summary>
    /// Door：完全使用自写物理引擎的 AABB 检测来判断玩家是否“站在门前”并且钥匙足够，
    /// 一旦满足条件，就播放开门动画（如果有）并延时切换到下一个场景。
    ///
    /// 说明：
    /// - 本脚本不依赖任何 Unity Collider2D / Rigidbody2D。
    /// - “门”的碰撞区域用两个字段：doorCenter（中心坐标）和 doorHalfSize（半宽半高）来表示一个 AABB。
    /// - 玩家碰撞盒由 SimplePhysicsBody 提供的 halfSize 属性决定，中心由 transform.position 决定。
    /// - 当玩家的 AABB 与门的 AABB 相交，并且 player.keyCount >= requiredKeys 时，触发开门。
    /// </summary>
    public class Door : MonoBehaviour
    {
        [Header("—— 门的 AABB 区域（无需 Unity Collider） ——")]
        [Tooltip("门的中心坐标（世界坐标）。如果想让门跟随 GameObject 移动，可留空并在 Awake/Update 里同步 transform.position")]
        public Vector2 doorCenter = Vector2.zero;

        [Tooltip("门的半宽半高（世界单位）。门的总宽度 = doorHalfSize.x * 2，总高度 = doorHalfSize.y * 2")]
        public Vector2 doorHalfSize = new Vector2(1f, 2f);

        [Header("—— 钥匙 & 场景切换 ——")]
        [Tooltip("玩家需要持有的钥匙数量，才可打开此门")]
        public int requiredKeys = 1;

        [Tooltip("开门之后要加载的下一个场景名称（必须加入 Build Settings）")]
        public string nextSceneName;

        [Header("—— 开门动画（可选） ——")]
        [Tooltip("如果门有开／关两段动画，请挂上门物体的 Animator；若无动画可留空")]
        public Animator animator;

        [Tooltip("Animator 中控制开门的 bool 参数名称，默认为 “isOpen”。需与 Animator Controller 中的参数保持一致")]
        public string isOpenParam = "isOpen";

        [Tooltip("播放开门动画后等待多少秒再 LoadScene，保证动画播放完毕")]
        public float openToLoadDelay = 1.0f;

        // —— 私有字段 —— 
        private Player.Player playerRef;          // 场景中挂有 Player.Player 的玩家引用
        private SimplePhysicsBody playerBody;     // 玩家身上的 SimplePhysicsBody，以便获取 halfSize
        private bool hasOpened = false;           // 防止重复触发开门逻辑

        void Awake()
        {
            // 如果在 Inspector 中 doorCenter 没指定，就把它设为当前 GameObject 的位置：
            if (doorCenter == Vector2.zero)
            {
                doorCenter = transform.position;
            }
        }

        void Start()
        {
            // 1) 寻找场景中唯一的 Player.Player
            playerRef = FindObjectOfType<Player.Player>();
            if (playerRef == null)
            {
                Debug.LogError("[Door]：场景中找不到 Player.Player，请确认 Player 脚本已挂到玩家物体上。");
                return;
            }

            // 2) 获取 Player 上的 SimplePhysicsBody，以便拿到玩家碰撞盒的 halfSize
            playerBody = playerRef.GetComponent<SimplePhysicsBody>();
            if (playerBody == null)
            {
                Debug.LogError("[Door]：Player 身上没有挂 SimplePhysicsBody，请确认已挂载。");
            }

            // 3) 如果想用动画，一定要把 Animator 拖到 Inspector；否则会打 Warning
            if (animator == null)
            {
                Debug.LogWarning("[Door]：未指定 Animator，开门时不会播放动画，只会直接延时切换场景。");
            }
        }

        void Update()
        {
            // 如果已经打开过一次，无需再检测
            if (hasOpened) return;
            if (playerRef == null || playerBody == null) return;

            // —— 一：判断钥匙数量是否满足 —— 
            if (playerRef.keyCount < requiredKeys)
            {
                return;
            }

            // —— 二：更新 doorCenter 以保持跟随 GameObject —— 
            // 如果你希望门逻辑与这个脚本所在物体的位置同步，可以取消下面注释：
            // doorCenter = transform.position;

            // —— 三：构造玩家的 AABB —— 
            Vector2 playerCenter = playerRef.transform.position;
            Vector2 playerHalfSize = playerBody.halfSize;

            // —— 四：检测 AABB 相交 —— 
            if (IsAABBIntersect(playerCenter, playerHalfSize, doorCenter, doorHalfSize))
            {
                // 玩家与门的矩形区域相交且钥匙数量足够 → 开门
                TriggerOpenDoor();
            }
        }

        /// <summary>
        /// 判断两个 AABB（Axis‐Aligned Bounding Box）是否相交
        /// centerA / halfA：物体 A 的中心 + 半尺寸
        /// centerB / halfB：物体 B 的中心 + 半尺寸
        /// </summary>
        private bool IsAABBIntersect(Vector2 centerA, Vector2 halfA, Vector2 centerB, Vector2 halfB)
        {
            bool overlapX = Mathf.Abs(centerA.x - centerB.x) <= (halfA.x + halfB.x);
            bool overlapY = Mathf.Abs(centerA.y - centerB.y) <= (halfA.y + halfB.y);
            return overlapX && overlapY;
        }

        /// <summary>
        /// 触发“开门”逻辑：播放动画（如果有），然后延时加载下一个场景
        /// </summary>
        private void TriggerOpenDoor()
        {
            hasOpened = true;

            // 1) 播放开门动画（如果 Animator 非空）
            if (animator != null && !string.IsNullOrEmpty(isOpenParam))
            {
                animator.SetBool(isOpenParam, true);
            }

            // 2) 启动协程：等待动画播放完毕后再切场景
            StartCoroutine(OpenAndLoadCoroutine());
        }

        private IEnumerator OpenAndLoadCoroutine()
        {
            // 等待一段时间以保证动画完整播放。若不需要动画，可将 openToLoadDelay 设为 0。
            yield return new WaitForSeconds(openToLoadDelay);

            // 检查下一个场景名是否为空
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogError("[Door]：nextSceneName 为空，无法切换场景。请在 Inspector 中填写正确的场景名，并确保已添加到 Build Settings。");
                yield break;
            }

            // 日志提示
            Debug.Log($"[Door]：玩家钥匙数量≥{requiredKeys} 且已站在门前，正在加载场景 “{nextSceneName}” …");
            SceneManager.LoadScene(nextSceneName);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 在编辑器的 Scene 视图里画出门的 AABB 区域，方便调整和可视化。
        /// </summary>
        void OnDrawGizmosSelected()
        {
            // 门的中心：如果 doorCenter 未设过，就使用 transform.position
            Vector2 center = (doorCenter == Vector2.zero) ? (Vector2)transform.position : doorCenter;

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);                     // 半透明橙色表示填充
            Vector3 gizCenter = new Vector3(center.x, center.y, 0f);
            Vector3 gizSize = new Vector3(doorHalfSize.x * 2, doorHalfSize.y * 2, 0f);
            Gizmos.DrawCube(gizCenter, gizSize);

            Gizmos.color = Color.yellow;                                      // 黄色线条轮廓
            Gizmos.DrawWireCube(gizCenter, gizSize);
        }
#endif
    }
}
