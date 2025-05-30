using UnityEngine;

namespace World
{
    public class Key : MonoBehaviour
    {
        /* 大小默认 0.4×0.4，可在 Inspector 调 */
        public Vector2 halfSize = new(0.2f, 0.2f);

        bool collected;

        void Update()
        {
            if (collected) return;

            var player = FindObjectOfType<Player.Player>();
            if (player == null) return;

            if (Overlap(player))
                Collect(player);
        }

        bool Overlap(Player.Player p)
        {
            Vector2 pMin = p.Pos - p.Half;
            Vector2 pMax = p.Pos + p.Half;

            Vector2 kMin = (Vector2)transform.position - halfSize;
            Vector2 kMax = (Vector2)transform.position + halfSize;

            return !(kMax.x < pMin.x || kMin.x > pMax.x ||
                     kMax.y < pMin.y || kMin.y > pMax.y);
        }

        void Collect(Player.Player p)
        {
            collected = true;
            p.AddKey();

            /* 动效：先隐藏渲染器再销毁 */
            var sr = GetComponent<SpriteRenderer>();
            if (sr) sr.enabled = false;
            Destroy(gameObject, 0.1f);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, halfSize * 2);
        }
#endif
    }
}
