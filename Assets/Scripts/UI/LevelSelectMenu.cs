using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace UI
{
    [System.Serializable]
    public struct LevelInfo
    {
        public string sceneName; // 要加载的 Scene 名称（Build Settings 里也要添加）
        public Button button;    // Inspector 里把对应的 Button 拖进来
    }

    /// <summary>
    /// 关卡选择菜单：只管按钮的锁定/点击，放在 UI 包里更直观
    /// </summary>
    public class LevelSelectMenu : MonoBehaviour
    {
        [Header("关卡列表 (SceneName + Button)")]
        public List<LevelInfo> levels;

        [Header("第一个关卡默认解锁")]
        public bool firstUnlocked = true;

        private void Start()
        {
            for (int i = 0; i < levels.Count; i++)
            {
                var info = levels[i];
                bool unlocked = (i == 0 && firstUnlocked)
                    || PlayerPrefs.GetInt(info.sceneName + "_Unlocked", 0) == 1;

                info.button.interactable = unlocked;

                string sn = info.sceneName;  // 防止闭包引用问题
                info.button.onClick.AddListener(() => SceneManager.LoadScene(sn));
            }
        }
    }
}
