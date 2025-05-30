using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace World
{
    [RequireComponent(typeof(Light2D))]
    public class Torch : MonoBehaviour
    {
        public bool isOn = true;

        [Header("Hint")]
        public float hintRange = 1.2f;        // 玩家距离多少米出现提示
        public Canvas hintCanvas;             // 拖子物体 Canvas
        public GameObject keyL;
        public GameObject keyM;

        Light2D light2d;
        Player.Player player;

        void Awake()
        {
            light2d = GetComponent<Light2D>();
            player = FindObjectOfType<Player.Player>();
            ApplyState();
            if (hintCanvas) hintCanvas.gameObject.SetActive(false);
        }

        void Update()
        {
            if (!player || !hintCanvas) return;

            float sqr = (player.transform.position - transform.position).sqrMagnitude;
            bool show = sqr <= hintRange * hintRange;

            if (show != hintCanvas.gameObject.activeSelf)
                hintCanvas.gameObject.SetActive(show);
        }

        public void Switch() => SetState(!isOn);
        public void SetState(bool on)
        {
            isOn = on;
            ApplyState();
        }

        void ApplyState()     // 开关火把光
        {
            if (light2d) light2d.enabled = isOn;
            if (keyM) keyM.SetActive(isOn);   // 火把点燃时才显示“充电 (M)”
        }

        // Torch.cs 末尾或任意合适位置
        public static Torch GetNearestBurning(Vector2 worldPos)
        {
            Torch[] torches = FindObjectsOfType<Torch>();
            Torch best = null;
            float bestDistSq = float.PositiveInfinity;

            foreach (var t in torches)
            {
                if (!t.isOn) continue;                 // 只要点燃的
                float d = (t.transform.position - (Vector3)worldPos).sqrMagnitude;
                if (d < bestDistSq)
                {
                    bestDistSq = d;
                    best = t;
                }
            }
            return best;
        }
        
                // ① 圆形范围：半径 r 内就算可交互
        public static Torch GetNearestInRadius(Vector3 playerPos, float radius)
        {
            float best = radius * radius;
            Torch bestTorch = null;

            foreach (var t in Object.FindObjectsOfType<Torch>())
            {
                float d = (t.transform.position - playerPos).sqrMagnitude;
                if (d <= best)      // 在圆内
                {
                    best = d;
                    bestTorch = t;
                }
            }
            return bestTorch;
        }

    }
}
