using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class MovablePlatform : MonoBehaviour
{
    [Header("Movement path (world coordinates)")]
    public Vector2[] points;            // Array of waypoints, Size >= 2
    public float speed = 2f;            // Speed (m/s)

    [Header("Surface Material")]
    public PhysicsMaterial surfaceMaterial;

    // The following two will be automatically calculated in Start() and exposed for PhysicsEngine access
    [HideInInspector] public Vector2 halfSize;
    [HideInInspector] public Vector2 centerOffset;

    // Internal state
    int _nextIndex = 1;
    Vector2 _lastPos;
    Tilemap _tilemap;

    void Start()
    {

        Debug.Log($"[MovablePlatform] Start on {gameObject.name}");

        _tilemap = GetComponent<Tilemap>();
        // Shrink cellBounds to fit all Tiles
        _tilemap.CompressBounds();

        // Get localBounds and consider Transform.lossyScale
        Bounds lb = _tilemap.localBounds;
        Vector3 ls = transform.lossyScale;
        halfSize = new Vector2(lb.size.x * ls.x * 0.5f,
                               lb.size.y * ls.y * 0.5f);
        centerOffset = new Vector2(lb.center.x * ls.x,
                                   lb.center.y * ls.y);

        // Record the initial position
        _lastPos = transform.position;
        PhysicsEngine.I.RegisterPlatform(this);
    }

    void OnDisable()
    {
        PhysicsEngine.I?.UnregisterPlatform(this);
    }

    /// <summary>Called in PhysicsEngine.FixedUpdate</summary>
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

    /// <summary>Movement delta this frame relative to the last frame</summary>
    public Vector2 MovementDelta => (Vector2)transform.position - _lastPos;

    /// <summary>Called after all logic in this frame, updates _lastPos</summary>
    public void LateTick() => _lastPos = transform.position;

    /// <summary>Determines if the character's feet are overlapping with the platform's AABB</summary>
    public bool IsOverlapping(Vector2 feet, Vector2 bodyHalf)
    {
        // Platform's actual center = transform.position + centerOffset
        Vector2 platCenter = (Vector2)transform.position + centerOffset;
        return CollisionDetector.AABBOverlap(
            feet, bodyHalf,
            platCenter, halfSize
        );
        
    }

        #if UNITY_EDITOR
  
    void OnDrawGizmosSelected() {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.green;
        Vector2 center = (Vector2)transform.position + centerOffset;
        Gizmos.DrawWireCube(center, halfSize * 2f);
    }
    #endif

}
