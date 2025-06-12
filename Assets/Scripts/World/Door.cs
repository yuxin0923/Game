// Assets/Scripts/World/Door.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameCore; // 引用 GameCore 命名空间，以便访问 GameEvents
using Player;   // 引用 Player 命名空间，以便访问 Player.Player
                // 引用你自写的物理引擎命名空间，以便访问 SimplePhysicsBody;
using TMPro;    // 引用 TextMeshPro 命名空间，以便显示钥匙数量 

namespace World
{
    /// <summary>
    /// Door: Uses entirely self-written physics engine AABB detection to determine if the player is “standing in front of the door” and has enough keys, 
    /// Once this is the case, the door opening animation is played (if any) and delayed to switch to the next scene.
    /// 
    /// Description: 
    /// - This script does not rely on any Unity Collider2D / Rigidbody2D. 
    /// - The collision area of the “door” is represented by two fields: doorCenter and doorHalfSize. An AABB. 
    /// - The player collision box is determined by the halfSize property provided by SimplePhysicsBody, and the center is determined by transform.position.
    /// - Triggers the door opening when the player's AABB intersects the door's AABB and player.keyCount >= requiredKeys.
    /// </summary>
    public class Door : MonoBehaviour
    {
        [Header("—— Door AABB Area (No Unity Collider Required) ——")]
        [Tooltip("Door center position (world coordinates). If you want the door to follow the GameObject, leave it empty and sync transform.position in Awake/Update.")]
        public Vector2 doorCenter = Vector2.zero;

        [Tooltip("Door half-width and half-height (world units). Total width = doorHalfSize.x * 2, total height = doorHalfSize.y * 2")]
        public Vector2 doorHalfSize = new Vector2(1f, 2f);

        [Header("—— Key & Scene Transition ——")]
        [Tooltip("The number of keys the player must hold to open this door")]
        public int requiredKeys = 1;

        [Tooltip("The name of the next scene to load after opening the door (must be added to Build Settings)")]
        public string nextSceneName;

        [Header("—— Door Opening Animation (Optional) ——")]
        [Tooltip("If the door has open/close animations, attach the door object's Animator here; leave empty if no animation")]
        public Animator animator;

        [Tooltip("The name of the bool parameter in the Animator that controls the door opening, default is “isOpen”. Must match the parameter in the Animator Controller.")]
        public string isOpenParam = "isOpen";

        [Tooltip("Wait how many seconds after playing the door opening animation before LoadScene, to ensure the animation is finished")]
        public float openToLoadDelay = 1.0f;




        [Header("UI Display - Drag the KeyText Below")]
        [SerializeField] TMP_Text keyText;

        // —— Private Fields —— 
        private Player.Player playerRef;          // Reference to the Player.Player in the scene
        private SimplePhysicsBody playerBody;     // Reference to the SimplePhysicsBody on the player
        private bool hasOpened = false;           // Prevents re-triggering the door opening logic

        void Awake()
        {
            // If doorCenter is not specified in the Inspector, set it to the current GameObject's position:
            if (doorCenter == Vector2.zero)
            {
                doorCenter = transform.position;
            }
        }

        void Start()
        {
            // 1) Find the unique Player.Player in the scene
            playerRef = FindObjectOfType<Player.Player>();
            if (playerRef == null)
            {
                Debug.LogError("[Door]：场景中找不到 Player.Player，请确认 Player 脚本已挂到玩家物体上。");
                return;
            }

            // 2) Get the SimplePhysicsBody on the Player to access the player's collision box halfSize
            playerBody = playerRef.GetComponent<SimplePhysicsBody>();
            if (playerBody == null)
            {
                Debug.LogError("[Door]：Player 身上没有挂 SimplePhysicsBody，请确认已挂载。");
            }

            // 3) If you want to use animations, make sure to drag the Animator to the Inspector; otherwise, a warning will be issued
            if (animator == null)
            {
                Debug.LogWarning("[Door]：未指定 Animator，开门时不会播放动画，只会直接延时切换场景。");
            }

            // Initialize the key display
            UpdateKeyDisplay();
            // Subscribe: Refresh the display whenever the player collects a key
            GameEvents.OnKeyCollected += UpdateKeyDisplay;
        }


