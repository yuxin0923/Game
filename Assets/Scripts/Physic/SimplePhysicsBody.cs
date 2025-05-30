using UnityEngine;

public class SimplePhysicsBody : MonoBehaviour
{
    /* ─────────── 可调参数 ─────────── */
    [Tooltip("包围盒半尺寸（世界单位）")]
    public Vector2 halfSize = new(0.45f, 0.45f);

    [Tooltip("重力缩放系数 1 = 正常重力，0 = 漂浮")]
    public float gravityScale = 1f;

    /* ─────────── 运行时状态 ─────────── */
    [HideInInspector] public Vector2 velocity;    // 当前速度
    [HideInInspector] public bool    grounded;    // true = 站在地面

    const float STEP = 0.05f;                     // 穿隧分片尺寸

    /* ─────────── 生命周期 ─────────── */

    // Awake 里可放一些初始化（可选）
    void Awake() { }

    // ★ 改用 Start 注册，保证 PhysicsEngine 已经 Awake
    void Start()
    {
        PhysicsEngine.I.Register(this);
    }

    // 反注册仍写在 OnDisable，做空指针保护
    void OnDisable()
    {
        if (PhysicsEngine.I != null)
            PhysicsEngine.I.Unregister(this);
    }

    /// <summary>被 PhysicsEngine 每帧调用</summary>
    public void Tick(float dt)
    {
        // 1) 施加重力
        velocity += PhysicsEngine.I.gravity * gravityScale * dt;
        grounded = false;

        // 2) 先 X 再 Y 逐轴移动
        MoveAxis(ref velocity.x, Vector2.right, dt);
        MoveAxis(ref velocity.y, Vector2.up,    dt);
    }

    /* ─────────── 单轴移动 + 碰撞分离 ─────────── */
    void MoveAxis(ref float vAxis, Vector2 axis, float dt)
    {
        float move      = vAxis * dt;
        float direction = Mathf.Sign(move);
        float remaining = Mathf.Abs(move);

        while (remaining > 0f)
        {
            float step = Mathf.Min(STEP, remaining);
            Vector2 next = (Vector2)transform.position + axis * step * direction;

            if (Collides(next))
            {
                vAxis = 0f;                              // 撞到就清速度
                if (axis == Vector2.up && direction < 0) // 往下撞，视为落地
                    grounded = true;
                return;
            }

            transform.position = next;                   // 真正移动
            remaining         -= step;
        }
    }

    /* ─────────── 4 点采样 AABB vs Tilemap ─────────── */
    bool Collides(Vector2 pos)
    {
        Vector2 min = pos - halfSize;
        Vector2 max = pos + halfSize;

        return  TilemapWorld.I.IsSolid(new(min.x, min.y)) ||
                TilemapWorld.I.IsSolid(new(min.x, max.y)) ||
                TilemapWorld.I.IsSolid(new(max.x, min.y)) ||
                TilemapWorld.I.IsSolid(new(max.x, max.y));
    }

#if UNITY_EDITOR
    /* ─────────── Scene 视图可视化 ─────────── */
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, halfSize * 2f);
    }
#endif
}