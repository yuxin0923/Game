using UnityEngine;
using UnityEngine.UI;
using GameCore;
using World;

namespace Level
{
    /// <summary>
    /// 单关管理器：放在每个关卡场景里的空 GameObject 上，用来显示钥匙数、监听通关事件、解锁下一关
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [Header("HUD 上的钥匙计数 (可留空)")]
        public Text keyCounterText;

        private int collected = 0;

        private void Start()
        {
            // 监听世界事件
            GameEvents.OnKeyCollected += OnKeyCollected;
            //GameEvents.OnDoorOpened   += OnDoorOpened;
            RefreshUI();
        }

        private void OnKeyCollected()
        {
            collected++;
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (keyCounterText != null)
                keyCounterText.text = collected.ToString();
        }

        // private void OnDoorOpened()
        // {
        //     var cur = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        //     // 假设场景名格式 "LevelX"
        //     if (int.TryParse(cur.Replace("Level", ""), out int n))
        //     {
        //         string next = "Level" + (n + 1);
        //         PlayerPrefs.SetInt(next + "_Unlocked", 1);
        //     }
        // }

        private void OnDestroy()
        {
            GameEvents.OnKeyCollected -= OnKeyCollected;
            // GameEvents.OnDoorOpened   -= OnDoorOpened;
        }
    }
}
