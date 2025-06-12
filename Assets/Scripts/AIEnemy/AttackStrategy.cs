// Assets/Scripts/AI/AttackStrategy.cs
/*
AttackStrategy.cs — Strategy Pattern Node ("Attack")
====================================================
Part of the **Strategy Pattern** that powers the enemy FSM. `AIEnemyManager`
selects this class whenever the current state is *Attack* and then calls
`Execute()` every frame.

Responsibility
--------------
• Keep the enemy anchored near the player while applying periodic damage.
• Abort and hand control back to the manager when:  
  – The player walks out of range.  
  – The enemy is blocked by a cliff/wall for too long.
• Encapsulate attack‑specific cooldowns and edge detection so other
  strategies remain unaware of them.

Why this helps later
--------------------
Need a new attack type (e.g. ranged, AOE)?  Just derive a new strategy or
extend this one without touching `AIEnemyManager` or patrol/chase code.
*/
using UnityEngine;

namespace AIEnemy
{
    [System.Serializable]
    public class AttackStrategy : IEnemyStrategy
    {
        [Tooltip("This attack deducts the value of the player's flashlight charge")]
        [Min(1f)] public float damage = 20f;

        [Tooltip("Attack interval (seconds)")]
        public float cooldown = 0.75f;

        [Tooltip("Attack range, exit attack state if out of this range")]
        public float attackRange = 1f;

        private float _cd;
        private float _waitTimer = 0f;
        private const float MAX_WAIT_TIME = 2f; // Wait at the edge for up to 2 seconds

        public bool Execute(AIEnemyManager ctx, float dt)
        {
            // 1. Player is out of attack range (with buffer) → back to Chase
            float dist = Vector2.Distance(ctx.PlayerTf.position, ctx.transform.position);
            if (dist > attackRange * 1.2f) 
                return true;

            // 2. Cliff/wall detection
            bool cliffOrWall = CheckCliffOrWall(ctx);
            if (cliffOrWall)
            {
                // Stop at the dangerous edge
                ctx.Body.MoveHoriz(0, 0);
                ctx.SetAnimMove(0, true);

                // Wait timer
                _waitTimer += dt;

                // Wait timeout → back to Patrol
                if (_waitTimer >= MAX_WAIT_TIME)
                {
                    _waitTimer = 0f;
                    return true;
                }
                
                return false;
            }
            else
            {
                // Reset wait timer
                _waitTimer = 0f;
            }

            // 3. Force attack when player is fully overlapped (no cooldown)
            if (dist <= 0.1f) 
            {
                _cd = 0;
            }

            // 4. Cooldown handling
            _cd -= dt;
            if (_cd <= 0f)
            {
                _cd = cooldown;
                ctx.SetAnimCatchPlayer();

                // 5. Immediate damage
                var player = ctx.PlayerTf.GetComponent<Player.Player>();
                player?.OnAttacked(damage);
            }

            // 6. Stay on Attack
            return false;
        }
        
        // Detects if there is a cliff or wall in front of you
        private bool CheckCliffOrWall(AIEnemyManager ctx)
        {
            var pos = ctx.transform.position;
            var half = ctx.Body.HalfSize;
            const float δ = 0.03f; // small offset
            int facing = ctx.Facing;

            // Cliff detection probe
            Vector2 cliffProbe = new Vector2(
                pos.x + facing * (half.x + δ), // One position ahead.
                pos.y - half.y - 0.1f          // Foot position
            );

            // Wall detection probe
            Vector2 wallProbe = new Vector2(
                pos.x + facing * (half.x + δ), // One position ahead.
                pos.y                          // Middle height
            );

            // Detects if there is a cliff or wall in front of you
            bool cliffAhead = !TilemapWorld.I.IsSolid(cliffProbe);
            bool wallAhead = TilemapWorld.I.IsSolid(wallProbe);
            
            return cliffAhead || wallAhead;
        }
    }
}