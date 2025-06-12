
using UnityEngine;
using System.Collections.Generic;
/*
 PhysicsEngine.cs — Lightweight 2-D Kinematic Solver (Singleton + Service)
 ------------------------------------------------------------------------
 • Role  
   Central “physics loop” that ticks every MovablePlatform first, then every 
   SimplePhysicsBody, and finally resolves landings / platform-carry.  
   Replaces Unity’s built-in physics with a deterministic, tile-friendly
   scheme tailored to side-scrollers.

 • Key interactions  
   – Bodies / platforms self-register via Register/Unregister; the engine
     stores them in private lists and exposes Platforms as read-only for
     collision probes.  
   – SimplePhysicsBody asks the engine for surface material and grounding
     status but performs its own velocity integration.

 • Why this design?  
   1. **Singleton Service** Any script can join the physics system without
      keeping scene references, and exactly one engine runs per frame.  
   2. **Deterministic Ordering** By moving platforms before bodies, landing
      penetration is resolved in a single place, avoiding per-object race
      conditions.  
   3. **Platform Carry** LateTick applies the platform’s movement delta to
      grounded bodies, removing the need for dynamic parenting.  
   4. **Extensibility** New features (slopes, one-way tiles, conveyor belts)
      can be added inside this hub without touching individual body scripts.
*/

public class PhysicsEngine : MonoBehaviour
{
    public static PhysicsEngine I { get; private set; }

    // 1) Private field - make sure to keep it, all foreach(var p in platforms) inside the class should use it
    readonly List<SimplePhysicsBody> bodies     = new();
    readonly List<MovablePlatform> platforms    = new();

    // 2) Expose a read-only property for SimplePhysicsBody to access
    public IReadOnlyList<MovablePlatform> Platforms => platforms;

    public void Register(SimplePhysicsBody b)        => bodies.Add(b);
    public void Unregister(SimplePhysicsBody b)      => bodies.Remove(b);
    public void RegisterPlatform(MovablePlatform p)  => platforms.Add(p);
    public void UnregisterPlatform(MovablePlatform p)=> platforms.Remove(p);

    void Awake()
    {
        if (I && I != this) Destroy(gameObject);
        I = this;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // 1) Move all platforms first
        foreach (var p in platforms)
            p.Tick(dt);

        // 2) Record previous frame positions and update rigidbodies
        int n = bodies.Count;
        var prevPos = new Vector3[n];
        for (int i = 0; i < n; i++)
            prevPos[i] = bodies[i].transform.position;
        for (int i = 0; i < n; i++)
            bodies[i].Tick(dt);

        // 3) Platform landing interception (prevent penetration)
        for (int i = 0; i < n; i++)
        {
            var b = bodies[i];
            if (b.velocity.y <= 0f)
            {
                float prevFeetY = prevPos[i].y - b.halfSize.y;
                float currFeetY = b.transform.position.y - b.halfSize.y;

                foreach (var p in platforms)
                {
                    float topY = p.transform.position.y + p.centerOffset.y + p.halfSize.y;
                    if (prevFeetY > topY && currFeetY <= topY)
                    {
                        float dx = Mathf.Abs(b.transform.position.x - (p.transform.position.x + p.centerOffset.x));
                        if (dx < b.halfSize.x + p.halfSize.x)
                        {
                            var pos = b.transform.position;
                            pos.y = topY + b.halfSize.y;
                            b.transform.position = pos;
                            b.velocity = new Vector2(b.velocity.x, 0f);
                            b.grounded = true;
                            b.SetSurfaceMaterial(p.surfaceMaterial);
                            break;
                        }
                    }
                }
            }
        }

        // 4) Platform landing & update final positions
        foreach (var p in platforms)
        {
            Vector2 delta = p.MovementDelta;
            foreach (var b in bodies)
            {
                if (!b.grounded) continue;
                Vector2 feet = (Vector2)b.transform.position
                             - Vector2.up * (b.halfSize.y - 0.01f);
                if (p.IsOverlapping(feet, b.halfSize))
                {
                    b.transform.position += new Vector3(delta.x, delta.y, 0f);
                    b.SetSurfaceMaterial(p.surfaceMaterial);
                }
            }
            p.LateTick();
        }
    }
}
