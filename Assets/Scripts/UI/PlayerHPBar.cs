using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : MonoBehaviour
{
    [Header("HP Bar Config")]
    [SerializeField] public Slider playerHpBar;
    [SerializeField] public TMP_Text playerHpBarText;
    public int currentHealth;
    public int maxHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitHealth();
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitHealth()
    {
        currentHealth = (int)gameManager.instance.playerStatHandler.currentHealth;
        maxHealth = (int)gameManager.instance.playerStatHandler.maxHealth;

        playerHpBarText.text = " HP: " + Mathf.Clamp(currentHealth, 0, maxHealth) + " / " + maxHealth;
        playerHpBar.value = 1;

    }

    public void UpdatePlayerHealthBarUI()
    {
        currentHealth = (int)gameManager.instance.playerStatHandler.currentHealth;
        maxHealth = (int)gameManager.instance.playerStatHandler.maxHealth;

        playerHpBarText.text = " HP: " + currentHealth + " / " + maxHealth;
        playerHpBar.value = (float)Mathf.Clamp(currentHealth, 0, maxHealth) / maxHealth;
    }
}
