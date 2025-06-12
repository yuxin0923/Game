using UnityEngine;
using GameCore; 

namespace World
{
    public class Key : MonoBehaviour
    {
        /* Default size 0.4×0.4, can be adjusted in Inspector */
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


            GameEvents.OnKeyCollected?.Invoke();  // Notify level logic

            /* Animation: Hide the renderer first, then destroy */
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
