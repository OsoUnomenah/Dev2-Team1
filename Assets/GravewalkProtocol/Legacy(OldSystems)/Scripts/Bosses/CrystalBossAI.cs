using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System.Threading;
using NUnit.Framework.Internal;

public class CrystalBossAI : MonoBehaviour, IDamage, IInteract, IFreeze
{
    
    
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] int xpGive = 100;
    [SerializeField] Renderer model;
    private NavMeshAgent agent0;
    public UnityEngine.UI.Slider healthbar;
    public TMP_Text healthText;

    public GameObject onScreenDMG;
    public TMP_Text damageText;
    [SerializeField] private int currentHealth;

    [SerializeField] private float sightRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float hearingRange;

    [SerializeField] public GameObject exitPortal;


    [SerializeField] private float wanderRadius;
    [SerializeField] private float wanderTimer;

    [Header("Audio")]
    [SerializeField] BaseSoundSO _hit;
    [SerializeField] BaseSoundSO _dead;

    [Header("Weapon")]
    // may need multiple types of bullets so make another one if need be
    [SerializeField] GameObject bullet;

    //only needed if you are using the same type of gun pivot as the enemies
    [SerializeField] GameObject armor;

    [SerializeField] GameObject weakpoint1;
    [SerializeField] GameObject weakpoint2;
    [SerializeField] GameObject weakpoint3;
    [SerializeField] GameObject weakpoint4;
    

    //might need multiple different shoot rates for a boss
    [Range(0.1f, 2f)][SerializeField] float shootRate;
    

    [Header("Don't touch unles debugging")]
    [SerializeField] List<int> Modifiers;

    private Vector3 lastHeardPosition;
    private bool heardNoise;

    //makes a boss freeze, some bosses may not make sense to freeze
    private bool isFroze;
    
    private Transform player;
    

    private float nextAttackTime;
    private float attackCooldown = 1.5f;
    private float RestTime;
    private bool PlayerInTrigger;
    private float timer;
    private enum BossState
    {
        Rest,
        Decide,
        Attack1,
        Attack2,
        Attack3,
        Dead
    }

    private BossState currentState;
    int phasePicker;

    Vector3 playerDir;

    Color originalColor;
    [SerializeField] GameObject gun;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    
    private void Start()
    {
        exitPortal = GameObject.FindGameObjectWithTag("Portal");
        exitPortal.SetActive(false);
        
        armor.SetActive(false);

        currentHealth = maxHealth;
        
        originalColor = model.material.color;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        currentState = BossState.Rest;
        
        gameManager.instance.updateGameGoal(1);

        timer = -100;
        phasePicker = 0;
        
    }

    private void Update()
    {
        playerDir = gameManager.instance.player.transform.position - transform.position;
        updateHealthBar();
        if (currentState == BossState.Dead)
            return;
        if (player == null || isFroze)
        {
            currentState = BossState.Rest;
           
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        ArmorCheck();

        switch(currentState)
        {
            case BossState.Rest:                
                Rest();
                
                break;

            case BossState.Attack1:
                Spin();

                break;

            case BossState.Attack2:
                ArmoredUp();

                break;
            case BossState.Attack3:
                Lava();
                break;

            case BossState.Decide:
                Decide();

                break;
        }
    }

    private void Rest()
    {
        if (PlayerInTrigger)
        {
            currentState = BossState.Decide;
        }
    }
    bool isArmored = false;

    private void ArmoredUp()
    {
        Debug.Log("Attack 2");
        if(isArmored)
        {
            currentState = BossState.Decide;
            return;
        }
        if (!armor.activeSelf)
        {
            armor.SetActive(true);
            isArmored = true;
            timer = Random.Range(1200, 2000);

        }
        if (!PlayerInTrigger)
        {            
            armor.SetActive(false);
            currentState = BossState.Rest;
            timer = -100;
            
        }
         
        
        timer--;
    }
    private void ArmorCheck()
    {
        if(!weakpoint1.activeSelf && !weakpoint2.activeSelf && !weakpoint3.activeSelf && !weakpoint4.activeSelf)
        {
            armor.SetActive(false);
            weakpoint1.SetActive(true);
            weakpoint1.GetComponent<Weakpoints>().activateWeakpoint();

            weakpoint2.SetActive(true);
            weakpoint2.GetComponent<Weakpoints>().activateWeakpoint();

            weakpoint3.SetActive(true);
            weakpoint3.GetComponent<Weakpoints>().activateWeakpoint();

            weakpoint4.SetActive(true);
            weakpoint4.GetComponent<Weakpoints>().activateWeakpoint();

            StartCoroutine(armorCooldown());
        }
    }
    IEnumerator armorCooldown()
    {
        yield return new WaitForSeconds(15f);
        isArmored = false;
    }
    private void Lava()
    {
        Debug.Log("Attack 3");
        currentState = BossState.Decide;
        //timer = Random.Range(1200, 2000);

        //timer--;
    }
    private void Spin()
    {
        Debug.Log("Attack 1");
        currentState = BossState.Decide;
        //if (!PlayerInTrigger)
        //{
        //    currentState = BossState.Rest;
        //}
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInTrigger = true;
            model.material.color = Color.orange;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            PlayerInTrigger = false;
        model.material.color = originalColor;
    }

    private void Decide()
    {
        if (!PlayerInTrigger)
        {
            Rest();
        }
        else if (timer == -100)
        {
            timer = Random.Range(1, 1200); //1200
        }

        timer -= 1;

        if (timer < 0)
        {
            phasePicker = Random.Range(1, 4);// picks from a range of 1 2 or 3
        }

        
        switch (phasePicker)
        {
            case 0:
                break;
            case 1:
                currentState = BossState.Attack1;
                break;
            case 2:
                currentState = BossState.Attack2;
                break;
            case 3:
                currentState = BossState.Attack3;
                break;
        }
        //if (Time.time >= nextAttackTime)
        //{
        //    nextAttackTime = Time.time + attackCooldown;

        //    Debug.Log("Zombie Attack");

        //    IDamage damageable = player.GetComponentInChildren<IDamage>();

        //    if (damageable != null)
        //    {
        //        damageable.takeDamage(attackDamage);
        //        Debug.Log("Damage Applied");
        //    }
        //    else
        //    {
        //        Debug.Log("No IDamage Found");
        //    }
        //}
    }

    public void updateHealthBar()
    {
        healthText.text = currentHealth + " / " + maxHealth;
        healthbar.value = (float)currentHealth / (float)maxHealth;
    }
    
    IEnumerator updateDamageText()
    {
        damageText.text = ("DMG: " + gameManager.instance.playerDamageOut.ToString());

        onScreenDMG.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        onScreenDMG.SetActive(false);
    }

    public void takeDamage(int amount)
    {
        //Set the damage to display on the damage text
        gameManager.instance.playerDamageOut = amount;

        //Show the damage text
        StartCoroutine(updateDamageText());

        currentHealth -= amount;        


        if (currentHealth <= 0)
        {
            currentState = BossState.Dead;
            if (agent0 != null)
                agent0.isStopped = true;

            AudioManager.instance.PlaySoundAtPosition(_dead, gameObject);

            gameManager.instance.updateGameGoal(-1);
            gameManager.instance.addXp(xpGive);
            RecticleBehaviour.OffHover();

            if (exitPortal != null)
            {
                exitPortal.SetActive(true);
            }

            Destroy(gameObject);
        }
        else
        {
            AudioManager.instance.PlaySoundAtPosition(_hit, gameObject);
            StartCoroutine(flashRed());
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = originalColor;
    }

    IEnumerator flashGreen()
    {
        model.material.color = Color.green;
        yield return new WaitForSeconds(0.3f);
        model.material.color = originalColor;
    }


    public void Interact()
    {
        // StartCoroutine(flashGreen()); //this was for testing interaction, can be removed or changed to something else
    }
    public void OnHoverEnter()
    {
        RecticleBehaviour.OnHover(1);
    }
    public void OnHoverExit()
    {
        RecticleBehaviour.OffHover();
    }

    public void freeze(float duration)
    {
        //not using the durtion for him cause he is only gonna turn blue for a second and shake it off
        isFroze = true;
        StartCoroutine(freezeHandler(20f));
    }
    IEnumerator freezeHandler(float duration)
    {
        model.material.color = Color.blue;


        yield return new WaitForSeconds(duration);

        model.material.color = originalColor;
        isFroze = false;
    }
}
