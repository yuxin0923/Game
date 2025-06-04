// Assets/Scripts/AIEnemy/AIEnemyManager.cs
using UnityEngine;
using AIEnemy;
using System.Collections.Generic;

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
    // 新增：公共Facing属性
    public int Facing { get; private set; } = -1;  // +1=面右, -1=面左

    /* ===== Private ===== */
    private IEnemyStrategy _current;
    // private StrategyFactory _factory => StrategyFactory.Instance;
    private StrategyFactory _factory = new StrategyFactory(); // 每个敌人自己的工厂
    private Dictionary<EnemyState, IEnemyStrategy> _strategyCache; // 缓存策略实例

    private SpriteRenderer _sr;
    private Animator _anim;

    private bool _playerInSight;
    private float _detectTimer;
    // 新增：跟踪当前朝向，+1=面右, -1=面左
    private int facingDir = -1; 
    private float _baseScaleX;     // 记录原始 x 缩放
    private int   _graphicDir;     // 贴图默认朝向：右=+1，左=-1

    /* ================= Unity ================= */
    private void Awake()
    {
        Body = GetComponent<SimplePhysicsBody>();
        _sr = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
        /* === 关键两行 === */
        _baseScaleX = transform.localScale.x;
        _graphicDir = _baseScaleX >= 0 ? +1 : -1;


        PlayerTf = FindObjectOfType<Player.Player>()?.transform;

        // 初始化策略缓存
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

    /* --- FSM --- */
    if (_current == null) return;
    
    // 计算真实距离（包含垂直分量）
    float dist = PlayerTf ? Vector2.Distance(transform.position, PlayerTf.position) : float.PositiveInfinity;
    
    // 状态切换逻辑（优先级：Attack > Chase > Patrol）
    EnemyState next = State;
    switch (State)
    {
        case EnemyState.Patrol:
            if (_playerInSight) next = EnemyState.Chase;
            break;
            
        case EnemyState.Chase:
            // 直接进入攻击状态的条件（忽略冷却）
            if (dist <= attackRange) 
            {
                next = EnemyState.Attack;
            }
            // 玩家离开视野或太远
            else if (!_playerInSight || dist > sightRadius)
            {
                next = EnemyState.Patrol;
            }
            break;
            
        case EnemyState.Attack:
            // 玩家超出攻击范围(带缓冲) → 回Chase
            if (dist > attackRange * 1.2f) 
            {
                next = EnemyState.Chase;
            }
            // 玩家离开视野 → 回Patrol
            else if (!_playerInSight)
            {
                next = EnemyState.Patrol;
            }
            break;
    }
    
    // 立即切换状态
    if (next != State)
    {
        SwitchState(next);
    }
    
    // 执行当前状态逻辑
    _current.Execute(this, Time.deltaTime);
}

    // private void Update()
    // {
    //     /* --- Detection --- */
    //     _detectTimer += Time.deltaTime;
    //     if (_detectTimer >= detectInterval)
    //     {
    //         _detectTimer = 0f;
    //         _playerInSight = DetectPlayer();
    //     }

    //     /* --- FSM --- */
    //     if (_current == null) return;
    //     if (!_current.Execute(this, Time.deltaTime))
    //         return;

    //     // 先计算“当前玩家是否仍在可攻击范围内”
    //     float dist = 0f;
    //     if (PlayerTf != null)
    //         dist = Mathf.Abs(PlayerTf.position.x - transform.position.x);
    //         // 优化状态切换逻辑
    //     EnemyState next = State switch
    //     {
    //         EnemyState.Patrol => _playerInSight ? EnemyState.Chase : EnemyState.Patrol,
            
    //         EnemyState.Chase => (_playerInSight && DistToPlayer() <= attackRange) 
    //             ? EnemyState.Attack 
    //             : EnemyState.Patrol,
            
    //         EnemyState.Attack => (_playerInSight && DistToPlayer() <= attackRange * 1.2f) 
    //             ? EnemyState.Attack 
    //             : EnemyState.Chase,
                
    //         _ => EnemyState.Dead
    //     };
        
    //     // 立即切换状态（无冷却）
    //     if (next != State)
    //     {
    //         SwitchState(next);
    //     }
    // }

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
        
        // 获取或创建策略实例
        if (!_strategyCache.TryGetValue(to, out _current))
        {
            _current = _factory.Create(to);
            _strategyCache.Add(to, _current);
        }

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
    // 修改 SetFacing：让它只负责翻贴图和更新 facingDir    /* ---------- 修改 SetFacing ---------- */
    // public void SetFacing(int dir)
    // {
    //     if (dir == 0) return;          // 0 = 保持原朝向
    //     facingDir = dir > 0 ? +1 : -1; // +1=向右，-1=向左

    //     Vector3 s  = transform.localScale;
    //     // 公式：绝对值 × 移动方向 × 贴图默认朝向
    //     s.x = Mathf.Abs(_baseScaleX) * facingDir * _graphicDir;
    //     transform.localScale = s;
    // }
    public void SetFacing(int dir)
    {
        if (dir == 0) return;          // 0 = 保持原朝向
        Facing = dir > 0 ? +1 : -1;    // 更新Facing属性
        facingDir = Facing;             // 保持与现有字段的同步

        Vector3 s  = transform.localScale;
        // 公式：绝对值 × 移动方向 × 贴图默认朝向
        s.x = Mathf.Abs(_baseScaleX) * Facing * _graphicDir;
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
