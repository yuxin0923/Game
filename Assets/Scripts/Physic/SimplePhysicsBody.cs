using UnityEngine;

/// 极简 2D AABB 刚体 + Tilemap 碰撞（不依赖 Unity 内置物理）
[RequireComponent(typeof(Transform))]
public class SimplePhysicsBody : MonoBehaviour
{
    [Header("尺寸 / 重力")]
    public Vector2 halfSize = new(0.45f, 0.45f);
        // ← 在这里新增一行属性，不要重复声明 halfSize
    public Vector2 HalfSize => halfSize;
    
    public float gravity = -30f;

    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public bool    grounded;

    const float STEP = 0.05f; // 单帧最大分片位移

    void Start()        => PhysicsEngine.I.Register(this);
    void OnDisable()    { if (PhysicsEngine.I) PhysicsEngine.I.Unregister(this); }
    


    [Header("物理材质")]
    public PhysicsMaterial defaultMaterial;
    private PhysicsMaterial currentMaterial;

    // 设置当前表面材质
    public void SetSurfaceMaterial(PhysicsMaterial material)
    {
        currentMaterial = material;
    }

    // 重置为默认材质
    public void ResetSurfaceMaterial()
    {
        currentMaterial = defaultMaterial;
    }

    // 获取当前摩擦系数
    private float GetFriction()
    {
        return currentMaterial != null ? currentMaterial.friction : 
            defaultMaterial != null ? defaultMaterial.friction : 0.5f;
    }

    /* -------------------- 主循环：由 PhysicsEngine 调用 -------------------- */
    public void Tick(float dt)
    {

        // 应用摩擦力(新加的)
            // 应用摩擦力
        if (grounded && Mathf.Abs(velocity.x) > 0.01f)
        {
            float friction = GetFriction();
            velocity.x = Mathf.Lerp(velocity.x, 0, friction * dt * 10);
        }

        
        velocity.y += gravity * dt;
        grounded = false;

        MoveAxis(ref velocity.x, Vector2.right, dt); // 先水平
        MoveAxis(ref velocity.y, Vector2.up, dt); // 再垂直
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