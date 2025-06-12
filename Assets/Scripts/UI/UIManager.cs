
using UnityEngine;
using UnityEngine.UI;
using GameCore;                       
using TMPro; 

/*
 UIManager.cs — Central HUD / Menu Controller (Singleton + Facade / Mediator)
 ---------------------------------------------------------------------------
 • Role  
   Provides a single switch-board for every in-game canvas (Main Menu, HUD,
   Pause, Game-Over, etc.) and wires Unity-UI buttons to high-level actions
   in other systems.  All scene UIs are therefore driven from one place,
   keeping each canvas prefab “dumb”.

 • Key interactions  
   – Calls GameManager.I.ChangeState / RestartLevel to drive scene flow.  
   – Subscribes to no events directly, but *publishes* UI intent such as
     instruction requests via GameEvents (Observer) so that gameplay
     systems can react without referencing UI code.  
   – Exposed UpdateKeyUI(int) lets the Key pickup logic update the HUD
     counter without needing to know which canvas is currently active.

 • Why this design?  
   1. Singleton ensures exactly one UIManager survives scene loads; every
      other script can reach it via UIManager.I without cached links.  
   2. Facade-style ShowXXX() methods hide the enable/disable bookkeeping
      (Switch + Set) and prevent scattered scene code from touching raw
      Canvas objects.  
   3. Button listeners are attached once in Awake(); swapping a canvas or
      adding a new button only requires editing the inspector, not code.  
   4. Decoupling through GameManager and GameEvents keeps UI free from
      game-play logic, so new states or overlays can be added by extending
      GameState and dropping in another Canvas prefab.
*/

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager I { get; private set; }

        /*──────── Canvas References ────────*/
        [Header("—— Canvas ——")]
        public Canvas mainMenu;
        public Canvas levelSelect;
        public Canvas hud;
        public Canvas pauseMenu;       // ← Translucent pause layer
        public Canvas gameOver;
        public Canvas deathCanvas;     // “YOU DIED” overlay

        /*──────── Button References ────────*/
        [Header("—— Main Menu Buttons ——")]
        public Button playButton;
        public Button quitButton;

        [Header("—— GameOver / Death Buttons ——")]
        public Button retryButton;
        public Button menuButton;
        public Button restartButton;   // Dead layer Restart

        [Header("—— Pause ——")]
        public Button pauseButton;     // HUD top right ⏸

        [Header("—— Instruction ——")]
        public Button instructionButton;

        [Header("—— GameEnd Buttons ——")]
        public Button winMenuButton;
        public Button winQuitButton;

        /*──────── Other UI (optional) ────────*/
        
        public TMP_Text keyCounterText;

        /*──────────────────────────────*/
        private void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;

            /* Main Menu */
            playButton?.onClick.AddListener(() => GameManager.I.ChangeState(GameState.LevelSelect));
            quitButton?.onClick.AddListener(Application.Quit);

            /* Death / GameOver */
            retryButton ?.onClick.AddListener(() => GameManager.I.RestartLevel());
            menuButton  ?.onClick.AddListener(() => GameManager.I.ChangeState(GameState.Menu));
            restartButton?.onClick.AddListener(() => GameManager.I.RestartLevel());

            /* Victory */
            winMenuButton?.onClick.AddListener(() => GameManager.I.ChangeState(GameState.Menu));
            winQuitButton?.onClick.AddListener(Application.Quit);

            /* Instruction */
            instructionButton?.onClick.AddListener(() => GameEvents.OnInstructionRequested?.Invoke());


        }

        /*──────── Canvas Switch API ────────*/
        public void ShowMenu()        => Switch(mainMenu);
        public void ShowLevelSelect() => Switch(levelSelect);
        public void ShowHUD()         => Switch(hud);
        public void ShowPause()       => Switch(pauseMenu);
        public void ShowGameOver()    => Switch(gameOver);
        public void ShowDeath()       => Switch(deathCanvas);

        /*──────── Private tools ────────*/
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

