// using UnityEngine;
// using UnityEngine.Rendering.Universal;  // 为了访问 Light2D
// using UnityEngine.UI;                   // 如果你要在 Torch 里控制 Canvas / KeyL / KeyM

// namespace World
// {
//     [RequireComponent(typeof(SpriteRenderer))]
//     [RequireComponent(typeof(Animator))]
//     [RequireComponent(typeof(Light2D))]
//     public class Torch : MonoBehaviour
//     {
//         [Header("初始是否点燃")]
//         public bool isOn = true;

//         [Header("必要：拖拽两张 Sprite 到这两个槽里（仅用于编辑器里预览）")]
//         public Sprite torchOnSprite;   // 编辑器里预览用：任意一帧燃烧火焰 Sprite
//         public Sprite torchOffSprite;  // 编辑器里预览用：熄灭时那张静止 Sprite

//         [Header("灯光强度设置（点燃时／熄灭时）")]
//         public float onLightIntensity = 1f;    // 火把点燃时的光强度
//         public float offLightIntensity = 0f;   // 火把熄灭时的光强度（也可以设成 0.1f 等很微弱的值）

//         [Header("提示（可选）")]
//         public Canvas hintCanvas;      // 玩家靠近时，显示“按 L/按 M”之类的提示 UI
//         public float hintRange = 1.2f; // 提示出现的距离
//         public GameObject keyL;        // “按 L 开/关火把” 的提示小图
//         public GameObject keyM;        // “按 M 传送到最近燃烧的火把” 的提示小图

//         // 内部组件引用
//         private SpriteRenderer spriteRenderer;
//         private Animator      animator;
//         private Light2D       light2d;
//         private Player.Player player;  // 场景里的 Player

//         // Animator 里用的参数哈希
//         private static readonly int kIsOnParam = Animator.StringToHash("isOn");

//         //——————————————————————————
//         // Awake：缓存组件、根据 isOn 设置初始状态、关闭提示
//         //——————————————————————————
//         void Awake()
//         {
//             spriteRenderer = GetComponent<SpriteRenderer>();
//             animator       = GetComponent<Animator>();
//             light2d        = GetComponent<Light2D>();

//             // 尝试在场景里找到 Player 脚本
//             player = FindObjectOfType<Player.Player>();

//             // 编辑器里预览用：先把 SpriteRenderer 切到“开/关”那张图
//             // 运行时会被 Animator 里的 Clip 覆盖掉
//             if (torchOffSprite != null && torchOnSprite != null)
//             {
//                 spriteRenderer.sprite = isOn ? torchOnSprite : torchOffSprite;
//             }

//             // 根据 Inspector 勾的 isOn，把 Animator 参数和 Light2D 强度同步到这两个状态
//             animator.SetBool(kIsOnParam, isOn);
//             ApplyLightIntensity(isOn);

//             // 一开始把所有提示 UI（Canvas / KeyL / KeyM）全部隐藏
//             if (hintCanvas != null) hintCanvas.gameObject.SetActive(false);
//             if (keyL       != null) keyL.SetActive(false);
//             if (keyM       != null) keyM.SetActive(false);
//         }

//         //——————————————————————————
//         // Update：检测玩家距离 → 显示/隐藏提示 → 按键 L 切换 → 按键 M 传送
//         //——————————————————————————
//         void Update()
//         {
//             // 1) “玩家靠近”逻辑：如果有 hintCanvas，就根据距离 show/hide
//             if (player != null && hintCanvas != null)
//             {
//                 float sqrDist = (player.transform.position - transform.position).sqrMagnitude;
//                 bool showHint = sqrDist <= hintRange * hintRange;
//                 if (hintCanvas.gameObject.activeSelf != showHint)
//                 {
//                     hintCanvas.gameObject.SetActive(showHint);
//                 }
//             }



//             // 3) 当玩家靠近且火把当前是开启状态（isOn == true）时，
//             //    就把 keyM (“按 M 传送到最近燃烧的火把”) 提示显示，否则隐藏
//             if (player != null && keyM != null)
//             {
//                 bool showM = PlayerIsNear() && isOn;
//                 if (keyM.activeSelf != showM)
//                     keyM.SetActive(showM);
//             }

//             // 4) 玩家按 M 键：跳到最近燃烧的火把（可选逻辑，依项目决定要不要写在这里）
//             // if (Input.GetKeyDown(KeyCode.M) && PlayerIsNear() && isOn)
//             // {
//             //     PlayerTeleportToNearestTorch();
//             // }
//         }

//         //——————————————————————————
//         // 切换火把开/关的公开接口
//         //——————————————————————————
//         public void Switch()
//         {
//             SetState(!isOn);
//         }

