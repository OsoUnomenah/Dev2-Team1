using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    public void resume()
    {
        gameManager.instance.PauseGame();
    }
    public void restart()
    {
        if (AudioManager.instance.UISound)
        {
            StartCoroutine(WaitForUISoundRestart());
        }
        else
        {
            gameManager.instance.stateUnpause();
            gameManager.instance.respawnPlayer();
            SceneManager.LoadScene(gameManager.instance.hubLevel);
            
        }
    }

    public void settings()
    {
        gameManager.instance.settings();
    }

    public void quit()
    {
        if (AudioManager.instance.UISound)
        {
            StartCoroutine(WaitForUISoundQuit());
        }
        else
        {

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }

    public void back()
    {
        gameManager.instance.back();
    }

    public void respawn()
    {
        gameManager.instance.respawnPlayer();
        gameManager.instance.stateUnpause();
    }

    public void nextLevel()
    {
        if (AudioManager.instance.UISound)
        {
            StartCoroutine(WaitForUISoundNextLevel());
        }
        else
        {
            gameManager.instance.NextLevel();
        }

        
    }

    IEnumerator WaitForUISoundRestart()
    {
        float clipLength = AudioManager.instance.amSource.clip.length + 2;
        yield return new WaitForSeconds(clipLength);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gameManager.instance.stateUnpause();
    }

    IEnumerator WaitForUISoundQuit()
    {
        float clipLength = AudioManager.instance.amSource.clip.length + 2;
        yield return new WaitForSeconds(clipLength);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator WaitForUISoundNextLevel()
    {
        float clipLength = AudioManager.instance.amSource.clip.length + 2;
        yield return new WaitForSeconds(clipLength);

        gameManager.instance.NextLevel();
    }
}
