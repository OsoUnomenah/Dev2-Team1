using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneLoader : MonoBehaviour
{
    [Header("Loading Scene")]
    [SerializeField] private string loadingSceneName = "LoadingScreen";

    public void LoadScene(string sceneName)
    {
        PlayerPrefs.SetString("SceneToLoad", sceneName);
        PlayerPrefs.Save();

        SceneManager.LoadScene(loadingSceneName);
    }
}
