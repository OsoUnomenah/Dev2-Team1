using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage, IInteract, IFreeze, IShatterable
{

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] int xpGive = 100;
    [SerializeField] Renderer model;
    public UnityEngine.UI.Slider healthbar;
    public TMP_Text healthText;
    public bool isDead = false;
    bool isFroze = false;

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
    [SerializeField] BaseSoundSO _grunts;
    [SerializeField] BaseSoundSO _attack;
    [SerializeField] BaseSoundSO _footsteps;
    [Range(5f, 25f)][SerializeField] float gruntRateMax;
    [Range(1f, 20f)][SerializeField] float gruntRateMin;
    [SerializeField] private AudioSource audioSource;
    public GameObject explosion;

    private float footstepTimer;
    private float gruntRate;
    private float gruntTimer;

    [Header("Currency")]
    [SerializeField] private int minCurrencyDrop = 1;
    [SerializeField] private int maxCurrencyDrop = 3;

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

    public bool IsFrozen
    {
        get { return isFroze; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        currentHealth = maxHealth;
        updateHealthBar();


        originalColor = model.material.color;

        player = GameObject.FindGameObjectWithTag("Player").transform;

        agent = GetComponent<NavMeshAgent>();

        currentState = ZombieState.Wander;
        wanderTime = wanderTimer;
        // gameManager.instance.updateGameGoal(1);

        gruntRate = Random.Range(gruntRateMin, gruntRateMax);
    }

    private void Update()
    {

        if (currentState == ZombieState.Dead)
            return;
        if (!isFroze)
        {

            if (player == null)
            {
                currentState = ZombieState.Wander;
                return;
            }

            float distance = Vector3.Distance(transform.position, player.position);

            switch (currentState)
            {
                case ZombieState.Wander:
                    Wander();

                    if (distance <= sightRange)
                    {
                        currentState = ZombieState.Chase;
                    }
                    SpinWheels();
                    break;

                case ZombieState.Chase:
                    agent.SetDestination(player.position);

                    if (distance <= attackRange)
                    {
                        currentState = ZombieState.Attack;
                    }
                    SpinWheels();
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
            PlayGrunt();
            HandleFootsteps();
        }
        else
        {
            model.material.color = Color.blue;
        }
    }
    public int spinSpeed;
    public GameObject wheel1;
    public GameObject wheel2;
    private void SpinWheels()
    {
        if (agent.velocity.magnitude > 0.1f)
        {
            wheel1.transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime);
            wheel2.transform.Rotate(Vector3.down * spinSpeed * Time.deltaTime);
        }
    }

    private void Wander()
    {
        wanderTime += Time.deltaTime;

        if (wanderTime >= wanderTimer)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * wanderRadius;

            randomDirection += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
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

            AudioManager.instance.PlaySoundFollowPosition(_grunts, gameObject);

            Debug.Log("Zombie heard noise");
        }
    }

    private void AttackPlayer()
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            Debug.Log("Zombie Attack");

            IDamage damageable = gameManager.instance.playerStatHandler.GetComponentInChildren<IDamage>();

            AudioManager.instance.PlaySoundAtPosition(_attack, gameObject);

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
        if (isDead)
        {
            return;
        }
        gameManager.instance.playerDamageOut = amount;
        StartCoroutine(updateDamageText());

        currentHealth -= amount;
        updateHealthBar();

        if (currentHealth <= 0)
        {

            Die();
            StartCoroutine(death());
        }
        else
        {
            currentState = ZombieState.Chase;
            AudioManager.instance.PlaySoundAtPosition(_hit, gameObject);
            StartCoroutine(flashRed());
        }
    }
    IEnumerator death()
    {
        Instantiate(explosion, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(.5f);
        Destroy(gameObject);
    }
    private void Die()
    {
        if (isDead)
        {
            return;
        }

        currentState = ZombieState.Dead;
        isDead = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (AudioManager.instance != null && _dead != null)
        {
            AudioManager.instance.PlaySoundAtPosition(_dead, gameObject);
        }

        if (gameManager.instance != null)
        {
            gameManager.instance.addXp(xpGive);

            int currencyDrop = GetCurrencyDrop();
            gameManager.instance.addCurrency(currencyDrop);
        }

        if (WaveManager.instance != null)
        {
            WaveManager.instance.Drop(transform);
            WaveManager.instance.OnEnemyKilled();
        }

        RecticleBehaviour.OffHover();

        Destroy(gameObject);
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
        isFroze = true;
        StartCoroutine(freezeHandler(duration));
    }
    IEnumerator freezeHandler(float duration)
    {
        model.material.color = Color.blue;
        agent.isStopped = true;

        yield return new WaitForSeconds(duration);

        model.material.color = originalColor;
        isFroze = false;
        agent.isStopped = false;
    }

    private int GetCurrencyDrop()
    {
        return UnityEngine.Random.Range(minCurrencyDrop, maxCurrencyDrop + 1);
    }

    public void Shatter()
    {
        if (isDead)
        {
            return;
        }

        Debug.Log("SHATTER CALLED ON: " + gameObject.name);

        Die();
    }

    private void PlayGrunt()
    {
        gruntTimer += Time.deltaTime;

        if (gruntTimer > gruntRate)
        {
            gruntTimer = 0;
            AudioManager.instance.PlaySoundFollowPosition(_grunts, gameObject);
        }
    }

    private void HandleFootsteps()
    {
        if (agent.velocity.magnitude > 0 && !audioSource.isPlaying)
        {
            AudioManager.instance.PlaySoundFromSource(_footsteps, gameObject);
        }


    }
}
