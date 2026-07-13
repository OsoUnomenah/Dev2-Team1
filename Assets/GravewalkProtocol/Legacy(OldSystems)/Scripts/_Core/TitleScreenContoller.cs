using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenController : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private string gameplaySceneName;
    [SerializeField] private string creditsSceneName;
    [SerializeField] private bool loadNextSceneIfGameplayNameEmpty = true;

    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject mainMenuPanel;

    [Header("Keyboard Shortcut")]
    [SerializeField] private bool spaceStartsGame = true;


    private void Awake()
    {
        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        if (spaceStartsGame && Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (!string.IsNullOrEmpty(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName);
            return;
        }

        if (loadNextSceneIfGameplayNameEmpty)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        Debug.LogWarning("No gameplay scene name assigned on TitleScreenController.");
    }

    public void OpenCredits()
    {
        if (!string.IsNullOrEmpty(creditsSceneName))
        {
            SceneManager.LoadScene(creditsSceneName);
            return;
        }

        Debug.LogWarning("No credits scene name assigned on TitleScreenController.");
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("No settings panel assigned on TitleScreenController.");
        }
    }

    public void ShowMainMenu()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

}