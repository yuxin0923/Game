using System.Collections.Generic;
using UnityEngine;

public class PhysicsEngine : MonoBehaviour
{
    public static PhysicsEngine I { get; private set; }

    [Header("Global")]
    public Vector2 gravity = new(0, -30);

    // ↓ 如果 C# 版本低，把 new() 换成 new List<SimplePhysicsBody>()
    readonly List<SimplePhysicsBody> bodies = new();

    public void Register(SimplePhysicsBody b)   => bodies.Add(b);
    public void Unregister(SimplePhysicsBody b) => bodies.Remove(b);

    void Awake()
    {
        if (I && I != this) Destroy(gameObject);
        I = this;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        foreach (var b in bodies) b.Tick(dt);
    }
}
