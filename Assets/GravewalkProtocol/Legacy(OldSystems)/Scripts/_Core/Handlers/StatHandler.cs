using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class StatHandler : MonoBehaviour, IDamage
{
    [Header("HP")]
    [Range(10f, 500f)][SerializeField] public float health = 100;
    public float currentHealth;
    public float maxHealth;
    public float modHealth = 0;

    [Header("Stamina")]
    [Range(50f, 500f)][SerializeField] public float stamina;
    [Range(0, 1)][SerializeField] public float staminaRegen;
    public float currentStamina;
    public float maxStamina;
    public float modStamina;

    [Header("Dash")]
    [Range(0, 10)][SerializeField] public float dashLoss;
    public float dashCost;

    [Header("Melee")]
    [Range(0, 10)][SerializeField] public float meleeLoss;


    [Header("Damage")]
    [Range(0, 1000)][SerializeField] public float damage;
    [Range(0, 1000)][SerializeField] public float modDamage;
    public float currentDamage;

    [Header("Defense")]
    [Range(0, 100)][SerializeField] public float defense;
    [Range(0, 100)][SerializeField] public float currentDefense;
    [Range(0, 100)][SerializeField] public float modDefense;

    [Header("Movement Mods")]
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
        
        HandleStamina();

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
        dashCost = gameManager.instance.dashCost;
        GE_OnPlayerStaminaChanged.Raise(this, gameManager.instance.playerStatHandler);
    }
    
    public void HandleStamina()
    {
        if (gameManager.instance.dashTriggered
            && !gameManager.instance.isDashing
            && gameManager.instance.characterController.isGrounded
            && gameManager.instance.playerInputHandler.currentSpeed != 0)
        {
            currentStamina -= dashCost;
        }

        if (gameManager.instance.isDashing)
        {
            currentStamina += -dashLoss;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            GE_OnPlayerStaminaChanged.Raise(this, gameManager.instance.playerStatHandler);

            if (currentStamina <= 0)
            {
                gameManager.instance.canDash = false;
            }
        }
        else if (gameManager.instance.isMeleeing)
        {
            currentStamina -= meleeLoss;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            GE_OnPlayerStaminaChanged.Raise(this, gameManager.instance.playerStatHandler);

            if (currentStamina <= 0)
            {
                gameManager.instance.canMelee = false;
            }
        }
        else if (!gameManager.instance.isDashing && !gameManager.instance.isMeleeing)
        {
            currentStamina += staminaRegen;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            GE_OnPlayerStaminaChanged.Raise(this, gameManager.instance.playerStatHandler);

            if (currentStamina >= maxStamina)
            {
                gameManager.instance.canDash = true;
                gameManager.instance.canMelee = true;
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
        if (gameManager.instance.isDashing)
        {
            return;
        }

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

