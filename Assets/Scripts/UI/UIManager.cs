// using UnityEngine;
// using UnityEngine.UI;
// using GameCore;

// namespace UI
// {
//     /// <summary>
//     /// 全局界面管理：负责主菜单、选关、游戏中 HUD、GameOver 四个 Canvas 的切换，
//     /// 以及主菜单／失败界面的按钮回调。
//     /// </summary>
//     public class UIManager : MonoBehaviour
//     {
//         public static UIManager I { get; private set; }

//         [Header("—— Canvas 引用 ——")]
//         public Canvas mainMenu;      // 主菜单
//         public Canvas levelSelect;   // 关卡选择
//         public Canvas hud;           // 游戏中 HUD（钥匙数、血条等）
//         public Canvas gameOver;      // 失败／通关后界面

//         [Header("—— 主菜单 按钮 ——")]
//         public Button playButton;    // “开始” 按钮，进关卡选择
//         public Button quitButton;    // “退出” 按钮，退出游戏

//         [Header("—— GameOver 界面 按钮 ——")]
//         public Button retryButton;   // “重试” 按钮，重新开始本关
//         public Button menuButton;    // “主菜单” 按钮，返回主菜单

//         private void Awake()
//         {
//             // 单例
//             if (I != null && I != this)
//             {
//                 Destroy(gameObject);
//                 return;
//             }
//             I = this;
//             //DontDestroyOnLoad(gameObject);


//             // // 添加保护性检查
//             // if (mainMenu == null) Debug.LogError("MainMenu canvas not assigned!");
//             // if (levelSelect == null) Debug.LogError("LevelSelect canvas not assigned!");
//             // if (hud == null) Debug.LogError("HUD canvas not assigned!");
//             // if (gameOver == null) Debug.LogError("GameOver canvas not assigned!");

//             // 挂按钮回调
//             if (playButton != null)
//                 playButton.onClick.AddListener(() => GameManager.I.ChangeState(GameState.LevelSelect));

//             if (quitButton != null)
//                 quitButton.onClick.AddListener(Application.Quit);

//             if (retryButton != null)
//                 retryButton.onClick.AddListener(() => GameManager.I.ChangeState(GameState.Playing));

//             if (menuButton != null)
//                 menuButton.onClick.AddListener(() => GameManager.I.ChangeState(GameState.Menu));
//         }

//         /// <summary>显示主菜单，隐藏其它所有界面</summary>
//         public void ShowMenu()
//         {
//             mainMenu.enabled = true;
//             levelSelect.enabled = false;
//             hud.enabled = false;
//             gameOver.enabled = false;
//         }

//         /// <summary>显示关卡选择，隐藏其余</summary>
//         public void ShowLevelSelect()
//         {
//             mainMenu.enabled = false;
//             levelSelect.enabled = true;
//             hud.enabled = false;
//             gameOver.enabled = false;
//         }

//         /// <summary>显示游戏中 HUD，隐藏其余</summary>
//         public void ShowHUD()
//         {
//             mainMenu.enabled = false;
//             levelSelect.enabled = false;
//             hud.enabled = true;
//             gameOver.enabled = false;
//         }

//         /// <summary>显示失败/通关界面，隐藏其余</summary>
//         public void ShowGameOver()
//         {
//             mainMenu.enabled = false;
//             levelSelect.enabled = false;
//             hud.enabled = false;
//             gameOver.enabled = true;
//         }
//     }
// }
using UnityEngine;
using UnityEngine.UI;
using GameCore;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager I { get; private set; }

        [Header("—— Canvas 引用 ——")]
        public Canvas mainMenu;
        public Canvas levelSelect;
        public Canvas hud;
        public Canvas gameOver;

        [Header("—— 主菜单 按钮 ——")]
        public Button playButton;
        public Button quitButton;

        [Header("—— GameOver 按钮 ——")]
        public Button retryButton;
        public Button menuButton;

        private void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;

            if (playButton  != null) playButton .onClick.AddListener(() => GameManager.I.ChangeState(GameState.LevelSelect));
            if (quitButton  != null) quitButton .onClick.AddListener(Application.Quit);
            if (retryButton != null) retryButton.onClick.AddListener(() => GameManager.I.ChangeState(GameState.Playing));
            if (menuButton  != null) menuButton .onClick.AddListener(() => GameManager.I.ChangeState(GameState.Menu));
        }

        /* ----------  下面四个方法只加了一行 if 判空 ---------- */

        public void ShowMenu()
        {
            if (mainMenu    ) mainMenu.enabled    = true;
            if (levelSelect ) levelSelect.enabled = false;
            if (hud         ) hud.enabled         = false;
            if (gameOver    ) gameOver.enabled    = false;
        }

        public void ShowLevelSelect()
        {
            if (mainMenu    ) mainMenu.enabled    = false;
            if (levelSelect ) levelSelect.enabled = true;
            if (hud         ) hud.enabled         = false;
            if (gameOver    ) gameOver.enabled    = false;
        }

        public void ShowHUD()
        {
            if (mainMenu    ) mainMenu.enabled    = false;
            if (levelSelect ) levelSelect.enabled = false;
            if (hud         ) hud.enabled         = true;
            if (gameOver    ) gameOver.enabled    = false;
        }

        public void ShowGameOver()
        {
            if (mainMenu    ) mainMenu.enabled    = false;
            if (levelSelect ) levelSelect.enabled = false;
            if (hud         ) hud.enabled         = false;
            if (gameOver    ) gameOver.enabled    = true;
        }
    }
}
