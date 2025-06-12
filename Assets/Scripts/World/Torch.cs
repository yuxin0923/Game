
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace World
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Light2D))]
    public class Torch : MonoBehaviour
    {
        [Header("Initial ignition or not")]
        public bool isOn = true;

        [Header("Necessary: Drag two Sprites into these slots (for editor preview only)")]
        public Sprite torchOnSprite;
        public Sprite torchOffSprite;

        [Header("Light Intensity Settings (When Ignited / Extinguished)")]
        public float onLightIntensity = 1f;
        public float offLightIntensity = 0f;

        [Header("Hint (Optional)")]
        public Canvas hintCanvas;
        public float hintRange = 1.2f;
        public GameObject keyL;
        public GameObject keyM;

        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private Light2D light2d;
        private Player.Player player;

        private static readonly int kIsOnParam = Animator.StringToHash("isOn");

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            light2d = GetComponent<Light2D>();

            player = FindObjectOfType<Player.Player>();

            if (torchOffSprite != null && torchOnSprite != null)
            {
                spriteRenderer.sprite = isOn ? torchOnSprite : torchOffSprite;
            }

            animator.SetBool(kIsOnParam, isOn);
            ApplyLightIntensity(isOn); // 初始化灯光状态

            if (hintCanvas != null) hintCanvas.gameObject.SetActive(false);
            if (keyL != null) keyL.SetActive(false);
            if (keyM != null) keyM.SetActive(false);
        }

        void Update()
        {
            if (player != null && hintCanvas != null)
            {
                float sqrDist = (player.transform.position - transform.position).sqrMagnitude;
                bool showHint = sqrDist <= hintRange * hintRange;
                if (hintCanvas.gameObject.activeSelf != showHint)
                {
                    hintCanvas.gameObject.SetActive(showHint);
                }
            }

            if (player != null && keyM != null)
            {
                bool showM = PlayerIsNear() && isOn;
                if (keyM.activeSelf != showM)
                    keyM.SetActive(showM);
            }
        }

        public void Switch()
        {
            SetState(!isOn);
        }

        public void SetState(bool on)
        {
            isOn = on;
            animator.SetBool(kIsOnParam, isOn);
            ApplyLightIntensity(isOn);
            Debug.Log($"{name} switched: " + isOn);
        }

        // Modified light control method - completely disable the component
        private void ApplyLightIntensity(bool on)
        {
            if (light2d != null)
            {
                // Directly enable/disable the entire Light2D component
                light2d.enabled = on;

                // If the state is on, set the correct intensity
                if (on)
                {
                    light2d.intensity = onLightIntensity;
                }
            }
        }

        public bool PlayerIsNear()
        {
            if (player == null) return false;
            float sqrDist = (player.transform.position - transform.position).sqrMagnitude;
            return (sqrDist <= hintRange * hintRange);
        }

        public static Torch GetNearestBurning(Vector2 worldPos, float minDistance)
        {
            float minDistSqr = minDistance * minDistance;
            Torch bestTorch = null;
            float bestSqr = float.PositiveInfinity;

            foreach (var t in FindObjectsOfType<Torch>())
            {
                if (!t.isOn) continue;
                float sqr = ((Vector2)t.transform.position - worldPos).sqrMagnitude;

                if (sqr < minDistSqr) continue;
                if (sqr < bestSqr)
                {
                    bestTorch = t;
                    bestSqr = sqr;
                }
            }
            return bestTorch;
        }

        public static Torch GetNearestInRadius(Vector3 playerPos, float radius)
        {
            Torch bestTorch = null;
            float bestSqr = radius * radius;

            foreach (var t in Object.FindObjectsOfType<Torch>())
            {
                float sqr = (t.transform.position - playerPos).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    bestTorch = t;
                }
            }
            return bestTorch;
        }

        private void OnValidate()
        {
            if (spriteRenderer != null && torchOnSprite != null && torchOffSprite != null)
            {
                spriteRenderer.sprite = isOn ? torchOnSprite : torchOffSprite;
            }

            // Modified editor preview handling
            if (light2d != null)
            {
                // Use the enabled property to control the light switch
                light2d.enabled = isOn;

                // If the state is on, set the correct intensity
                if (isOn)
                {
                    light2d.intensity = onLightIntensity;
                }
            }
            
            if (animator != null)
            {
                animator.SetBool(kIsOnParam, isOn);
            }
        }
    }
}