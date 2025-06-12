using UnityEngine;
using UnityEngine.Tilemaps;
using Player;

namespace World
{
    [RequireComponent(typeof(Tilemap))]
    public class DeathWall : MonoBehaviour
    {
        private Vector2 wallCenter;
        private Vector2 wallHalfSize;
        private Player.Player playerRef;
        private SimplePhysicsBody playerBody;
        private bool hasKilled = false;

        void Awake()
        {
            Tilemap tm = GetComponent<Tilemap>();
            if (tm == null) return;
            
            tm.CompressBounds();
            Bounds localBounds = tm.localBounds;
            
            Vector3 worldCenter3 = tm.transform.TransformPoint(localBounds.center);
            Vector3 worldSize3 = Vector3.Scale(localBounds.size, tm.transform.lossyScale);

            wallCenter = worldCenter3;
            wallHalfSize = worldSize3 * 0.5f;
        }

        void Start()
        {
            playerRef = FindObjectOfType<Player.Player>();
            if (playerRef == null) return;
            
            playerBody = playerRef.GetComponent<SimplePhysicsBody>();
        }

        void FixedUpdate()
        {
            if (hasKilled || playerRef == null || playerBody == null) return;

            Vector2 pCenter = playerRef.transform.position;
            Vector2 pHalf = playerBody.halfSize;

            // AABB detection methods using the physics engine
            if (CollisionDetector.AABBOverlap(pCenter, pHalf, wallCenter, wallHalfSize))
            {
                playerRef.Die();
                hasKilled = true;
            }
        }

        #if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Vector2 center = wallCenter;
            Vector2 half = wallHalfSize;
            if (center == Vector2.zero && half == Vector2.zero)
            {
                center = transform.position;
                half = Vector2.one;
            }

            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawCube(center, half * 2);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, half * 2);
        }
        #endif
    }
}