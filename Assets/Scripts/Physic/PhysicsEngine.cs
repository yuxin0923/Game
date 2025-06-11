// using System.Collections.Generic;
// using UnityEngine;

// public class PhysicsEngine : MonoBehaviour
// {
//     public static PhysicsEngine I { get; private set; }

//     [Header("Global")]
//     public Vector2 gravity = new(0, -30);

//     // ↓ 如果 C# 版本低，把 new() 换成 new List<SimplePhysicsBody>()
//     readonly List<SimplePhysicsBody> bodies = new();
    

//     public void Register(SimplePhysicsBody b)   => bodies.Add(b);
//     public void Unregister(SimplePhysicsBody b) => bodies.Remove(b);

//     void Awake()
//     {
//         if (I && I != this) Destroy(gameObject);
//         I = this;
//     }

//     void FixedUpdate()
//     {
//         float dt = Time.fixedDeltaTime;
//         foreach (var b in bodies) b.Tick(dt);
//     }
// }
using UnityEngine;
using System.Collections.Generic;

public class PhysicsEngine : MonoBehaviour
{
    public static PhysicsEngine I { get; private set; }

    // 1) 私有字段——一定要保留它，类内部所有 foreach(var p in platforms) 都要用它
    readonly List<SimplePhysicsBody> bodies     = new();
    readonly List<MovablePlatform> platforms    = new();

    // 2) 对外暴露一个只读属性，供 SimplePhysicsBody 调用
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

        // 1) 先移动所有平台
        foreach (var p in platforms)
            p.Tick(dt);

        // 2) 记录上一帧位置并更新刚体
        int n = bodies.Count;
        var prevPos = new Vector3[n];
        for (int i = 0; i < n; i++)
            prevPos[i] = bodies[i].transform.position;
        for (int i = 0; i < n; i++)
            bodies[i].Tick(dt);

        // 3) 平台落地拦截（防止穿透）
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

        // 4) 载人 & 更新平台最后位置
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
