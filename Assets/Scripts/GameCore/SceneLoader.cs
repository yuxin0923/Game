using UnityEngine.SceneManagement;

namespace GameCore
{
    /// <summary>
    /// 封装 UnityEngine.SceneManagement.SceneManager.LoadScene，
    /// 以后要加过渡、异步，改这里即可。
    /// </summary>
    public static class SceneLoader
    {
        public static void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
