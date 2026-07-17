using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("Loading UI")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text percentageText;
    [SerializeField] private GameObject loadingCanvas;

    private void Start()
    {
        if (progressBar != null)
        {
            progressBar.value = 0f;
        }

        if (percentageText != null)
        {
            percentageText.text = "0%";
        }

        string sceneToLoad =
            PlayerPrefs.GetString("SceneToLoad", string.Empty);

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError(
                "LoadingScreen: No destination scene was provided."
            );

            return;
        }

        StartCoroutine(LoadSceneAsync(sceneToLoad));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation loadingOperation =
            SceneManager.LoadSceneAsync(
                sceneName,
                LoadSceneMode.Single
            );

        if (loadingOperation == null)
        {
            yield break;
        }

        loadingOperation.allowSceneActivation = false;

        while (!loadingOperation.isDone)
        {
            float progress =
                Mathf.Clamp01(loadingOperation.progress / 0.9f);

            if (progressBar != null)
            {
                progressBar.value = progress;
            }

            if (percentageText != null)
            {
                percentageText.text =
                    Mathf.RoundToInt(progress * 100f) + "%";
            }

            if (loadingOperation.progress >= 0.9f)
            {
                if (progressBar != null)
                {
                    progressBar.value = 1f;
                }

                if (percentageText != null)
                {
                    percentageText.text = "100%";
                }

                yield return null;

                if (loadingCanvas != null)
                {
                    loadingCanvas.SetActive(false);
                }

                PlayerPrefs.DeleteKey("SceneToLoad");

                loadingOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}