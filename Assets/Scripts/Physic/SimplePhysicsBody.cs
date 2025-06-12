using UnityEngine;

///  Minimalist 2D AABB Rigid Body + Tilemap Collision (Doesn't rely on Unity's built-in physics)
[RequireComponent(typeof(Transform))]
public class SimplePhysicsBody : MonoBehaviour
{
    /* --- Size / Gravity --------------------------------------------------- */
    [Header("Size / Gravity")]
    public Vector2 halfSize = new(0.45f, 0.45f);
    public Vector2 HalfSize => halfSize;            // For AI / other scripts to sample

    public float gravity = -30f;

    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public bool    grounded;

    const float STEP = 0.05f;                       // Maximum fragment displacement per frame

    void Start()     => PhysicsEngine.I.Register(this);
    void OnDisable() { if (PhysicsEngine.I) PhysicsEngine.I.Unregister(this); }

    /* --- Physics Material ----------------------------------------------------- */
    [Header("Physics Material")]
    public PhysicsMaterial defaultMaterial;
    PhysicsMaterial currentMaterial;                // null -> use default

    /// <summary>External (e.g. moving platforms) can be forced to specify material</summary>
    public void SetSurfaceMaterial(PhysicsMaterial mat)
    {
        currentMaterial = mat ?? defaultMaterial;
    }

    /// <summary>Reset the material to default (usually called when leaving a platform)</summary>
    public void ResetSurfaceMaterial() => currentMaterial = defaultMaterial;

    /* --- Movement Dynamics Parameters ----------------------------------------------- */
    [Header("Movement Parameters")]
    [Tooltip("Maximum ground acceleration (m/s²)")]
    public float baseGroundAccel = 50f;

    float _moveInput; // [-1,1]
    float _reqSpeed;  // Specified by Player / AI

    /// <summary>Write horizontal inputs to the rigid body every frame</summary>
    public void SetMoveInput(float dir, float speed)
    {
        _moveInput = Mathf.Clamp(dir, -1f, 1f);
        _reqSpeed  = Mathf.Abs(speed);
    }

    /* ----------------------- Main Loop: Called by PhysicsEngine ------------- */
    public void Tick(float dt)
    {
        /* 1. Foot Sampling: Only when grounded and not externally forced */
        if (grounded)
        {
            Vector2 probe = (Vector2)transform.position - new Vector2(0, halfSize.y + 0.02f);
            var tileMat = TilemapWorld.I?.GetMaterial(probe);
            // If an external script (moving platform) has set a specific material in the previous frame, keep it
            if (currentMaterial == null || currentMaterial == defaultMaterial)
                currentMaterial = tileMat ?? defaultMaterial;
        }
        else
        {
            // Keep default material in the air to avoid being affected by the previous frame
            currentMaterial = defaultMaterial;
        }

        float friction = GetFriction();            // 0(Ice) ~ 1(Mud)

        /* 2. Horizontal Speed Smoothing ---------------------------------------------- */
        float targetSpeed = _moveInput * _reqSpeed;
        float accel       = Mathf.Lerp(10f, baseGroundAccel, friction);
        float curAccel    = grounded ? accel : accel * 0.3f; // Air thrust weakened
        velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, curAccel * dt);

        /* 3. Vertical Gravity ---------------------------------------------- */
        velocity.y += gravity * dt;
        grounded = false;                            // Will be re-evaluated in MoveAxis

        /* 4. Axis Movement & Collision ----------------------------------------- */
        MoveAxis(ref velocity.x, Vector2.right, dt);
        MoveAxis(ref velocity.y, Vector2.up,    dt);
    }

    /* ------------------------ Internal Tools ------------------------------- */
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

            // —— Here we check both tiles and platforms simultaneously ——
            if (HitTile(next) || HitPlatform(next))
            {
                // Revert to the position just before the collision
                while (HitTile(next) || HitPlatform(next))
                    next -= axis * 0.001f * dir;
                transform.position = next;

                // Reset velocity on wall hit or landing
                vAxis = 0f;
                // If moving downwards, mark as grounded
                if (axis == Vector2.up && dir < 0)
                    grounded = true;
                return;
            }

            // If you don't hit it, update your position and move on to the next small step.
            transform.position = next;
            rest -= step;
        }
    }

    /// <summary>
    /// Checks if the AABB at the next position (center=next, halfSize=this.halfSize)
    /// overlaps with any MovingPlatform.
    /// </summary>
    bool HitPlatform(Vector2 pos)
    {
        foreach (var p in PhysicsEngine.I.Platforms)
        {
            // Platform's actual center = transform.position + centerOffset
            Vector2 center = (Vector2)p.transform.position + p.centerOffset;
            Vector2 half   = p.halfSize;
            if (CollisionDetector.AABBOverlap(pos, halfSize, center, half))
                return true;
        }
        return false;
    }

    bool HitTile(Vector2 pos)
    {
        Vector2 min = pos - halfSize;
        Vector2 max = pos + halfSize;
        float midY = (min.y + max.y) * 0.5f;

        // Left 3 points
        if (IsSolid(min.x, min.y) || IsSolid(min.x, midY) || IsSolid(min.x, max.y)) return true;
        // Right 3 points
        if (IsSolid(max.x, min.y) || IsSolid(max.x, midY) || IsSolid(max.x, max.y)) return true;
        return false;
    }

    static bool IsSolid(float x, float y) => TilemapWorld.I.IsSolid(new Vector2(x, y));

    /// <summary>Compatible with old Enemy / Platform calls: can still directly change horizontal speed</summary>
    public void MoveHoriz(float dir, float speed) => SetMoveInput(dir, speed);

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, halfSize * 2);
    }
#endif
}
