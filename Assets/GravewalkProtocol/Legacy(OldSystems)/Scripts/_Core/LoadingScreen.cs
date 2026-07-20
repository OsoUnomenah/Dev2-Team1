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

    [Header("Loading Settings")]
    [Min(0f)]
    [SerializeField] private float minimumLoadingTime = 2f;

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
            Debug.LogError(
                $"LoadingScreen: Failed to load scene '{sceneName}'."
            );

            yield break;
        }

        loadingOperation.allowSceneActivation = false;

        float timer = 0f;

        while (!loadingOperation.isDone)
        {
            timer += Time.unscaledDeltaTime;

            float realProgress =
                Mathf.Clamp01(loadingOperation.progress / 0.9f);

            float timedProgress =
                minimumLoadingTime <= 0f
                    ? 1f
                    : Mathf.Clamp01(timer / minimumLoadingTime);

            float displayedProgress =
                Mathf.Min(realProgress, timedProgress);

            if (progressBar != null)
            {
                progressBar.value = displayedProgress;
            }

            if (percentageText != null)
            {
                percentageText.text =
                    Mathf.RoundToInt(displayedProgress * 100f) + "%";
            }

            bool sceneReady =
                loadingOperation.progress >= 0.9f;

            bool minimumTimePassed =
                timer >= minimumLoadingTime;

            if (sceneReady && minimumTimePassed)
            {
                if (progressBar != null)
                {
                    progressBar.value = 1f;
                }

                if (percentageText != null)
                {
                    percentageText.text = "100%";
                }

                yield return new WaitForSecondsRealtime(0.2f);

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