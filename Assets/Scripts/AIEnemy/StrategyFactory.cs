// Assets/Scripts/AI/StrategyFactory.cs
using AIEnemy;
/*
 StrategyFactory.cs — Behaviour Creator (Factory-Method + Strategy)
 ------------------------------------------------------------------
 • Purpose  
   Encapsulates the **construction logic** for enemy behaviour classes so
   that `AIEnemyManager` never has to `new` a concrete type or know which
   class goes with which `EnemyState`.

 • How it fits in  
     AIEnemyManager      (Context / FSM)
         │  asks for
         ▼
     StrategyFactory ──► returns  PatrolStrategy / ChaseStrategy / …
         │
         ▼  (implements)
     IEnemyStrategy      (Strategy interface)

   The manager caches the resulting instance per state, so allocation
   happens only once per enemy.

 • Extending  
   1. Add a value to the `EnemyState` enum, e.g. `Sneak`.  
   2. Implement `SneakStrategy : IEnemyStrategy`.  
   3. Append a line to the switch expression below.  
   No other code changes required — open for extension, closed for
   modification.
*/
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