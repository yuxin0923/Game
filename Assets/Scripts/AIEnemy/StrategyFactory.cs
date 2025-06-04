// Assets/Scripts/AI/StrategyFactory.cs
using AIEnemy;

namespace AIEnemy
{
    //     /// <summary>简单单例工厂：按状态返回同一个策略实例，避免 GC</summary>
    //     public sealed class StrategyFactory
    //     {
    //         public static StrategyFactory Instance { get; } = new StrategyFactory();
    //         private StrategyFactory() { }

    //         readonly PatrolStrategy  patrol  = new();
    //         readonly ChaseStrategy   chase   = new();
    //         readonly AttackStrategy  attack  = new();
    //         readonly DeadStrategy    dead    = new();

    //         public IEnemyStrategy Get(EnemyState s) => s switch
    //         {
    //             EnemyState.Patrol  => patrol,
    //             EnemyState.Chase   => chase,
    //             EnemyState.Attack  => attack,
    //             _                  => dead
    //         };
    //     }


    /// <summary>工厂：为每个敌人创建独立的策略实例</summary>
    public sealed class StrategyFactory
    {
        public IEnemyStrategy Create(EnemyState s) => s switch
        {
            EnemyState.Patrol  => new PatrolStrategy(),
            EnemyState.Chase   => new ChaseStrategy(),
            EnemyState.Attack  => new AttackStrategy(),
            _                  => new DeadStrategy()
        };
    }




}