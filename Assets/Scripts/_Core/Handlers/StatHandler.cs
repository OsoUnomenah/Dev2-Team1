using System.Xml.XPath;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using JetBrains.Annotations;

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
    [Range(0f, 100f)][SerializeField] public float modJumps;

    public Slider healthBar;
    public TMP_Text healthText;

    public Slider staminaBar;
    public TMP_Text staminaText;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxHealth = health + modHealth;
        currentHealth = maxHealth;

        maxStamina = stamina + modStamina;
        currentStamina = maxStamina;
        sprintCost = gameManager.instance.sprintCost;

        currentDamage = damage + modDamage;
    }

    // Update is called once per frame
    void Update()
    {
        healthText.text = " HP: "+ currentHealth + " / " + maxHealth;
        healthBar.value = (float)currentHealth / (float)maxHealth;

        staminaText.text = " STM: " + Mathf.CeilToInt(currentStamina) + " / " + Mathf.CeilToInt(maxStamina);
        staminaBar.value = (float)currentStamina / (float)maxStamina;
        
        currentDamage = damage + modDamage;
        
        HandleSprint();

    }

    public void HandleSprint()
    {


        if (gameManager.instance.SprintTriggered && !gameManager.instance.isSprinting)
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
    }


            public int EnemyAttack()
    {
        gameManager.instance.enemyDamageOut = (int)currentDamage;
        return (int)currentDamage;
    }

    public void takeDamage(int amount)
    {
        currentHealth += amount;
    }

}

