using UnityEngine;
using UnityEngine.SceneManagement;
using GameCore;    // 自己的命名空间
using UI;          // 引用你 UI 包里的 UIManager

namespace GameCore
{
    public enum GameState
    {
        Menu,
        LevelSelect,
        Playing,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }
        public GameState State { get; private set; }

        // 下一个要加载的关卡 Scene 名
        private string nextSceneName;
        // 切场景之后再切 UI
        private bool changingState;

        private void Awake()
        {
            // 单例
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }
            I = this;
            DontDestroyOnLoad(gameObject);

            // 订阅世界事件
            GameEvents.OnDoorOpened += OnDoorOpened;
            GameEvents.OnPlayerDied += OnPlayerDied;

            // 监听场景加载完
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            GameEvents.OnDoorOpened -= OnDoorOpened;
            GameEvents.OnPlayerDied -= OnPlayerDied;
        }

        /// <summary>
        /// 从选关或重试按钮调用，启动新关
        /// </summary>
        public void StartLevel(string sceneName)
        {
            nextSceneName = sceneName;
            ChangeState(GameState.Playing);
        }


        private void OnDoorOpened(string nextScene)
        {
            // 解锁下一个关卡
            var cur = SceneManager.GetActiveScene().name;
            if (cur.StartsWith("Level") && int.TryParse(cur.Substring(5), out int n))
            {
                PlayerPrefs.SetInt("Level" + (n + 1), 1);
            }

            // 直接开始下一关
            StartLevel(nextScene);   // ← 复用你已有的启动关卡方法
        }

        // 死亡时调用：直接切到 GameOver
        private void OnPlayerDied()
        {
            ChangeState(GameState.GameOver);
        }
        
        public void ChangeState(GameState newState)
        {
            State = newState;
            changingState = true;  // 添加状态切换标记
            
            switch (newState)
            {
                case GameState.Menu:
                    SceneManager.LoadScene("MainMenu");
                    // 移除 UIManager.I.ShowMenu();
                    break;
                    
                case GameState.LevelSelect:
                    SceneManager.LoadScene("LevelSelect");
                    // 移除 UIManager.I.ShowLevelSelect();
                    break;
                    
                case GameState.Playing:
                    if (!string.IsNullOrEmpty(nextSceneName))
                    {
                        SceneManager.LoadScene(nextSceneName);
                        // 移除 UIManager.I.ShowHUD();
                    }
                    break;
                    
                case GameState.GameOver:
                    SceneManager.LoadScene("GameOver");
                    // 移除 UIManager.I.ShowGameOver();
                    break;
            }
        }
    
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!changingState) return;
            changingState = false;
            
            // 添加空检查确保安全
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

        
    }
}
