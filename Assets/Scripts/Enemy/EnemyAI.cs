using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class enemyAI : MonoBehaviour, IDamage, IInteract
{
    
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] int xpGive = 100;
    [SerializeField] Renderer model;
    public UnityEngine.UI.Slider healthbar;
    public TMP_Text healthText;

    public GameObject onScreenDMG;
    public TMP_Text damageText;
    [SerializeField] private int currentHealth;

    [SerializeField] private float sightRange = 15f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float hearingRange = 20f;


    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderTimer = 5f;

    [Header("Audio")]
    [SerializeField] BaseSoundSO _hit;
    [SerializeField] BaseSoundSO _dead;
   


    [Header("Don't touch unles debugging")]
    [SerializeField] List<int> Modifiers;

    private Vector3 lastHeardPosition;
    private bool heardNoise;
    private Transform player;
    private NavMeshAgent agent;

    private float nextAttackTime;
    private float attackCooldown = 1.5f;
    private float wanderTime;

    private enum ZombieState
    {
        Wander,
        Chase,
        Attack,
        Dead
    }

    private ZombieState currentState;

    Color originalColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        currentHealth = maxHealth;
        
        originalColor = model.material.color;

        player = GameObject.FindGameObjectWithTag("Player").transform;

        agent = GetComponent<NavMeshAgent>();

        currentState = ZombieState.Wander;
        wanderTime = wanderTimer;
        gameManager.instance.updateGameGoal(1);


    }

    private void Update()
    {
        updateHealthBar();
        if (currentState == ZombieState.Dead)
            return;
        if (player == null)
        {
            currentState = ZombieState.Wander;
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        switch(currentState)
        {
            case ZombieState.Wander:
                Wander();

                if (distance <= sightRange)
                {
                    currentState = ZombieState.Chase;
                }
                break;

            case ZombieState.Chase:
                agent.SetDestination(player.position);

                if(distance <= attackRange)
                {
                    currentState = ZombieState.Attack;
                }
                break;

            case ZombieState.Attack:
                agent.SetDestination(transform.position);

                AttackPlayer();

                if (distance > attackRange)
                {
                    currentState = ZombieState.Chase;
                }
                break;

        }
    }

    private void Wander()
    {
        wanderTime += Time.deltaTime;

        if(wanderTime >= wanderTimer)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * wanderRadius;

            randomDirection += transform.position;

            NavMeshHit hit;
            if(NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            wanderTime = 0;
        }
    }

    public void HearNoise(Vector3 noisePosition)
    {
        float distance = Vector3.Distance(transform.position, noisePosition);

        if (distance <= hearingRange)
        {
            lastHeardPosition = noisePosition;
            heardNoise = true;

            Debug.Log("Zombie heard noise");
        }
    }

    private void AttackPlayer()
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            Debug.Log("Zombie Attack");

            IDamage damageable = player.GetComponentInChildren<IDamage>();

            if (damageable != null)
            {
                damageable.takeDamage(attackDamage);
                Debug.Log("Damage Applied");
            }
            else
            {
                Debug.Log("No IDamage Found");
            }
        }
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
            currentState = ZombieState.Dead;
            if (agent != null)
                agent.isStopped = true;

            AudioManager.instance.PlaySoundAtPosition(_dead, gameObject);

            gameManager.instance.updateGameGoal(-1);
            gameManager.instance.addXp(xpGive);
            TempUI.OffHover();
            Destroy(gameObject);
        }
        else
        {
            currentState = ZombieState.Chase;
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
        TempUI.OnHover(1);
    }
    public void OnHoverExit()
    {
        TempUI.OffHover();
    }
}
