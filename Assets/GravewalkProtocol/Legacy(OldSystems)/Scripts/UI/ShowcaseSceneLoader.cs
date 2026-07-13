using UnityEngine;
using UnityEngine.SceneManagement;

public class ShowcaseSceneLoader : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string showcaseSceneName = "Showcase";

    public void LoadShowcaseScene()
    {
        if (string.IsNullOrEmpty(showcaseSceneName))
        {
            Debug.LogWarning("Showcase scene name is missing on ShowcaseSceneLoader.");
            return;
        }

        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        SceneManager.LoadScene(showcaseSceneName);
    }
}
