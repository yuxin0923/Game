// Assets/Scripts/Player/Player.cs
using UnityEngine;
using World; 
 // 引入自定义的 SimplePhysicsBody 脚本
namespace Player
{
    /// 
    /// 角色逻辑层：只暴露 Move / Jump / ToggleTorch 等接口给命令、AI 或关卡触发。
    /// 真正的物理运算交给 SimplePhysicsBody。
    ///
    [RequireComponent(typeof(SimplePhysicsBody))]
    public class Player : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("水平移动速度（m/s）")]
        public float moveSpeed = 8f;

        [Tooltip("起跳初速度（m/s）")]
        public float jumpSpeed = 15f;

        public int keyCount { get; private set; }
        /* 便于 Key 脚本获得包围盒 */
        public Vector2 Pos => transform.position;
        public Vector2 Half => body.halfSize;
        // 新增：对 Flashlight 的引用
        [Header("Flashlight (Charge)")]
        public Flashlight flashlight;         // 拖拽场景中的 Player 上的 Flashlight 组件
        [Tooltip("靠近多远的已点燃火把开始充电（单位：米）")]
        public float rechargeRange = 1.2f;

        /* ---------- 组件引用 ---------- */
        SimplePhysicsBody body;     // 自己写的刚体脚本
        // Flashlight torch;           // 如果没挂手电，可为 null

        void Awake()
        {
            body = GetComponent<SimplePhysicsBody>();
            // torch = GetComponent<Flashlight>();   // 可选组件
        }

        void Update()
        {
            HandleRechargeProximity();
            // 其他输入：Move/Jump/ToggleTorch/Teleport 等
        }

        /* ========== 供 ICommand / AI 调用的接口 ========== */

        /// 水平移动：dir ∈ [-1,1]（左 -1，右 +1，松手 0）
        public void Move(float dir)
        {
            body.velocity.x = dir * moveSpeed;
        }

        /// 跳跃：只有落地时才生效
        public void Jump()
        {
            if (body.grounded)
                body.velocity.y = jumpSpeed;
        }
        /* ------------------------------------------------ */
        /* 这里可以继续加 Dash()、WallJump() 等高级动作接口    */
        /* ------------------------------------------------ */
        public void AddKey()
        {
            keyCount++;
            Debug.Log($"Key collected! total = {keyCount}");
        }

        /* ====== 新增：处理“靠近火把” 充/放电 ====== */
        void HandleRechargeProximity()
        {
            if (flashlight == null) return;   // 如果没挂 Flashlight 组件，直接退出

            // 1. 查找玩家周围是否有“点燃的火把”在圆形范围内
            Torch near = Torch.GetNearestInRadius(transform.position, rechargeRange);
            if (near != null && near.isOn)
            {
                // 只要找到了一个点燃的就开始给手电充电
                flashlight.StartRecharge();
            }
            else
            {
                // 否则离开范围，停止充电
                flashlight.StopRecharge();
            }
        }

        /// 切换最近的火把状态并传送到最近的点燃火把上方
        public void TeleportToNearestBurningTorch(float interactRange = 1.2f)
        {
            /*-----------------------------------------------------------
            * ① 先确认玩家脚下是否真的“站在一盏火把上”
            *    且这盏火把正处于点燃状态；否则瞬移无效
            *----------------------------------------------------------*/
            Torch curr = Torch.GetNearestInRadius(transform.position, interactRange);
            if (curr == null || !curr.isOn)
                return;                                         // A. 没火把 / B. 火把没点燃 → 直接退出

            /*-----------------------------------------------------------
            * ② 在所有火把里寻找“离玩家最近、且不是同一盏”的
            *    其它点燃火把 B
            *----------------------------------------------------------*/
            Torch[] torches = Object.FindObjectsOfType<Torch>();
            Torch target = null;
            float bestSq = float.PositiveInfinity;
            Vector2 me = transform.position;

            foreach (var t in torches)
            {
                if (!t.isOn || t == curr) continue;             // 只考虑点燃 & 非当前这盏
                float sq = ((Vector2)t.transform.position - me).sqrMagnitude;
                if (sq < bestSq)
                {
                    bestSq = sq;
                    target = t;
                }
            }

            if (target == null) return;                        // 场景里没有其它点燃火把 → 不移动

            /*-----------------------------------------------------------
            * ③ 把玩家传送到目标火把正上方一点
            *----------------------------------------------------------*/
            Vector2 half = body.halfSize;
            Vector3 pos = target.transform.position;
            pos.y += half.y + 0.05f;                           // 微抬高避免卡砖
            transform.position = pos;
            body.velocity = Vector2.zero;
        }



        /// 切换最近的火把状态（点燃 ⇄ 熄灭）
        public void ToggleNearestTorch(float radius = 1.2f)
        {
            Torch t = Torch.GetNearestInRadius(transform.position, radius);
            if (t != null)
                t.Switch();                // 点燃 ⇄ 熄灭
        }

        // Player 类里，放在其他 public 方法下面
        /* ====== 伤害处理 ====== */

        public void Die()
        {
            Debug.Log("Player died: out of battery");
            // TODO: 这里可触发 UIManager.GameOver()、播放动画等
            Destroy(gameObject);        // 先用最简单的销毁
        }
        
        
        public void OnAttacked(float amount)
        {
            if (flashlight != null)
            {
                flashlight.ReduceCharge(amount);
            }
            else
            {
                // 没有手电组件时直接死亡
                Die();
            }
        }


    }
}
