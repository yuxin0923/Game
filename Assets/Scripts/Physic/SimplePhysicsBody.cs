using UnityEngine;

/// 极简 2D AABB 刚体 + Tilemap 碰撞（不依赖 Unity 内置物理）
[RequireComponent(typeof(Transform))]
public class SimplePhysicsBody : MonoBehaviour
{
    /* --- 尺寸 / 重力 --------------------------------------------------- */
    [Header("尺寸 / 重力")]
    public Vector2 halfSize = new(0.45f, 0.45f);
    public Vector2 HalfSize => halfSize;            // 供 AI / 其他脚本采样

    public float gravity = -30f;

    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public bool    grounded;

    const float STEP = 0.05f;                       // 单帧最大分片位移

    void Start()     => PhysicsEngine.I.Register(this);
    void OnDisable() { if (PhysicsEngine.I) PhysicsEngine.I.Unregister(this); }

    /* --- 物理材质 ----------------------------------------------------- */
    [Header("物理材质")]
    public PhysicsMaterial defaultMaterial;
    PhysicsMaterial currentMaterial;                // null -> 用 default

    /// <summary>外部（如移动平台）可强行指定材质</summary>
    public void SetSurfaceMaterial(PhysicsMaterial mat)
    {
        currentMaterial = mat ?? defaultMaterial;
    }

    /// <summary>让材质回到默认（通常在离开平台时调用）</summary>
    public void ResetSurfaceMaterial() => currentMaterial = defaultMaterial;

    /* --- 移动动力学参数 ----------------------------------------------- */
    [Header("移动参数")]
    [Tooltip("地面最大加速度 (m/s²)")]
    public float baseGroundAccel = 50f;

    float _moveInput; // [-1,1]
    float _reqSpeed;  // 由 Player / AI 指定

    /// <summary>每帧把水平输入写进刚体</summary>
    public void SetMoveInput(float dir, float speed)
    {
        _moveInput = Mathf.Clamp(dir, -1f, 1f);
        _reqSpeed  = Mathf.Abs(speed);
    }

    /* ----------------------- 主循环：由 PhysicsEngine 调用 ------------- */
    public void Tick(float dt)
    {
        /* 1. 脚底采样：仅在本身 grounded 且没被外部强制材质时进行 */
        if (grounded)
        {
            Vector2 probe = (Vector2)transform.position - new Vector2(0, halfSize.y + 0.02f);
            var tileMat = TilemapWorld.I?.GetMaterial(probe);
            // 若外部脚本(移动平台)已在上一帧设定特定材质，则优先保持
            if (currentMaterial == null || currentMaterial == defaultMaterial)
                currentMaterial = tileMat ?? defaultMaterial;
        }
        else
        {
            // 空中保持默认材质，以免被上一帧残留影响
            currentMaterial = defaultMaterial;
        }

        float friction = GetFriction();            // 0(冰) ~ 1(泥)

        /* 2. 水平速度渐变 ---------------------------------------------- */
        float targetSpeed = _moveInput * _reqSpeed;
        float accel       = Mathf.Lerp(10f, baseGroundAccel, friction);
        float curAccel    = grounded ? accel : accel * 0.3f; // 空中推力减弱
        velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, curAccel * dt);

        /* 3. 垂直方向重力 ---------------------------------------------- */
        velocity.y += gravity * dt;
        grounded = false;                            // 将在 MoveAxis 中重新判定

        /* 4. 分轴移动 & 碰撞 ----------------------------------------- */
        MoveAxis(ref velocity.x, Vector2.right, dt);
        MoveAxis(ref velocity.y, Vector2.up,    dt);
    }

    /* ------------------------ 内部工具 ------------------------------- */
    float GetFriction() => currentMaterial ? currentMaterial.friction : 0.5f;

    void MoveAxis(ref float vAxis, Vector2 axis, float dt)
    {
        float move = vAxis * dt;
        float dir  = Mathf.Sign(move);
        float rest = Mathf.Abs(move);

        while (rest > 0f)
        {
            float step = Mathf.Min(STEP, rest);
            Vector2 next = (Vector2)transform.position + axis * step * dir;

            if (HitTile(next))
            {
                while (HitTile(next)) next -= axis * 0.001f * dir;
                transform.position = next;
                vAxis = 0f;
                if (axis == Vector2.up && dir < 0) grounded = true;
                return;
            }

            transform.position = next;
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

    static bool IsSolid(float x, float y) => TilemapWorld.I.IsSolid(new Vector2(x, y));

    /// <summary>兼容旧 Enemy / Platform 调用：仍可直接更改水平速度</summary>
    public void MoveHoriz(float dir, float speed) => SetMoveInput(dir, speed);

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, halfSize * 2);
    }
#endif
}
