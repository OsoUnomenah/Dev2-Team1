using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class StatHandler : MonoBehaviour, IDamage
{
    [Range(10f, 500f)][SerializeField] public float health = 100;
    public float currentHealth;
    public float maxHealth;
    public float modHealth = 0;


    [Range(50f, 500f)][SerializeField] public float stamina;
    public float currentStamina;
    public float maxStamina;
    public float modStamina;
    [Range(0, 10)][SerializeField] public float sprintCost;
    [Range(0, 1)][SerializeField] public float sprintGain;
    [Range(0, 1)][SerializeField] public float sprintLoss;



    [Range(0, 1000)][SerializeField] public float damage;
    [Range(0, 1000)][SerializeField] public float modDamage;
    public float currentDamage;

    [Range(0, 100)][SerializeField] public float defense;
    [Range(0, 100)][SerializeField] public float currentDefense;
    [Range(0, 100)][SerializeField] public float modDefense;


    [Range(0f, 100f)][SerializeField] public float modSpeed;
    [Range(0, 100)][SerializeField] public int modJumps;

    public Slider healthBar;
    public TMP_Text healthText;

    public Slider staminaBar;
    public TMP_Text staminaText;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxHealth = health + modHealth;
        currentHealth = maxHealth;
        UpdatePlayerHealthBarUI();

        maxStamina = stamina + modStamina;
        currentStamina = maxStamina;
        sprintCost = gameManager.instance.sprintCost;
        UpdatePlayerStaminaBarUI();

        currentDamage = damage + modDamage;
        modJumps = 1;
    }

    // Update is called once per frame
    void Update()
    {   
        currentDamage = damage + modDamage;
        
        HandleSprint();

    }

    public void UpdatePlayerStaminaBarUI()
    {
        staminaText.text = " STM: " + Mathf.CeilToInt(currentStamina) + " / " + Mathf.CeilToInt(maxStamina);
        staminaBar.value = (float)currentStamina / (float)maxStamina;
    }

    public void UpdatePlayerHealthBarUI()
    {
        healthText.text = " HP: " + currentHealth + " / " + maxHealth;
        healthBar.value = (float)currentHealth / (float)maxHealth;
    }

    public void HandleSprint()
    {
        if (gameManager.instance.SprintTriggered 
            && !gameManager.instance.isSprinting 
            && gameManager.instance.characterController.isGrounded 
            && gameManager.instance.playerInputHandler.currentSpeed != 0)
        {
            currentStamina -= gameManager.instance.sprintCost;
            gameManager.instance.isSprinting = true;
        }

        if (gameManager.instance.isSprinting)
        {
            currentStamina += -sprintLoss;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            if (currentStamina <= 0)
            {
                gameManager.instance.SprintTriggered = false;
                gameManager.instance.isSprinting = false;
                gameManager.instance.canSprint = false;
            }
        }
        else 
        {
            currentStamina += sprintGain;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            if (currentStamina >= maxStamina)
            {
             gameManager.instance.canSprint = true;
            
            }
        }
        UpdatePlayerStaminaBarUI();
    }


    public int EnemyAttack()
    {
        gameManager.instance.enemyDamageOut = (int)currentDamage;
        return (int)currentDamage;
    }




    public void takeDamage(int amount)
    {
        StartCoroutine(FlashDamage());

        StatHandler stats = gameManager.instance.playerStatHandler;

        int defenseBonus = Mathf.RoundToInt(stats.modDefense);

        // Defense reduces incoming damage.
        // Minimum damage is 1 so enemies can still hurt the player.
        int finalDamage = Mathf.Max(1, amount - defenseBonus);

        stats.currentHealth -= Mathf.Clamp(finalDamage, 0, maxHealth);
        stats.UpdatePlayerHealthBarUI();

        if (gameManager.instance.gameDebug)
        {
            Debug.Log("Enemy Damage: " + amount + " - Defense: " + defenseBonus + " = " + finalDamage);
        }

        if (stats.currentHealth <= 0)
        {
            gameManager.instance.youLose();
        }


    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        StartCoroutine(FlashHeal());
        if (currentHealth >  maxHealth)
            currentHealth = maxHealth;
        UpdatePlayerHealthBarUI();

    }
    IEnumerator FlashDamage()
    {
        gameManager.instance.playerDamageFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.playerDamageFlash.SetActive(false);
    }

    IEnumerator FlashHeal()
    {
        gameManager.instance.playerHealFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.playerHealFlash.SetActive(false);
    }

}

