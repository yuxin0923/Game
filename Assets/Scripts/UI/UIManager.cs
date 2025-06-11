using UnityEngine;
using UnityEngine.UI;
using GameCore;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager I { get; private set; }

        /*―――― Canvas ――――*/
        [Header("—— Canvas 引用 ——")]
        public Canvas mainMenu;
        public Canvas levelSelect;
        public Canvas hud;
        public Canvas gameOver;
        public Canvas deathCanvas;    // “YOU DIED” 叠层

                // ---------- ① 声明按钮 ----------
        [Header("—— Instruction ——")]
        public Button instructionButton;


        /*―――― Buttons ――――*/
        [Header("—— 主菜单按钮 ——")]
        public Button playButton;
        public Button quitButton;

        [Header("—— GameOver / Death 按钮 ——")]
        public Button retryButton;
        public Button menuButton;
        public Button restartButton;  // 死亡层上的 Restart

        private void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;

            /* 挂按钮回调 */
            if (playButton) playButton.onClick.AddListener(() => GameManager.I.ChangeState(GameState.LevelSelect));
            if (quitButton) quitButton.onClick.AddListener(Application.Quit);
            if (retryButton) retryButton.onClick.AddListener(() => GameManager.I.RestartLevel());
            if (menuButton) menuButton.onClick.AddListener(() => GameManager.I.ChangeState(GameState.Menu));
            if (restartButton) restartButton.onClick.AddListener(() => GameManager.I.RestartLevel());
                // ☆ 替换原来的直接跳转：
            if (instructionButton)
                instructionButton.onClick.AddListener(
                    () => GameEvents.OnInstructionRequested?.Invoke()
                );
        }

        /*―――― 场景 UI 切换 ――――*/
        public void ShowMenu()
        {
            Set(mainMenu,    true);
            Set(levelSelect, false);
            Set(hud,         false);
            Set(gameOver,    false);
            Set(deathCanvas, false);
        }

        public void ShowLevelSelect()
        {
            Set(mainMenu,    false);
            Set(levelSelect, true);
            Set(hud,         false);
            Set(gameOver,    false);
            Set(deathCanvas, false);
        }

        public void ShowHUD()
        {
            Set(mainMenu,    false);
            Set(levelSelect, false);
            Set(hud,         true);
            Set(gameOver,    false);
            Set(deathCanvas, false);
        }

        public void ShowGameOver()
        {
            Set(mainMenu,    false);
            Set(levelSelect, false);
            Set(hud,         false);
            Set(gameOver,    true);
            Set(deathCanvas, false);
        }

        public void ShowDeath()
        {
            Set(mainMenu,    false);
            Set(levelSelect, false);
            Set(hud,         false);
            Set(gameOver,    false);
            Set(deathCanvas, true);
        }

        /*―――― 小工具：安全启/关 Canvas ――――*/
        private void Set(Canvas c, bool on)
        {
            if (c != null) c.enabled = on;
        }

        /* （可选）钥匙计数刷新 */
        public Text keyCounterText;
        public void UpdateKeyUI(int count)
        {
            if (keyCounterText) keyCounterText.text = count.ToString();
        }
    }
}

