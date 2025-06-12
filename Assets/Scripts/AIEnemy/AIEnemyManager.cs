// Assets/Scripts/AIEnemy/AIEnemyManager.cs
/*
========================================================================================================================
AIEnemyManager.cs — Behaviour Orchestrator for 2‑D Enemies
========================================================================================================================
Author: XinYu
Unity Version: 2021‑LTS +

OVERVIEW
--------
AIEnemyManager is the single, high‑level “brain” component attached to every enemy prefab. Its job is **not** to implement the
behaviour logic itself (walk, chase, attack…) but to *orchestrate* those behaviours, acting as a lightweight finite‑state
machine (FSM) that decides *when* the enemy should patrol, chase, or attack, and then delegates *how* to do it to a
pluggable **strategy** object.  Think of it as the Context in the **Strategy Pattern**:

    AIEnemyManager (Context)  →  IEnemyStrategy (Strategy interface)
                                   ↑          ↑           ↑
                                   │          │           └── AttackStrategy
                                   │          └── ChaseStrategy
                                   └── PatrolStrategy

Creation of those concrete strategies is centralised inside **StrategyFactory** (a minimal **Factory Method** helper), and each
instance is cached in the private dictionary `_strategyCache` so that allocating a behaviour the second time costs zero GC.

The manager also talks to several *Unity* components sitting on the same GameObject:

    • SpriteRenderer & Animator  – cosmetic changes: flipping sprites, blending animations.
    • SimplePhysicsBody          – a custom Kinematic Rigidbody wrapper that actually moves the enemy.
    • Gizmos (editor‑only)       – visualises FOV & perception radius for fast tuning.

In play‑time it needs a reference to the **Player** `Transform` (found automatically in Awake) and a static helper
`LineOfSight.Clear` that performs a tile‑by‑tile raycast.


-----------------------------------------------------------------------------------------------------------
PUBLIC API (the parts other scripts may touch)
-----------------------------------------------------------------------------------------------------------
    EnemyState   State            – current high‑level state (readonly)
    Transform    PlayerTf         – lazy auto‑found reference to the player (can be overridden)
    bool         PlayerInSight    – last LOS result (useful for UI‑debug or alarms)
    int          Facing           – graphic facing direction (+1=right, ‑1=left)

    void         SetFacing(int)   – force a new facing; automatically mirrors the sprite
    void         SetAnimMove(...) – helper invoked by strategies so they don’t need to poke Animator directly
    void         SetAnimAttack()  – triggers -> "Attack" in the Animator state‑machine
    void         SetAnimDead()    – triggers -> "Dead", used by DeathStrategy (not shown here)
    void         SetAnimCatchPlayer() – optional cinematic when catching the player

Nothing else should be called from the outside; all AI decisions live here.

-----------------------------------------------------------------------------------------------------------
HOW DOES IT TALK TO OTHER SCRIPTS?
-----------------------------------------------------------------------------------------------------------
• **Strategies (PatrolStrategy, ChaseStrategy, AttackStrategy …)**
    The manager passes itself to `Execute(this, deltaTime)` every frame.  Strategies may in turn call
    SetAnimMove / SetFacing / Body.Move(...) etc. but they *never* modify the FSM.

• **Player.Player** (or any player controller)
    Only read‑only access: distance calculation and LOS checks.  No coupling beyond `Transform`.

• **LineOfSight** (static helper)
    Used exclusively inside `DetectPlayer()` to check whether terrain tiles block vision.

• **SimplePhysicsBody**
    Provides `Move(float vx, float vy)` & physics queries.  Strategies issue movement commands here rather than using
    `Rigidbody2D` directly to keep the physics layer swappable.

• **Animator**
    Receives primitive flags & triggers so the artist remains free to change animation graphs without touching code.

Design Patterns Recap
---------------------
    1. **Finite‑State Machine (FSM)** – top‑level behavioural flow.
    2. **Strategy**                  – decouples *what* is done in a state from the manager.
    3. **Factory Method**            – hides construction logic of concrete strategies.
    4. (Minor) **Lazy Initialisation** – strategies are created on first use and cached.

USAGE NOTES & EXTENSIBILITY
---------------------------
• Add a new behaviour → Derive from `IEnemyStrategy`, implement `Execute`, then extend `EnemyState` enum & `StrategyFactory`.
• Tweak detection → play with `sightRadius`, `fov`, `groundMask` and `detectInterval` in the Inspector.
• To support 3‑D → Replace `LineOfSight` and `SimplePhysicsBody` with 3‑D versions; FSM remains intact.
• Networking → Make `State` authoritative and replicate to clients; strategies run only on the server.

*/

