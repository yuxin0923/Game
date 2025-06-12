
// Assets/Scripts/AI/AIContracts.cs

/*
1. **EnemyState enum** – labels each high‑level behaviour (*Patrol, Chase,
   Attack, Dead*).  Used by `AIEnemyManager` as a finite‑state indicator.
2. **IEnemyStrategy interface** – the common API all behaviour classes must
   implement.  Follows the **Strategy Pattern** so new behaviours can be hot‑
   swapped without modifying the FSM.

Design Pattern Note
-------------------
* **State Pattern**: `EnemyState` represents the discrete states the enemy can
  be in.
* **Strategy Pattern**: `IEnemyStrategy` decouples *what each state does* from
  the manager that decides *when* to run it.

Add‑on Friendly: introducing a “Flee” state later is as simple as adding a
`Flee` value here and writing `FleeStrategy : IEnemyStrategy` — the rest of
 the code remains untouched.
*/
using UnityEngine;

namespace AIEnemy
{
    /// Enemy finite state machines can take values
    public enum EnemyState { Patrol, Chase, Attack, Dead }

    /// All behavior strategies implement this interface
    public interface IEnemyStrategy
    {
        /// Returns true  ⇒  Needs to exit current state
        bool Execute(AIEnemyManager ctx, float dt);
    }
}
