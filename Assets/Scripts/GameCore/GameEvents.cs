using System;

namespace GameCore
{
    public static class GameEvents
    {
        /// <summary>玩家死亡时触发</summary>
        public static Action OnPlayerDied;

        /// <summary>钥匙收集时触发</summary>
        public static Action OnKeyCollected;

        /// <summary>门打开时触发</summary>
        // public static Action OnDoorOpened;

        public static Action<string> OnDoorOpened;   // <— 带 string

        /* ☆ 新增：主菜单点 Instruction 时触发，没有参数 */
        public static Action OnInstructionRequested;     // ← 就这一行
    }
}
