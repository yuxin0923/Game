// Assets/Scripts/AIEnemy/AIEnemyManager.cs
using UnityEngine;
using AIEnemy;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(SimplePhysicsBody))]
public class AIEnemyManager : MonoBehaviour
{
    [Header("=== Environment ===")]
    public LayerMask groundMask = ~0;   // 默认 Everything，可在 Inspector 调成 Ground

    /* ===== Inspector ===== */
    [Header("=== Detection ===")]
    [Tooltip("圆形感知半径 (m)")]
    public float sightRadius = 6f;
    [Tooltip("视野角度(°)，0 表示 360° 无死角")]
    [Range(0, 360)] public float fov = 120f;
    [Tooltip("探测间隔 (秒)")]
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

    /* ===== Private ===== */
    private IEnemyStrategy _current;
    private StrategyFactory _factory => StrategyFactory.Instance;

    private SpriteRenderer _sr;
    private Animator _anim;

    private bool _playerInSight;
    private float _detectTimer;
    // 新增：跟踪当前朝向，+1=面右, -1=面左
    private int facingDir = +1; 

    /* ================= Unity ================= */
    private void Awake()
    {
        Body = GetComponent<SimplePhysicsBody>();
        _sr = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();

        PlayerTf = FindObjectOfType<Player.Player>()?.transform;

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

        /* --- FSM --- */
        if (_current == null) return;
        if (!_current.Execute(this, Time.deltaTime)) return;

        EnemyState next = State switch
        {
            EnemyState.Patrol  => EnemyState.Chase,
            EnemyState.Chase   => (DistToPlayer() <= attackRange) ? EnemyState.Attack : EnemyState.Patrol,
            EnemyState.Attack  => (DistToPlayer() > attackRange)   ? EnemyState.Patrol : EnemyState.Attack,
            _                  => EnemyState.Dead
        };
        SwitchState(next);
    }

    /* ================= Detection Algorithm ================= */
    private bool DetectPlayer()
    {
        if (!PlayerTf) return false;

        Vector2 enemyPos = transform.position;
        Vector2 playerPos = PlayerTf.position;

        // 1. 距离圆
        if ((playerPos - enemyPos).sqrMagnitude > sightRadius * sightRadius)
            return false;

        // 2. 视野角
        if (fov > 0f && fov < 360f)
        {
            //Vector2 forward = Vector2.right * Mathf.Sign(transform.localScale.x);
            //Vector2 forward = Vector2.left * Mathf.Sign(transform.localScale.x);
            Vector2 forward = Vector2.right * facingDir;

            Vector2 dir = (playerPos - enemyPos).normalized;
            float cosHalf = Mathf.Cos(fov * 0.5f * Mathf.Deg2Rad);
            if (Vector2.Dot(forward, dir) < cosHalf)
                return false;
        }

        // 3. 瓦片 Raycast (Bresenham)
        return LineOfSight.Clear(enemyPos, playerPos);
    }

    /* ================= Internals ================= */
    private void SwitchState(EnemyState to)
    {
        State = to;
        _current = _factory.Get(to);

        // 同步参数
        if (to == EnemyState.Patrol && _current is PatrolStrategy patrol)
            patrol.Init(transform.position, patrolHalfDistance, patrolSpeed);

        if (to == EnemyState.Chase && _current is ChaseStrategy chase)
        {
            chase.speed = chaseSpeed;
            chase.attackRange = attackRange;
        }

        // 动画
        if (to == EnemyState.Attack) SetAnimAttack();
        if (to == EnemyState.Dead)   SetAnimDead();
    }

    private float DistToPlayer() => PlayerTf ? Vector2.Distance(transform.position, PlayerTf.position) : float.PositiveInfinity;

    /* === Animation helpers === */
    // 放在类里面原来 SetAnimMove 旁边
    // 修改 SetFacing：让它只负责翻贴图和更新 facingDir
    public void SetFacing(int dir)
    {
        if (dir == 0) return;           // 0 意味不动，可以保持原面朝
        facingDir = dir > 0 ? +1 : -1;  // 标准化成 +1 / -1

        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * facingDir; // localScale.x = ±原始宽度
        transform.localScale = s;
    }


    // 这里保留 SetAnimMove，但改为两个参数：水平速度 和 “是否在追击”
    public void SetAnimMove(float vx, bool isChasing)
    {
        _anim?.SetFloat("Speed", Mathf.Abs(vx));
        _anim?.SetBool("IsChasing", isChasing);

        // ★ 只要 vx 非 0，就调用 SetFacing(vx>0?+1:-1)
        if (Mathf.Abs(vx) > 0.001f)
        {
            SetFacing((int)Mathf.Sign(vx));
        }
    }

    public void SetAnimAttack()       => _anim?.SetTrigger("Attack");
    public void SetAnimDead()         => _anim?.SetTrigger("Dead");

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRadius);

        if (fov > 0f && fov < 360f)
        {
            Gizmos.color = Color.cyan;
            //Vector3 fwd = Vector3.right * Mathf.Sign(transform.localScale.x);
            //Vector3 fwd = Vector3.left * Mathf.Sign(transform.localScale.x);

            Vector3 fwd = Vector3.right * facingDir;

            Quaternion q1 = Quaternion.AngleAxis(+fov * 0.5f, Vector3.forward);
            Quaternion q2 = Quaternion.AngleAxis(-fov * 0.5f, Vector3.forward);
            Gizmos.DrawLine(transform.position, transform.position + q1 * fwd * sightRadius);
            Gizmos.DrawLine(transform.position, transform.position + q2 * fwd * sightRadius);
        }
    }
#endif
}
