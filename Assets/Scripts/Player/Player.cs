// Assets/Scripts/Player/Player.cs
using UnityEngine;
using World;
using GameCore;  

/*
 Player.cs — Core avatar script (Player package)
 -----------------------------------------------
 • Purpose  
   Exposes a clean, intent-level API (Move, Jump, ToggleTorch, Teleport, etc.)
   that can be invoked by human input, cut-scenes, AI or test harnesses.  
   Low-level collision / gravity is delegated to the sibling component
   SimplePhysicsBody, keeping this class free of physics math.

 • Architectural highlights  
   – Thin façade, Command-pattern-friendly: callers issue “what” to do, not
     “how”, so alternative controllers (e.g., replay, network, AI) can bind
     the same methods.  
   – Composition over inheritance: flashlight energy, key collection and
     tile-based world interactions live in separate components (Flashlight,
     Torch, SimplePhysicsBody), each with a single responsibility and
     inspector-friendly tuning.  
   – Event-driven decoupling: critical states (death, door entry) are
     broadcast via GameEvents, letting GameManager, UI, audio, etc. respond
     without direct references.

   Result: gameplay features can be added or swapped (dash, wall-jump, new
   resource meters) by dropping in new components or extending the façade,
   without touching physics or higher-level systems.
*/

 // Introducing custom SimplePhysicsBody scripts
namespace Player
{
    
    /// Character Logic Layer: Expose only Move / Jump / ToggleTorch interfaces to commands, AI or level triggers.
    /// Real physics are left to SimplePhysicsBody.
    [RequireComponent(typeof(SimplePhysicsBody))]
    public class Player : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Horizontal travel speed (m/s)")]
        public float moveSpeed = 8f;

        [Tooltip("Jump initial velocity (m/s)")]
        public float jumpSpeed = 15f;

        public int keyCount { get; private set; }
        /* Facilitates Key scripts to get a wraparound box */
        public Vector2 Pos => transform.position;
        public Vector2 Half => body.halfSize;
        // References to Flashlight
        [Header("Flashlight (Charge)")]
        public Flashlight flashlight;         // Drag the Flashlight component from the Player in the scene
        [Tooltip("Distance to start charging when near an ignited torch (meters)")]
        public float rechargeRange = 1.2f;

        /* ---------- Component References ---------- */
        SimplePhysicsBody body;     // Custom rigid body script
                                    // Flashlight torch;           // Can be null if no flashlight is attached
        
    
        void Awake()
        {
            body = GetComponent<SimplePhysicsBody>();
            // torch = GetComponent<Flashlight>();   // Optional component
        }

        void Update()
        {
            HandleRechargeProximity();
            // Other inputs: Move/Jump/ToggleTorch/Teleport, etc.
        }


        /* ========== Interface for ICommand / AI calls ========== */

        // /// Horizontal movement: dir ∈ [-1,1] (left -1, right +1, release 0)
        public void Move(float dir)
        {
            body.SetMoveInput(dir, moveSpeed);
        }


        /// Jump: only effective when grounded
        public void Jump()
        {
            if (body.grounded)
                body.velocity.y = jumpSpeed;
        }
        /* ------------------------------------------------ */
        /* Here you can continue to add advanced action interfaces such as Dash(), WallJump(), etc.    */
        /* ------------------------------------------------ */
        public void AddKey()
        {
            keyCount++;
            Debug.Log($"Key collected! total = {keyCount}");
        }

        /* ====== New: Handle "Near Torch" Charging/Discharging ====== */
        void HandleRechargeProximity()
        {
            if (flashlight == null) return;   // If no Flashlight component is attached, exit

            // 1. Check if there are any "lit torches" within a circular range around the player
            Torch near = Torch.GetNearestInRadius(transform.position, rechargeRange);
            if (near != null && near.isOn)
            {
                // As long as a lit torch is found, start charging the flashlight
                flashlight.StartRecharge();
            }
            else
            {
                // Otherwise, stop charging when out of range
                flashlight.StopRecharge();
            }
        }

        /// Switch the state of the nearest torch and teleport to the position above the nearest lit torch
        public void TeleportToNearestBurningTorch(float interactRange = 1.2f)
        {
            /*-----------------------------------------------------------
            * ① Check if the player is really “standing on a torch”.
            * ① Check if the player is really “standing on a torch” and the torch is in a lit state; otherwise, the teleportation is invalid.
            *----------------------------------------------------------*/
            Torch curr = Torch.GetNearestInRadius(transform.position, interactRange);
            if (curr == null || !curr.isOn)
                return;                                         //A. no torch / B. torch not lit → straight out

            /*-----------------------------------------------------------
            * ② Find the "nearest lit torch" that is not the same as the current one
            *----------------------------------------------------------*/
            Torch[] torches = Object.FindObjectsOfType<Torch>();
            Torch target = null;
            float bestSq = float.PositiveInfinity;
            Vector2 me = transform.position;

            foreach (var t in torches)
            {
                if (!t.isOn || t == curr) continue;             // Only consider lit & non-current ones
                float sq = ((Vector2)t.transform.position - me).sqrMagnitude;
                if (sq < bestSq)
                {
                    bestSq = sq;
                    target = t;
                }
            }

            if (target == null) return;                        // No other lit torches in the scene → Do not move

            /*-----------------------------------------------------------
            * ③ Teleport the player to the position above the target torch
            *----------------------------------------------------------*/
            Vector2 half = body.halfSize;
            Vector3 pos = target.transform.position;
            pos.y += half.y + 0.05f;                           // Slightly raise to avoid sticking
            transform.position = pos;
            body.velocity = Vector2.zero;
        }



        /// Switch the state of the nearest torch (ignite ⇄ extinguish)
        public void ToggleNearestTorch(float radius = 1.2f)
        {
            Torch t = Torch.GetNearestInRadius(transform.position, radius);
            if (t != null)
                t.Switch();                // Ignite ⇄ Extinguish
        }

        // Player class, under other public methods
        /* ====== Damage Handling ====== */

        public void Die()
        {
            if (flashlight != null)
            {
                flashlight.DrainAll(); // Immediately drain all charge
            }
            Debug.Log("Player died: out of battery");

            // Trigger global death event, handled by GameManager
            GameEvents.OnPlayerDied?.Invoke();

            // TODO: play the death animation before destroying it.
            //Destroy(gameObject);

        }
        
        
        public void OnAttacked(float amount)
        {
            if (flashlight != null)
            {
                flashlight.ReduceCharge(amount);
            }
            else
            {
                // Direct death without flashlight kit
                Die();
            }
        }


    }
}
