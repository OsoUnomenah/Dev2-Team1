using System.Xml.XPath;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    [Range(1, 1000)][SerializeField] public float damage;
    [Range(1, 1000)][SerializeField] public float currentDamage;
    [Range(1, 1000)][SerializeField] public float modDamage;

    [Range(1, 100)][SerializeField] public float defense;
    [Range(1, 100)][SerializeField] public float currentDefense;
    [Range(1, 100)][SerializeField] public float modDefense;


    [Range(10f, 100f)][SerializeField] public float modSpeed;
    [Range(10f, 100f)][SerializeField] public float modJumps;

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
    }

    // Update is called once per frame
    void Update()
    {
        healthText.text = " HP: "+ currentHealth + " / " + maxHealth;
        healthBar.value = (float)currentHealth / (float)maxHealth;

        staminaText.text = " STM: " + currentStamina + " / " + maxStamina;
        staminaBar.value = (float)currentStamina / (float)maxStamina;

    }

    public void takeDamage(int amount)
    {
        currentHealth += amount;
    }

}

