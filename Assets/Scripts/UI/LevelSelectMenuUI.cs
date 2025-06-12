using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace UI
{
    [System.Serializable]
    public struct LevelInfo
    {
        public string sceneName; // Scene name to load (also add in Build Settings)
        public Button button;    
    }

    /// <summary>
    /// Level selection menu: only manages button locking/clicking, more intuitive to put in UI package
    /// </summary>
    public class LevelSelectMenuUI : MonoBehaviour
    {
        [Header("Level List (SceneName + Button)")]
        public List<LevelInfo> levels;

        [Header("First level unlocked by default")]
        public bool firstUnlocked = true;

        private void Start()
        {
            for (int i = 0; i < levels.Count; i++)
            {
                var info = levels[i];
                bool unlocked = (i == 0 && firstUnlocked)
                    || PlayerPrefs.GetInt(info.sceneName + "_Unlocked", 0) == 1;

                info.button.interactable = unlocked;

                string sn = info.sceneName;  // Prevent closure capture issue
                info.button.onClick.AddListener(() => SceneManager.LoadScene(sn));
            }
        }
    }
}
