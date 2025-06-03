// Assets/Scripts/AIEnemy/AStar.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AIEnemy
{
    /// <summary>4-方向 A* 网格寻路（TilemapWorld.I.IsSolid 判断障碍）</summary>
    public static class AStar
    {
        static readonly Vector2Int[] DIRS = {
            new( 1, 0), new(-1, 0), new(0,  1), new(0, -1)
        };

        class Node
        {
            public Vector2Int pos;
            public Node parent;
            public int g, f; // g = 已走步数, f = g + h
        }

        public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
        {
            var open   = new List<Node>();
            var closed = new HashSet<Vector2Int>();

            open.Add(new Node { pos = start, g = 0, f = Heu(start, goal) });

            while (open.Count > 0)
            {
                open.Sort((a, b) => a.f - b.f);          // 取 f 最小
                Node cur = open[0];
                open.RemoveAt(0);

                if (cur.pos == goal)                     // 抵达
                    return Reconstruct(cur);

                closed.Add(cur.pos);

                foreach (var d in DIRS)
                {
                    Vector2Int nb = cur.pos + d;
                    if (closed.Contains(nb) || TilemapWorld.I.IsSolid(nb))
                        continue;

                    int tentativeG = cur.g + 1;
                    Node existed   = open.FirstOrDefault(n => n.pos == nb);

                    if (existed == null)
                    {
                        open.Add(new Node {
                            pos = nb, parent = cur,
                            g = tentativeG,
                            f = tentativeG + Heu(nb, goal)
                        });
                    }
                    else if (tentativeG < existed.g)
                    {
                        existed.parent = cur;
                        existed.g      = tentativeG;
                        existed.f      = tentativeG + Heu(nb, goal);
                    }
                }
            }
            return null;                                // 无路
        }

        static int Heu(Vector2Int a, Vector2Int b) =>
            Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        static List<Vector2Int> Reconstruct(Node n)
        {
            var list = new List<Vector2Int>();
            while (n != null) { list.Add(n.pos); n = n.parent; }
            list.Reverse();
            return list;
        }
    }
}
