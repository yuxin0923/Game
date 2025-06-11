using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class MovablePlatform : MonoBehaviour
{
    [Header("移动路径 (世界坐标)")]
    public Vector2[] points;            // 拐点数组，Size >= 2
    public float speed = 2f;            // 速度 (m/s)

    [Header("表面材质")]
    public PhysicsMaterial surfaceMaterial;

    // 以下两个会在 Start() 自动计算并公开给 PhysicsEngine 访问
    [HideInInspector] public Vector2 halfSize;
    [HideInInspector] public Vector2 centerOffset;

    // 内部状态
    int _nextIndex = 1;
    Vector2 _lastPos;
    Tilemap _tilemap;

    void Start()
    {

        Debug.Log($"[MovablePlatform] Start on {gameObject.name}");

        _tilemap = GetComponent<Tilemap>();
        // 把 cellBounds 收缩到紧贴所有 Tile
        _tilemap.CompressBounds();

        // 拿到 localBounds，并考虑 Transform.lossyScale
        Bounds lb = _tilemap.localBounds;
        Vector3 ls = transform.lossyScale;
        halfSize = new Vector2(lb.size.x * ls.x * 0.5f,
                               lb.size.y * ls.y * 0.5f);
        centerOffset = new Vector2(lb.center.x * ls.x,
                                   lb.center.y * ls.y);

        // 记录初始位置
        _lastPos = transform.position;
        PhysicsEngine.I.RegisterPlatform(this);
    }

    void OnDisable()
    {
        PhysicsEngine.I?.UnregisterPlatform(this);
    }

    /// <summary>在 PhysicsEngine.FixedUpdate 中被调用</summary>
    public void Tick(float dt)
    {
        Vector2 pos = transform.position;
        Vector2 dst = points[_nextIndex];
        Vector2 dir = (dst - pos).normalized;
        float step = speed * dt;

        if (Vector2.Distance(pos, dst) <= step)
        {
            transform.position = dst;
            _nextIndex = (_nextIndex + 1) % points.Length;
        }
        else
        {
            transform.position = pos + dir * step;
        }
    }

    /// <summary>本帧相对于上帧的位移</summary>
    public Vector2 MovementDelta => (Vector2)transform.position - _lastPos;

    /// <summary>在本帧所有逻辑结束后调用，更新 _lastPos</summary>
    public void LateTick() => _lastPos = transform.position;

    /// <summary>判定角色脚底 (feet) 与平台 AABB 是否重叠</summary>
    public bool IsOverlapping(Vector2 feet, Vector2 bodyHalf)
    {
        // 平台实际中心 = transform.position + centerOffset
        Vector2 platCenter = (Vector2)transform.position + centerOffset;
        return CollisionDetector.AABBOverlap(
            feet, bodyHalf,
            platCenter, halfSize
        );
        
    }

        #if UNITY_EDITOR
    // —— 把下面这一段，贴在这里 —— 
    void OnDrawGizmosSelected() {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.green;
        Vector2 center = (Vector2)transform.position + centerOffset;
        Gizmos.DrawWireCube(center, halfSize * 2f);
    }
    #endif

}
