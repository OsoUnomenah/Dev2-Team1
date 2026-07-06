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


    [Header("Events")]
    public GameEvent GE_OnPlayerHealthChanged;
    public GameEvent GE_OnPlayerStaminaChanged;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitHealth();
        InitStamina();

        currentDamage = damage + modDamage;
        modJumps = 1;
    }

    // Update is called once per frame
    void Update()
    {   
        currentDamage = damage + modDamage;
        
        HandleSprint();

    }

    private void InitHealth()
    {
        maxHealth = health + modHealth;
        currentHealth = maxHealth;
        GE_OnPlayerHealthChanged.Raise(this, gameManager.instance.playerStatHandler);

    }

    private void InitStamina()
    {
        maxStamina = stamina + modStamina;
        currentStamina = maxStamina;
        sprintCost = gameManager.instance.sprintCost;
        GE_OnPlayerStaminaChanged.Raise(this, gameManager.instance.playerStatHandler);
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
            GE_OnPlayerStaminaChanged.Raise(this, gameManager.instance.playerStatHandler);

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
            GE_OnPlayerStaminaChanged.Raise(this, gameManager.instance.playerStatHandler);

            if (currentStamina >= maxStamina)
            {
             gameManager.instance.canSprint = true;
            
            }
        }
        GE_OnPlayerStaminaChanged.Raise(this, gameManager.instance.playerStatHandler);
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

        //Raise Event to update health UI and trigger any other responses to health change
        GE_OnPlayerHealthChanged.Raise(this, gameManager.instance.playerStatHandler);

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
        currentHealth += Mathf.Clamp(amount, 0, maxHealth);
        StartCoroutine(FlashHeal());
        if(currentHealth > maxHealth){ currentHealth = maxHealth; }

        //Raise Event to update health UI and trigger any other responses to health change
        GE_OnPlayerHealthChanged.Raise(this, gameManager.instance.playerStatHandler);

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