        void OnDestroy()
        {
            GameEvents.OnKeyCollected -= UpdateKeyDisplay;
        }

        // Each time a key is collected, the player's keyCount will be +1 → here we take the latest value and refresh it.
        void UpdateKeyDisplay()
        {
            if (keyText != null)
            {
                // Here we construct the string, prefixing it with "keys: "
                keyText.text = $"keys: {playerRef.keyCount}/{requiredKeys}";
            }
        }


        void Update()
        {
            // If hasOpened is true, no need to check again
            if (hasOpened) return;
            if (playerRef == null || playerBody == null) return;

            // - I: Determine whether the number of keys meets the requirement
            if (playerRef.keyCount < requiredKeys)
            {
                return;
            }

            // - II: Update doorCenter to follow GameObject
            // If you want the door logic to sync with the position of the GameObject this script is attached to, you can uncomment the line below:
            // doorCenter = transform.position;

            // - III: Construct the player's AABB
            Vector2 playerCenter = playerRef.transform.position;
            Vector2 playerHalfSize = playerBody.halfSize;

            // - IV: Check AABB intersection
            if (IsAABBIntersect(playerCenter, playerHalfSize, doorCenter, doorHalfSize))
            {
                // Player's AABB intersects with the door's AABB and key count is sufficient → Open the door
                TriggerOpenDoor();
            }
        }

        /// <summary>
        /// Determine if two AABBs (Axis-Aligned Bounding Boxes) intersect 
        /// centerA / halfA: center of object A + half size 
        /// centerB / halfB: center of object B + half size
        /// </summary>
        private bool IsAABBIntersect(Vector2 centerA, Vector2 halfA, Vector2 centerB, Vector2 halfB)
        {
            bool overlapX = Mathf.Abs(centerA.x - centerB.x) <= (halfA.x + halfB.x);
            bool overlapY = Mathf.Abs(centerA.y - centerB.y) <= (halfA.y + halfB.y);
            return overlapX && overlapY;
        }

        /// <summary>
        /// Trigger the "open door" logic: play animation (if any), then delay loading the next scene
        /// </summary>
        private void TriggerOpenDoor()
        {
            hasOpened = true;

            // 1)  Play the door opening animation (if Animator is not empty)
            if (animator != null && !string.IsNullOrEmpty(isOpenParam))
            {
                animator.SetBool(isOpenParam, true);
            }

            // 2) Start the coroutine: wait for the animation to finish before changing scenes
            StartCoroutine(OpenAndLoadCoroutine());
        }

        private IEnumerator OpenAndLoadCoroutine()
        {
            // Wait for a while to ensure the animation plays completely. If no animation is needed, set openToLoadDelay to 0.
            yield return new WaitForSeconds(openToLoadDelay);

            // Check if the next scene name is empty
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogError("[Door]：nextSceneName is empty, you can't switch scenes. Please fill in the correct scene name in Inspector and make sure it is added to Build Settings.");
                yield break;
            }

            GameEvents.OnDoorOpened?.Invoke(nextSceneName);

            // Log message
            Debug.Log($"[Door]：Player's key count ≥ {requiredKeys} and standing in front of the door, loading scene “{nextSceneName}” …");
            // SceneManager.LoadScene(nextSceneName);


        }

#if UNITY_EDITOR
        /// <summary>
        /// Draws the door's AABB area in the Scene view of the editor for easy adjustment and visualization.
        /// </summary>
        void OnDrawGizmosSelected()
        {
            // Door's center: If doorCenter is not set, use transform.position
            Vector2 center = (doorCenter == Vector2.zero) ? (Vector2)transform.position : doorCenter;

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);                     // Semi-transparent orange fill
            Vector3 gizCenter = new Vector3(center.x, center.y, 0f);
            Vector3 gizSize = new Vector3(doorHalfSize.x * 2, doorHalfSize.y * 2, 0f);
            Gizmos.DrawCube(gizCenter, gizSize);

            Gizmos.color = Color.yellow;                                      // Yellow wireframe
            Gizmos.DrawWireCube(gizCenter, gizSize);
        }
#endif
    }
}
