// using UnityEngine;
// using UnityEngine.UI;
// using GameCore;

// namespace UI
// {
//     public class UIManager : MonoBehaviour
//     {
//         public static UIManager I { get; private set; }

//         /*―――― Canvas ――――*/
//         [Header("—— Canvas 引用 ——")]
//         public Canvas mainMenu;
//         public Canvas levelSelect;
//         public Canvas hud;
//         public Canvas gameOver;
//         public Canvas deathCanvas;    // “YOU DIED” 叠层
        

//                 // ---------- ① 声明按钮 ----------
//         [Header("—— Instruction ——")]
//         public Button instructionButton;


//         /*―――― Buttons ――――*/
//         [Header("—— 主菜单按钮 ——")]
//         public Button playButton;
//         public Button quitButton;

//         [Header("—— GameOver / Death 按钮 ——")]
//         public Button retryButton;
//         public Button menuButton;
//         public Button restartButton;  // 死亡层上的 Restart


//          [Header("—— Pause 菜单按钮 ——")]
//          public Canvas pauseMenu;            // 半透明暂停层
//          public Button pauseButton;

//         /*—— GameOver（通关）按钮 ——*/
//         [Header("—— GameEnd Buttons ——")]
//         public Button winMenuButton;   // 返回主菜单
//         public Button winQuitButton;   // 退出游戏


//         private void Awake()
//         {
//             if (I != null && I != this) { Destroy(gameObject); return; }
//             I = this;

//             /* 挂按钮回调 */
//             if (playButton) playButton.onClick.AddListener(() => GameManager.I.ChangeState(GameState.LevelSelect));
//             if (quitButton) quitButton.onClick.AddListener(Application.Quit);
//             if (retryButton) retryButton.onClick.AddListener(() => GameManager.I.RestartLevel());
//             if (menuButton) menuButton.onClick.AddListener(() => GameManager.I.ChangeState(GameState.Menu));
//             if (restartButton) restartButton.onClick.AddListener(() => GameManager.I.RestartLevel());
//             // ☆ 替换原来的直接跳转：
//             if (instructionButton)
//                 instructionButton.onClick.AddListener(
//                     () => GameEvents.OnInstructionRequested?.Invoke()
//                 );
//             if (winMenuButton) winMenuButton.onClick.AddListener(() => GameManager.I.ChangeState(GameState.Menu));
//             if (winQuitButton) winQuitButton.onClick.AddListener(Application.Quit);
//             if (pauseButton)           pauseButton.onClick.AddListener(() => GameManager.I.Pause());
//         }

//         /*―――― 场景 UI 切换 ――――*/
//         public void ShowMenu()
//         {
//             Set(mainMenu, true);
//             Set(levelSelect, false);
//             Set(hud, false);
//             Set(gameOver, false);
//             Set(deathCanvas, false);
//             Set(pauseMenu,   false);
//         }

//         public void ShowLevelSelect()
//         {
//             Set(mainMenu, false);
//             Set(levelSelect, true);
//             Set(hud, false);
//             Set(gameOver, false);
//             Set(deathCanvas, false);
//             Set(pauseMenu,   false);
//         }

//         public void ShowHUD()
//         {
//             Set(mainMenu, false);
//             Set(levelSelect, false);
//             Set(hud, true);
//             Set(gameOver, false);
//             Set(deathCanvas, false);
//             Set(pauseMenu,   false);
//         }

//         public void ShowGameOver()
//         {
//             Set(mainMenu, false);
//             Set(levelSelect, false);
//             Set(hud, false);
//             Set(gameOver, true);
//             Set(deathCanvas, false);
//             Set(pauseMenu,   false);
//         }

//         public void ShowDeath()
//         {
//             Set(mainMenu, false);
//             Set(levelSelect, false);
//             Set(hud, false);
//             Set(gameOver, false);
//             Set(deathCanvas, true);
//             Set(pauseMenu,   false);
//         }

//         /*―――― 小工具：安全启/关 Canvas ――――*/
//         private void Set(Canvas c, bool on)
//         {
//             if (c != null) c.enabled = on;
//         }

//         /* （可选）钥匙计数刷新 */
//         public Text keyCounterText;
//         public void UpdateKeyUI(int count)
//         {
//             if (keyCounterText) keyCounterText.text = count.ToString();
//         }
//     }
// }
using UnityEngine;
using UnityEngine.UI;
using GameCore;                        // 用于访问 GameManager

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager I { get; private set; }

        /*──────── Canvas 引用 ────────*/
        [Header("—— Canvas ——")]
        public Canvas mainMenu;
        public Canvas levelSelect;
        public Canvas hud;
        public Canvas pauseMenu;       // ← 半透明暂停层
        public Canvas gameOver;
        public Canvas deathCanvas;     // “YOU DIED” 叠层

        /*──────── 按钮引用 ────────*/
        [Header("—— 主菜单按钮 ——")]
        public Button playButton;
        public Button quitButton;

        [Header("—— GameOver / Death 按钮 ——")]
        public Button retryButton;
        public Button menuButton;
        public Button restartButton;   // 死亡层 Restart

        [Header("—— Pause ——")]
        public Button pauseButton;     // HUD 右上角 ⏸

        [Header("—— Instruction ——")]
        public Button instructionButton;

        [Header("—— GameEnd 按钮 ——")]
        public Button winMenuButton;
        public Button winQuitButton;

        /*──────── 其它 UI（可选） ────────*/
        public Text keyCounterText;

        /*──────────────────────────────*/
        private void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;

            /* 主菜单 */
            playButton?.onClick.AddListener(() => GameManager.I.ChangeState(GameState.LevelSelect));
            quitButton?.onClick.AddListener(Application.Quit);

            /* 死亡 / GameOver */
            retryButton ?.onClick.AddListener(() => GameManager.I.RestartLevel());
            menuButton  ?.onClick.AddListener(() => GameManager.I.ChangeState(GameState.Menu));
            restartButton?.onClick.AddListener(() => GameManager.I.RestartLevel());

            /* 胜利层 */
            winMenuButton?.onClick.AddListener(() => GameManager.I.ChangeState(GameState.Menu));
            winQuitButton?.onClick.AddListener(Application.Quit);

            /* Instruction */
            instructionButton?.onClick.AddListener(() => GameEvents.OnInstructionRequested?.Invoke());

            /* Pause 打开按钮 */
            pauseButton?.onClick.AddListener(() => GameManager.I.Pause());
        }

        /*──────── Canvas 切换 API ────────*/
        public void ShowMenu()        => Switch(mainMenu);
        public void ShowLevelSelect() => Switch(levelSelect);
        public void ShowHUD()         => Switch(hud);
        public void ShowPause()       => Switch(pauseMenu);
        public void ShowGameOver()    => Switch(gameOver);
        public void ShowDeath()       => Switch(deathCanvas);

        /*──────── 私有工具 ────────*/
        private void Switch(Canvas active)
        {
            Set(mainMenu,    active == mainMenu);
            Set(levelSelect, active == levelSelect);
            Set(hud,         active == hud);
            Set(pauseMenu,   active == pauseMenu);
            Set(gameOver,    active == gameOver);
            Set(deathCanvas, active == deathCanvas);
        }
        private static void Set(Canvas c, bool on) { if (c) c.enabled = on; }

        public void UpdateKeyUI(int count)
        {
            if (keyCounterText) keyCounterText.text = count.ToString();
        }
    }
}

