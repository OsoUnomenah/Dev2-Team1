using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading scenes

public class TitleScreenController : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // Detects if the Spacebar key was pressed down this frame
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadGameplayScene();
        }
    }

    void LoadGameplayScene()
    {
        // Option A: Loads the next scene in the Build Settings order
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

        // Option B: Alternatively, you can load by exact scene name:
        // SceneManager.LoadScene("YourGameplaySceneName");
    }
}
