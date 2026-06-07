using System.Xml.XPath;
using UnityEngine;

public class StatHandler : MonoBehaviour
{
    [Range(10f, 500f)][SerializeField] public float health;
    public float currentHealth;
    public float maxHealth;
    public float modHealth;


    [Range(10f, 100f)][SerializeField] public float stamina;
    public float currentStamina;
    public float MaxStamina;
    public float modStamina;

    [Range(10f, 100f)][SerializeField] public float level;
    [Range(10f, 100f)][SerializeField] public float currentLevel;
    [Range(10f, 100f)][SerializeField] public float maxLevel;
    [Range(10f, 100f)][SerializeField] public float xpToNextLevel;

    [Range(10f, 100f)][SerializeField] public float damage;
    [Range(10f, 100f)][SerializeField] public float currentDamage;
    [Range(10f, 100f)][SerializeField] public float modDamage;

    [Range(10f, 100f)][SerializeField] public float defense;
    [Range(10f, 100f)][SerializeField] public float currentDefense;
    [Range(10f, 100f)][SerializeField] public float maxDefense;

    [Range(10f, 100f)][SerializeField] public float speed;

    [Range(10f, 100f)][SerializeField] public float jumps;











    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }

   
}