// =========================================================================================================
// Original Code Starts Here — untouched except for the comment above
// =========================================================================================================


using UnityEngine;
using AIEnemy;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(SimplePhysicsBody))]
public class AIEnemyManager : MonoBehaviour
{
    [Header("=== Environment ===")]
    public LayerMask groundMask = ~0;   

    /* ===== Inspector ===== */
    [Header("=== Detection ===")]
    [Tooltip("Circular Perception Radius(m)")]
    public float sightRadius = 6f;
    [Tooltip("Field of view angle (°), 0 means 360° no dead angle")]
    [Range(0, 360)] public float fov = 120f;
    [Tooltip("Detection Interval (seconds)")]
    [Min(0.02f)] public float detectInterval = 0.2f;

    [Header("=== Patrol ===")]
    public float patrolHalfDistance = 3f;
    public float patrolSpeed = 2f;

    [Header("=== Chase / Attack ===")]
    public float chaseSpeed = 3.5f;
    public float attackRange = 1f;

    /* ===== Runtime ===== */
    public EnemyState State { get; private set; } = EnemyState.Patrol;
    public Transform PlayerTf { get; private set; }
    public SimplePhysicsBody Body { get; private set; }

    public bool PlayerInSight => _playerInSight;
    
    public int Facing { get; private set; } = -1;  

    /* ===== Private ===== */
    private IEnemyStrategy _current;
    // private StrategyFactory _factory => StrategyFactory.Instance;
    private StrategyFactory _factory = new StrategyFactory(); 
    private Dictionary<EnemyState, IEnemyStrategy> _strategyCache; 

    private SpriteRenderer _sr;
    private Animator _anim;

    private bool _playerInSight;
    private float _detectTimer;
    
    private int facingDir = -1; 
    private float _baseScaleX;     
    private int   _graphicDir;     

    /* ================= Unity ================= */
    private void Awake()
    {
        Body = GetComponent<SimplePhysicsBody>();
        _sr = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
       
        _baseScaleX = transform.localScale.x;
        _graphicDir = _baseScaleX >= 0 ? +1 : -1;


        PlayerTf = FindObjectOfType<Player.Player>()?.transform;

        // Initializing the Policy Cache
        _strategyCache = new Dictionary<EnemyState, IEnemyStrategy>();
        SwitchState(EnemyState.Patrol);
    }

private void Update()
{
    /* --- Detection --- */
    _detectTimer += Time.deltaTime;
    if (_detectTimer >= detectInterval)
    {
        _detectTimer = 0f;
        _playerInSight = DetectPlayer();
    }

    /* ----------------------------------- FSM --------------------------------- */
    if (_current == null) return;
    
    // Calculate true distance (including vertical component)
    float dist = PlayerTf ? Vector2.Distance(transform.position, PlayerTf.position) : float.PositiveInfinity;
    
    // State switching logic (priority: Attack > Chase > Patrol）
    EnemyState next = State;
    switch (State)
    {
        case EnemyState.Patrol:
            if (_playerInSight) next = EnemyState.Chase;
            break;
            
        case EnemyState.Chase:
            // Conditions for direct access to the attack state (ignoring cooldowns)
            if (dist <= attackRange) 
            {
                next = EnemyState.Attack;
            }
            // Player out of view or too far away
            else if (!_playerInSight || dist > sightRadius)
            {
                next = EnemyState.Patrol;
            }
            break;
            
        case EnemyState.Attack:
            // Player is out of attack range (with buffer) → Back to Chase
            if (dist > attackRange * 1.2f) 
            {
                next = EnemyState.Chase;
            }
            // Player leaves the field of view → Back to Patrol
            else if (!_playerInSight)
            {
                next = EnemyState.Patrol;
            }
            break;
    }
    
    // Immediate state switching
    if (next != State)
    {
        SwitchState(next);
    }

    // Execute current state logic
    _current.Execute(this, Time.deltaTime);
}


