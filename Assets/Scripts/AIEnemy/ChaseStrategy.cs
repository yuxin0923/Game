// Assets/Scripts/AI/ChaseStrategy.cs
/*
ChaseStrategy.cs — Strategy Node with Simple Steering Behaviour (Seek + Edge-avoid)
=================================================================================
Role in Architecture
--------------------
Selected by **AIEnemyManager** when the global FSM enters *Chase*. Implements a
very light-weight **Steering Behaviour** – essentially a *seek* force towards
the player, clamped to 1-D, with an additional rule to avoid stepping off
cliffs. Packaged as a **Strategy Pattern** element so alternative chase styles
(e.g. path-finding, jump-capable, zig-zag) can be swapped in later.

Maintenance Wins
----------------
* Want smarter chasing?  Replace edge probe with A* path or Steering flock.
* Need flying enemies?  Add a new `FlyChaseStrategy` that ignores ground checks.
* All without touching the core FSM or other strategies.
*/
using UnityEngine;

namespace AIEnemy
{
    public class ChaseStrategy : IEnemyStrategy
    {
        public float speed = 3.5f;
        public float attackRange = 1f;
        private float _waitTimer = 0f;
        private const float MAX_WAIT_TIME = 2f; // Wait up to 2 seconds at the edge

        public bool Execute(AIEnemyManager ctx, float dt)
        {
                // 1. Calculate true distance (including vertical component)
            float dist = Vector2.Distance(ctx.PlayerTf.position, ctx.transform.position);

            // 2. Player enters attack range → switch state by AIEnemyManager
            if (dist <= attackRange)
                return false; // Do not switch state, wait for Manager to handle

            // 3. Calculate horizontal direction for movement
            float dx = ctx.PlayerTf.position.x - ctx.transform.position.x;
            int chaseDir = dx > 0 ? +1 : -1;

            // Edge detection: is there a cliff ahead?
            bool cliffAhead = CheckCliffAhead(ctx, chaseDir);
            
            if (cliffAhead)
            {
                // Stop at the edge of the cliff
                ctx.Body.MoveHoriz(0, 0);
                ctx.SetAnimMove(0, true); // Speed is 0, but still in chase state

                // If player is in attack range, prepare to attack
                if (Mathf.Abs(dx) <= attackRange * 1.5f)
                {
                    // Keep facing the player
                    ctx.SetFacing(chaseDir);
                    return false;
                }

                // Wait timer
                _waitTimer += dt;

                // Wait timeout → back to Patrol
                if (_waitTimer >= MAX_WAIT_TIME)
                {
                    _waitTimer = 0f;
                    return true; // Back to Patrol
                }
                
                return false;
            }
            else
            {
                // Reset wait timer
                _waitTimer = 0f;

                // Normal chase
                ctx.Body.MoveHoriz(chaseDir, speed);
                ctx.SetAnimMove(chaseDir * speed, true);
                return false;
            }
        }

        // Checks if there is a cliff ahead
        private bool CheckCliffAhead(AIEnemyManager ctx, int dir)
        {
            var pos = ctx.transform.position;
            var half = ctx.Body.HalfSize;
            const float δ = 0.03f; // Small offset

            // Cliff detection probe
            Vector2 cliffProbe = new Vector2(
                pos.x + dir * (half.x + δ), // One position ahead
                pos.y - half.y - 0.1f       // Foot position
            );

            // Detects if there is ground ahead
            bool groundAhead = TilemapWorld.I.IsSolid(cliffProbe);

            // If there is no ground, it's a cliff
            return !groundAhead;
        }
    }
}
