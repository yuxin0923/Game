// Assets/Scripts/AIEnemy/AIEnemyManager.cs
using UnityEngine;
using AIEnemy;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(SimplePhysicsBody))]
public class AIEnemyManager : MonoBehaviour
{
    /* ------------ Inspector 参数 ------------ */
    [Header("===== 视距 & 玩家图层 =====")]
    public float sight = 6f;
    public LayerMask playerMask;   // 若需要射线，可自行使用

    [Header("===== 巡逻参数 =====")]
    public float patrolHalfDistance = 3f;
    public float patrolSpeed        = 2f;

    [Header("===== 追击 / 攻击 =====")]
    public float chaseSpeed  = 3.5f;
    public float attackRange = 1f;

    /* ------------ 运行时只读 ------------ */
    public EnemyState State { get; private set; } = EnemyState.Patrol;
    public Transform  PlayerTf    { get; private set; }
    public SimplePhysicsBody Body { get; private set; }

    /// 是否在“灯光 + 距离”探测范围内（可按需扩展为检测手电筒开关）
    public bool PlayerInSight =>
        PlayerTf &&
        (PlayerTf.position - transform.position).sqrMagnitude < sight * sight;

    /* ------------ 私有字段 ------------ */
    IEnemyStrategy current;
    StrategyFactory factory => StrategyFactory.Instance;

    SpriteRenderer sr;
    Animator       anim;

    /* ================= Unity 生命周期 ================= */
    void Awake()
    {
        Body = GetComponent<SimplePhysicsBody>();
        sr   = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        PlayerTf = FindObjectOfType<Player.Player>()?.transform;

        SwitchState(EnemyState.Patrol);  // 初始即巡逻
    }

    void Update()
    {
        if (current == null) return;

        bool done = current.Execute(this, Time.deltaTime);
        if (!done) return;

        /* ------ 简易状态转换表 ------ */
        EnemyState next = State switch
        {
            EnemyState.Patrol  => EnemyState.Chase,
            EnemyState.Chase   => (DistToPlayer() <= attackRange) ? EnemyState.Attack
                                                                  : EnemyState.Patrol,
            EnemyState.Attack  => (DistToPlayer() >  attackRange) ? EnemyState.Patrol
                                                                  : EnemyState.Attack,
            _                  => EnemyState.Dead
        };
        SwitchState(next);
    }

    /* =================== 动画封装 =================== */
    public void SetAnimMove(float vx) => anim?.SetFloat("Speed", Mathf.Abs(vx));
    public void SetAnimAttack()       => anim?.SetTrigger("Attack");
    public void SetAnimDead()         => anim?.SetTrigger("Dead");

    /* =================== 内部工具 =================== */
    void SwitchState(EnemyState to)
    {
        State   = to;
        current = factory.Get(to);

        /* --- 巡逻参数同步 & 初始化 --- */
        if (to == EnemyState.Patrol && current is PatrolStrategy patrol)
            patrol.Init(transform.position, patrolHalfDistance, patrolSpeed);

        /* --- 追击 / 攻击参数同步 --- */
        if (to == EnemyState.Chase && current is ChaseStrategy chase)
        {
            chase.speed       = chaseSpeed;
            chase.attackRange = attackRange;
        }
        if (to == EnemyState.Attack && current is AttackStrategy atk)
        {
            // 这里可同步 Inspector 的 cooldown / damage 等
        }

        /* --- 动画触发 --- */
        switch (to)
        {
            case EnemyState.Attack: SetAnimAttack(); break;
            case EnemyState.Dead:   SetAnimDead();   break;
        }
    }

    float DistToPlayer()
        => PlayerTf ? Vector2.Distance(transform.position, PlayerTf.position)
                    : float.PositiveInfinity;
}
