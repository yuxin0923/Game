// Assets/Scripts/AI/StrategyFactory.cs
using AIEnemy;

namespace AIEnemy
{
    /// <summary>Factory: create separate policy instances for each enemy</summary>
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