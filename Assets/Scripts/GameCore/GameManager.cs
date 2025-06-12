/*
GameManager.cs — Central Game Flow Controller
=====================================================
Purpose
-------
`GameManager` owns the high-level state machine that drives the entire game loop: **Main Menu → Level Select → Playing → Paused / Died → Game Over**.  It also exposes a singleton handle (`GameManager.I`) so any script—from UI buttons to world triggers—can request a state change in one line.

Key Responsibilities
--------------------
1. **Finite-State Machine (`GameState`)** – decides which scene and UI screen should be active.
2. **Scene Loading** – calls `SceneManager.LoadScene()` when a new level or menu is required.
3. **Time Control** – toggles `Time.timeScale` for global pause / resume.
4. **Event Hub** – subscribes to `GameEvents` (door opened, player died, etc.) and reacts accordingly.
5. **UI Bridge** – informs `UIManager` which canvas to show once the new scene finishes loading.

Why This Design Helps Maintenance
---------------------------------
* **Add a New Game Phase Easily** – just append a value to `GameState`, handle it in `ChangeState`, and (optionally) add a UI panel.  No other script needs editing.
* **Menu & Gameplay Decoupled** – gameplay scenes never reference UI canvases directly; they only raise events.  UI can be redesigned without touching core game code.
* **Single Source of Truth** – pause, restart, or quit logic lives in one place, preventing desynchronised states.
* **Scalable to Async Loading** – if you move to `Addressables` or additive scenes, replace the `SceneManager.LoadScene` lines; state logic stays intact.
* **Extensible Events** – new world events (e.g. checkpoint reached) can be wired into the same pattern: raise an event → listen in `GameManager` → call `ChangeState`.
*/
using UnityEngine;
using UnityEngine.SceneManagement;
using GameCore;   
using UI;          

namespace GameCore
{
    public enum GameState
    {
        Menu,
        LevelSelect,
        Playing,
        Died, 
        Paused,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }
        public GameState State { get; private set; }

        // Next level to be loaded Scene name
        private string nextSceneName;
        // Switch UI after changing scene
        private bool changingState;
        private bool isPaused = false; 
        private void SetPaused(bool paused) => Time.timeScale = paused ? 0f : 1f;

        private void Awake()
        {
            // Singleton
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }
            I = this;
            DontDestroyOnLoad(gameObject);

            // Subscribe to world events
            GameEvents.OnDoorOpened += OnDoorOpened;
            GameEvents.OnPlayerDied += OnPlayerDied;
            GameEvents.OnInstructionRequested += OnInstructionRequested;

            // Listen for scene loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            GameEvents.OnDoorOpened -= OnDoorOpened;
            GameEvents.OnPlayerDied -= OnPlayerDied;
            GameEvents.OnInstructionRequested -= OnInstructionRequested;
        }

        /// <summary>
        /// Called from level select or retry button to start a new level
        /// </summary>
        public void StartLevel(string sceneName)
        {
            nextSceneName = sceneName;
            ChangeState(GameState.Playing);
        }
                /*──────── Pause / Resume ────────*/
        public void Pause()
        {
            if (State != GameState.Playing || isPaused) return;

            isPaused = true;
            SetPaused(true);
            UIManager.I.ShowPause();
        }

        public void Resume()
        {
            if (!isPaused) return;

            isPaused = false;
            SetPaused(false);
            UIManager.I.ShowHUD();
        }



        private void OnDoorOpened(string nextScene)
        {

            if (nextScene == "GameEnd")
            {
                ChangeState(GameState.GameOver); // Will LoadScene("GameEnd") in the case below
                return;
            }
            // Unlock next level
            var cur = SceneManager.GetActiveScene().name;
            if (cur.StartsWith("Level") && int.TryParse(cur.Substring(5), out int n))
            {
                PlayerPrefs.SetInt("Level" + (n + 1), 1);
            }



            // Directly start the next level
            StartLevel(nextScene);   
        }

        // Called when player dies: directly switch to GameOver

        private void OnPlayerDied()
        {
            // No scene cuts, just state cuts
            ChangeState(GameState.Died);
        }

        public void ChangeState(GameState newState, string levelName = null)
        {
            State = newState;
            changingState = true;  // Add state change flag

            switch (newState)
            {
                case GameState.Menu:
                    SceneManager.LoadScene("MainMenu");
                    // Remove UIManager.I.ShowMenu();
                    break;

                case GameState.LevelSelect:
                    SceneManager.LoadScene("LevelSelect");
                    // Remove UIManager.I.ShowLevelSelect();
                    break;

                case GameState.Playing:
                    SetPaused(false);   // Pause the world
                    if (!string.IsNullOrEmpty(nextSceneName))
                    {
                        SceneManager.LoadScene(nextSceneName);
                        // Remove UIManager.I.ShowHUD();
                    }
                    break;

                case GameState.GameOver:
                    SceneManager.LoadScene("GameEnd");       
                    break;

                case GameState.Died:
                    // Stop player input
                    var player = FindObjectOfType<Player.Player>();
                    SetPaused(true);   // Pause the world
                    if (player != null) player.enabled = false;
                    UI.UIManager.I.ShowDeath();   // See below
                    break;
            }
        }




        // ───────────────────────── RestartLevel ─────────────────────────
     
        /// <summary>For UI / keyboard calls, reload the current level.</summary>
        public void RestartLevel()
        {
            // 1. Get the current active scene name
            string cur = SceneManager.GetActiveScene().name;

            // 2. Directly reuse StartLevel(),
            //    This will write to nextSceneName first, then switch to Playing state,
            //    The branch will definitely be able to correctly LoadScene(cur)
            StartLevel(cur);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!changingState) return;
            changingState = false;

            // Add null check for safety
            if (UIManager.I == null)
            {
                Debug.LogWarning("UIManager instance missing in scene");
                return;
            }

            switch (State)
            {
                case GameState.Menu:
                    UIManager.I.ShowMenu();
                    break;
                case GameState.LevelSelect:
                    UIManager.I.ShowLevelSelect();
                    break;
                case GameState.Playing:
                    UIManager.I.ShowHUD();
                    break;
                case GameState.GameOver:
                    UIManager.I.ShowGameOver();
                    break;
            }
        }

        private void OnInstructionRequested()
        {
            SceneLoader.Load("Instruction");   // Reuse encapsulation, future changes to async only affect SceneLoader
        }
        
        
    }
}
