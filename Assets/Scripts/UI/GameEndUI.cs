// Assets/Scripts/UI/EndGameMenu.cs
using UnityEngine;
using UnityEngine.UI;
using GameCore;                  // 用到 GameManager.I
using UnityEngine.SceneManagement;

namespace UI
{
    /// <summary>胜利场景（GameEnd）里的两个按钮。</summary>
    public class EndGameMenu : MonoBehaviour
    {
        [Header("Buttons")]
        public Button btnMainMenu;   // 返回主菜单
        public Button btnQuit;       // 退出游戏

        private void Awake()
        {
            if (btnMainMenu != null)
                btnMainMenu.onClick.AddListener(
                    () => GameManager.I.ChangeState(GameState.Menu));

            if (btnQuit != null)
                btnQuit.onClick.AddListener(QuitGame);
        }

        private static void QuitGame()
        {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;   // 仅在编辑器
    #else
            Application.Quit();                                // 真机 / PC 版
    #endif
        }
    }
}
