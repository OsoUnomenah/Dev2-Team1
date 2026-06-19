using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSTMBar : MonoBehaviour
{
    [Header("HP Bar Config")]
    [SerializeField] public Slider playerSTMBar;
    [SerializeField] public TMP_Text playerSTMBarText;
    public int currentStamina;
    public int maxStamina;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitStamina();

    }


    private void InitStamina()
    {
        currentStamina = (int)gameManager.instance.playerStatHandler.currentStamina;
        maxStamina = (int)gameManager.instance.playerStatHandler.maxStamina;

        playerSTMBarText.text = " STM: " + Mathf.CeilToInt(currentStamina) + " / " + Mathf.CeilToInt(maxStamina);
        playerSTMBar.value = 1;

    }

    public void UpdatePlayerStamBarUI()
    {
        currentStamina = (int)gameManager.instance.playerStatHandler.currentStamina;
        maxStamina = (int)gameManager.instance.playerStatHandler.maxStamina;

        playerSTMBarText.text = " STM: " + Mathf.CeilToInt(currentStamina) + " / " + Mathf.CeilToInt(maxStamina);
        playerSTMBar.value = (float)Mathf.Clamp(currentStamina, 0, maxStamina) / maxStamina;
    }
}
