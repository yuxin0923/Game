// Assets/Scripts/World/MovingPlatform.cs
using UnityEngine;
using Player;  // 假设 Player.cs 在这个命名空间

public class MovingPlatform : MonoBehaviour
{
    [Header("平台运动设置")]
    public Transform[] waypoints;
    public float speed = 2f;
    public float waitTime = 0.3f;

    [Header("平台物理体尺寸 (AABB)")]
    public Vector2 platformHalfSize = new Vector2(1f, 0.2f); // 平台宽度、高度的一半

    [Header("踩上平台要给玩家的材质 (Ice)")]
    public PhysicsMaterial iceMaterial;


    int idx = 0;
    float waitCounter = 0f;
    Vector3 lastPosition;
    bool playerOnPlatform = false;
    Player.Player player;      // 运行时找到第一个 Player 实例
    SimplePhysicsBody playerBody;

    void Start()
    {
        if (waypoints == null || waypoints.Length < 2)
            return;

        // 把平台搬到第一个点
        transform.position = waypoints[0].position;
        lastPosition = transform.position;

        // 尝试在场景中找一个 Player
        player = FindObjectOfType<Player.Player>();
        if (player != null)
            playerBody = player.GetComponent<SimplePhysicsBody>();
    }

    void FixedUpdate()
    {
        // —— 平台自身来回移动逻辑：与之前 OnTrigger 版一致 —— 
        if (waypoints == null || waypoints.Length < 2) return;
        if (waitCounter > 0f)
        {
            waitCounter -= Time.fixedDeltaTime;
        }
        else
        {
            Vector3 target = waypoints[idx].position;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.fixedDeltaTime);
            if (Vector3.Distance(transform.position, target) < 0.01f)
            {
                idx = (idx + 1) % waypoints.Length;
                waitCounter = waitTime;
            }
        }

        // —— 手动判断玩家是否踩在平台上 —— 
        if (player != null && playerBody != null)
        {
            Vector2 platformCenter = transform.position;
            Vector2 playerCenter = player.transform.position;
            Vector2 playerHalfSize = playerBody.HalfSize;

            bool isOverlap = CollisionDetector.AABBOverlap(
                platformCenter, platformHalfSize,
                playerCenter, playerHalfSize);

            if (isOverlap && !playerOnPlatform)
            {
                // 玩家“刚刚”踩到平台上
                playerOnPlatform = true;
                // ① 切换为冰面材质
                playerBody.SetSurfaceMaterial(iceMaterial);
                // ② 将玩家设为平台子对象，让玩家跟随平台运动
                player.transform.SetParent(transform);
            }
            else if (!isOverlap && playerOnPlatform)
            {
                // 玩家“刚刚”离开平台
                playerOnPlatform = false;
                // ① 恢复默认材质
                playerBody.ResetSurfaceMaterial();
                // ② 解除父子关系
                player.transform.SetParent(null);
            }
        }

        // —— 平台移动了多少，就让子对象（若有）跟着动；但由于我们是“SetParent”，Unity会自动同步子节点的位置 —— 
        //    因此这里就无需手动给 player 叠加平台移动增量
        lastPosition = transform.position;
    }

    // （可选）在 Scene 视图中绘制平台 AABB 帮助调试
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, platformHalfSize * 2f);
        if (waypoints != null && waypoints.Length > 1)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    int next = (i + 1) % waypoints.Length;
                    if (waypoints[next] != null)
                        Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
                }
            }
        }
    }
}
