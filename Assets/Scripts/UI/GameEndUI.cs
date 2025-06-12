// Assets/Scripts/UI/EndGameMenu.cs
using UnityEngine;
using UnityEngine.UI;
using GameCore;                  // Use the GameManager.
using UnityEngine.SceneManagement;

namespace UI
{
    /// <summary>Victory scene (GameEnd) with two buttons.</summary>
    public class EndGameMenu : MonoBehaviour
    {
        [Header("Buttons")]
        public Button btnMainMenu;   // Return to main menu
        public Button btnQuit;       // Quit game

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
            UnityEditor.EditorApplication.isPlaying = false;   // Only in editor
    #else
            Application.Quit();                                // Standalone / PC version
    #endif
        }
    }
}
