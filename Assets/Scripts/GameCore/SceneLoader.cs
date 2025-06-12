using UnityEngine.SceneManagement;

namespace GameCore
{
    /// <summary>
    /// Wrapping UnityEngine.SceneManagement.SceneManager.LoadScene, 
    /// If you want to add transitions and asynchrony in the future, just change it here.
    /// </summary>
    public static class SceneLoader
    {
        public static void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
