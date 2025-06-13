// Assets/Scripts/AI/DeadStrategy.cs
using UnityEngine;
/*
 DeadStrategy.cs — Placeholder Strategy (Strategy Pattern hook)
 --------------------------------------------------------------
 A minimal “do-nothing” behaviour used when an enemy reaches the **Dead**
 state.  It keeps the FSM valid today while leaving a clear spot to plug in
 future features such as death animation, loot drops, particles, pooling,
 or rag-doll cleanup.  Swapping in those effects later only requires
 fleshing out `Execute()`—no changes to the manager or factory.
*/
namespace AIEnemy
{
    public class DeadStrategy : IEnemyStrategy
    {
        public bool Execute(AIEnemyManager ctx, float dt)
        {
            // play death VFX / drop loot / return to pool …
            return false;
        }
    }
}
