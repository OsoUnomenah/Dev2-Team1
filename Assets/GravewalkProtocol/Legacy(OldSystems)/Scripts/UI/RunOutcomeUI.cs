using System.Collections;
using TMPro;
using UnityEngine;

public class RunOutcomeUI : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Outcome Panel")]
    [SerializeField] private GameObject outcomePanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statsText;

    [Header("Action Menus")]
    [SerializeField] private GameObject lossActionMenuPanel;
    [SerializeField] private GameObject winActionMenuPanel;

    [Header("Text")]
    [SerializeField] private string lossTitle = "RUN FAILED";
    [SerializeField] private string winTitle = "RUN CLEARED";

    private bool isShowing;
    private bool showingWinScreen;

    private void Awake()
    {
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.gameObject.SetActive(false);
        }

        if (outcomePanel != null)
        {
            outcomePanel.SetActive(false);
        }

        if (lossActionMenuPanel != null)
        {
            lossActionMenuPanel.SetActive(false);
        }

        if (winActionMenuPanel != null)
        {
            winActionMenuPanel.SetActive(false);
        }
    }

    public void ShowLossScreen()
    {
        if (isShowing)
        {
            return;
        }

        showingWinScreen = false;
        StartCoroutine(ShowOutcomeRoutine(false));
    }

    public void ShowWinScreen()
    {
        if (isShowing)
        {
            return;
        }

        showingWinScreen = true;
        StartCoroutine(ShowOutcomeRoutine(true));
    }

    private IEnumerator ShowOutcomeRoutine(bool wonRun)
    {
        isShowing = true;

        if (fadeGroup != null)
        {
            fadeGroup.gameObject.SetActive(true);
            fadeGroup.alpha = 0f;

            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                fadeGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
                yield return null;
            }

            fadeGroup.alpha = 1f;
        }

        if (gameManager.instance != null)
        {
            gameManager.instance.statePause();
        }

        if (outcomePanel != null)
        {
            outcomePanel.SetActive(true);
        }

        if (lossActionMenuPanel != null)
        {
            lossActionMenuPanel.SetActive(false);
        }

        if (winActionMenuPanel != null)
        {
            winActionMenuPanel.SetActive(false);
        }

        if (titleText != null)
        {
            titleText.text = wonRun ? "RUN CLEARED" : "RUN FAILED";
        }

        if (statsText != null && gameManager.instance != null)
        {
            string outcomeText = wonRun ? "Run Cleared" : "Run Failed";
            statsText.text = gameManager.instance.GetRunStatsText(outcomeText);
        }
    }

    public void ContinueToActionMenu()
    {
        if (outcomePanel != null)
        {
            outcomePanel.SetActive(false);
        }

        if (showingWinScreen)
        {
            if (winActionMenuPanel != null)
            {
                winActionMenuPanel.SetActive(true);
            }
        }
        else
        {
            if (lossActionMenuPanel != null)
            {
                lossActionMenuPanel.SetActive(true);
            }
        }
    }
}