    /* ================================ Detection Algorithm ========================= */
    private bool DetectPlayer()
    {
        if (!PlayerTf) return false;

        Vector2 enemyPos = transform.position;
        Vector2 playerPos = PlayerTf.position;

        // 1. Distance from the circle
        if ((playerPos - enemyPos).sqrMagnitude > sightRadius * sightRadius)
            return false;

        // 2. Field of view angle
        if (fov > 0f && fov < 360f)
        {
            Vector2 forward = Vector2.right * facingDir;

            Vector2 dir = (playerPos - enemyPos).normalized;
            float cosHalf = Mathf.Cos(fov * 0.5f * Mathf.Deg2Rad);
            if (Vector2.Dot(forward, dir) < cosHalf)
                return false;
        }

        // 3. Tile Raycast (Bresenham)
        return LineOfSight.Clear(enemyPos, playerPos);
    }

    /* ================= Internals ================= */
    private void SwitchState(EnemyState to)
    {
        State = to;
        
        // Get or create a policy instance
        if (!_strategyCache.TryGetValue(to, out _current))
        {
            _current = _factory.Create(to);
            _strategyCache.Add(to, _current);
        }

        // Synchronize parameters
        if (to == EnemyState.Patrol && _current is PatrolStrategy patrol)
            patrol.Init(transform.position, patrolHalfDistance, patrolSpeed);

        if (to == EnemyState.Chase && _current is ChaseStrategy chase)
        {
            chase.speed = chaseSpeed;
            chase.attackRange = attackRange;
        }

        // Animation
        if (to == EnemyState.Attack) SetAnimAttack();
        if (to == EnemyState.Dead)   SetAnimDead();
    }

    private float DistToPlayer() => PlayerTf ? Vector2.Distance(transform.position, PlayerTf.position) : float.PositiveInfinity;

    public void SetFacing(int dir)
    {
        if (dir == 0) return;          // 0 = maintain original orientation
        Facing = dir > 0 ? +1 : -1;    // Update Facing property
        facingDir = Facing;             // Keep in sync with existing field

        Vector3 s  = transform.localScale;
        // Formula: Absolute value × Movement direction × Default graphic direction
        s.x = Mathf.Abs(_baseScaleX) * Facing * _graphicDir;
        transform.localScale = s;
    }

    // two parameters: horizontal speed and "is chasing"
    public void SetAnimMove(float vx, bool isChasing)
    {
        _anim?.SetFloat("Speed", Mathf.Abs(vx));
        _anim?.SetBool("IsChasing", isChasing);

        //  As long as vx is not 0, call SetFacing(vx>0?+1:-1)
        if (Mathf.Abs(vx) > 0.001f)
        {
            SetFacing((int)Mathf.Sign(vx));
        }
    }

    public void SetAnimAttack()       => _anim?.SetTrigger("Attack");
    public void SetAnimDead()         => _anim?.SetTrigger("Dead");

    // Assets/Scripts/AIEnemy/AIEnemyManager.cs
    public void SetAnimCatchPlayer() => _anim?.SetTrigger("CatchPlayer");

    

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRadius);

        if (fov > 0f && fov < 360f)
        {
            Gizmos.color = Color.cyan;
            Vector3 fwd = Vector3.right * facingDir;

            Quaternion q1 = Quaternion.AngleAxis(+fov * 0.5f, Vector3.forward);
            Quaternion q2 = Quaternion.AngleAxis(-fov * 0.5f, Vector3.forward);
            Gizmos.DrawLine(transform.position, transform.position + q1 * fwd * sightRadius);
            Gizmos.DrawLine(transform.position, transform.position + q2 * fwd * sightRadius);
        }
    }
#endif
}
