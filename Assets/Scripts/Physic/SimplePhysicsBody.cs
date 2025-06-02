using UnityEngine;

/// 极简 2D AABB 刚体 + Tilemap 碰撞（不依赖 Unity 内置物理）
[RequireComponent(typeof(Transform))]
public class SimplePhysicsBody : MonoBehaviour
{
    [Header("尺寸 / 重力")]
    public Vector2 halfSize = new(0.45f, 0.45f);
    public float   gravity  = -30f;

    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public bool    grounded;

    const float STEP = 0.05f; // 单帧最大分片位移

    void Start()        => PhysicsEngine.I.Register(this);
    void OnDisable()    { if (PhysicsEngine.I) PhysicsEngine.I.Unregister(this); }

    /* -------------------- 主循环：由 PhysicsEngine 调用 -------------------- */
    public void Tick(float dt)
    {
        velocity.y += gravity * dt;
        grounded = false;

        MoveAxis(ref velocity.x, Vector2.right, dt); // 先水平
        MoveAxis(ref velocity.y, Vector2.up,    dt); // 再垂直
    }

    /* ------------------------ 下面都是内部工具 ---------------------------- */

    void MoveAxis(ref float vAxis, Vector2 axis, float dt)
    {
        float move = vAxis * dt;
        float dir  = Mathf.Sign(move);
        float rest = Mathf.Abs(move);

        while (rest > 0f)
        {
            float step = Mathf.Min(STEP, rest);
            Vector2 nextPos = (Vector2)transform.position + axis * step * dir;

            if (HitTile(nextPos))
            {
                while (HitTile(nextPos))                    // ← 用 nextPos 回退
                    nextPos -= axis * 0.001f * dir;

                transform.position = (Vector3)nextPos;      // ← 最终写回
                vAxis = 0f;
                if (axis == Vector2.up && dir < 0) grounded = true;
                return;
            }

            transform.position = nextPos;
            rest -= step;
        }

    }

    bool HitTile(Vector2 pos)
    {
        Vector2 min = pos - halfSize;
        Vector2 max = pos + halfSize;
        float midY = (min.y + max.y) * 0.5f;

        // 左边 3 点
        if (IsSolid(min.x, min.y) || IsSolid(min.x, midY) || IsSolid(min.x, max.y)) return true;
        // 右边 3 点
        if (IsSolid(max.x, min.y) || IsSolid(max.x, midY) || IsSolid(max.x, max.y)) return true;

        return false;
    }

    static bool IsSolid(float x, float y)
        => TilemapWorld.I.IsSolid(new Vector2(x, y));
    
    /// <summary>
    /// NEW
    /// 对外水平移动接口：dir = -1/0/+1
    /// 保持自身 Tick/碰撞逻辑不变
    /// </summary>
    public void MoveHoriz(float dir, float speed)
    {
        velocity.x = dir * speed;
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, halfSize * 2);
    }
#endif
}
