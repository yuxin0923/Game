// Assets/Scripts/Player/Player.cs
using UnityEngine;
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

        /* ---------- 组件引用 ---------- */
        SimplePhysicsBody body;     // 自己写的刚体脚本
        // Flashlight torch;           // 如果没挂手电，可为 null

        void Awake()
        {
            body  = GetComponent<SimplePhysicsBody>();
            // torch = GetComponent<Flashlight>();   // 可选组件
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

        /// 手电开关（如果场景挂了 Flashlight）
        // public void ToggleTorch()
        // {
        //     if (torch != null)
        //         torch.Switch();
        // }

        /* ------------------------------------------------ */
        /* 这里可以继续加 Dash()、WallJump() 等高级动作接口    */
        /* ------------------------------------------------ */
    }
}