//         //——————————————————————————
//         // 设置火把开/关状态的公开接口
//         // 会同步修改 Animator 参数 和 Light2D 强度
//         //——————————————————————————
//         public void SetState(bool on)
//         {
//             isOn = on;
//             animator.SetBool(kIsOnParam, isOn);
//             ApplyLightIntensity(isOn);
//             Debug.Log($"{name} switched: " + isOn);   // 临时打印
//         }

//         //——————————————————————————
//         // 根据 isOn 决定要把 light2d.intensity 设为 onLightIntensity 还是 offLightIntensity
//         //——————————————————————————
//         private void ApplyLightIntensity(bool on)
//         {
//             if (light2d != null)
//             {
//                 light2d.intensity = on ? onLightIntensity : offLightIntensity;
//                 // 如果想“熄灭时直接关掉 Light2D”、“重启时再打开”
//                 // 可以改成： light2d.enabled = on;
//                 // 但上述方案是调节 intensity，让你可以“熄灭时非常微弱”。
//             }
//         }

//         //——————————————————————————
//         // 判断玩家是否在本火把 hintRange 范围内
//         //——————————————————————————
//         public bool PlayerIsNear()
//         {
//             if (player == null) return false;
//             float sqrDist = (player.transform.position - transform.position).sqrMagnitude;
//             return (sqrDist <= hintRange * hintRange);
//         }

//         //——————————————————————————
//         // 静态方法：返回最近一个已经“点燃(isOn==true)”的火把
//         // 如果找不到则返回 null
//         //——————————————————————————

//         // Torch.cs
//         public static Torch GetNearestBurning(Vector2 worldPos, float minDistance)
//         {
//             float minDistSqr = minDistance * minDistance;
//             Torch bestTorch  = null;
//             float bestSqr    = float.PositiveInfinity;

//             foreach (var t in FindObjectsOfType<Torch>())
//             {
//                 if (!t.isOn) continue;                         // 只考虑点燃的
//                 float sqr = ((Vector2)t.transform.position - worldPos).sqrMagnitude;

//                 if (sqr < minDistSqr) continue;                // 离自己太近 ⇒ 认为“同一盏”，忽略
//                 if (sqr < bestSqr)
//                 {
//                     bestTorch = t;
//                     bestSqr   = sqr;
//                 }
//             }
//             return bestTorch;                                  // 可能为 null
//         }


//         //——————————————————————————
//         // 静态方法：返回“最近一个半径 <= radius”范围内的火把（不管是否点燃）
//         // 如果找不到则返回 null
//         //——————————————————————————
//         public static Torch GetNearestInRadius(Vector3 playerPos, float radius)
//         {
//             Torch bestTorch = null;
//             float bestSqr = radius * radius;

//             foreach (var t in Object.FindObjectsOfType<Torch>())
//             {
//                 float sqr = (t.transform.position - playerPos).sqrMagnitude;
//                 if (sqr <= bestSqr)
//                 {
//                     bestSqr = sqr;
//                     bestTorch = t;
//                 }
//             }
//             return bestTorch;
//         }

//         //——————————————————————————
//         // 当在编辑器里修改了 Inspector 中的 isOn / torchOnSprite / torchOffSprite 时
//         // 立刻在 Scene 视图里更新 SpriteRenderer 与 Light2D
//         //——————————————————————————
//         private void OnValidate()
//         {
//             if (spriteRenderer != null && torchOnSprite != null && torchOffSprite != null)
//             {
//                 spriteRenderer.sprite = isOn ? torchOnSprite : torchOffSprite;
//             }
//             if (light2d != null)
//             {
//                 light2d.intensity = isOn ? onLightIntensity : offLightIntensity;
//             }
//             if (animator != null)
//             {
//                 animator.SetBool(kIsOnParam, isOn);
//             }
//         }
//     }
// }
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
        [Header("初始是否点燃")]
        public bool isOn = true;

        [Header("必要：拖拽两张 Sprite 到这两个槽里（仅用于编辑器里预览）")]
        public Sprite torchOnSprite;
        public Sprite torchOffSprite;

        [Header("灯光强度设置（点燃时／熄灭时）")]
        public float onLightIntensity = 1f;
        public float offLightIntensity = 0f;

        [Header("提示（可选）")]
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

        // 修改后的灯光控制方法 - 完全禁用组件
        private void ApplyLightIntensity(bool on)
        {
            if (light2d != null)
            {
                // 直接启用/禁用整个Light2D组件
                light2d.enabled = on;
                
                // 如果开启状态，设置正确的强度
                if(on) 
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
            
            // 修改后的编辑器预览处理
            if (light2d != null)
            {
                // 使用 enabled 属性控制灯光开关
                light2d.enabled = isOn;
                
                // 如果开启状态，设置正确的强度
                if(isOn) 
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