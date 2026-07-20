using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToxicBossAI : MonoBehaviour, IDamage, IBossTrigger
{
    public enum BossState
    {
        Idle,
        Chasing,
        Attacking,
        Dead
    }

    [Header("Health")]
    [SerializeField] private float maxHealth = 1000f;

    [Tooltip("Default damage used by attacks unless an attack has its own value.")]
    [SerializeField] private int attackDamage = 10;

    [Tooltip("XP given when the boss dies.")]
    [SerializeField] private int xpGive = 100;

    [Tooltip("The boss model. This can be disabled when the boss dies.")]
    [SerializeField] private GameObject model;

    [Header("Health UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText;

    [Tooltip("Parent object containing the floating damage text.")]
    [SerializeField] private GameObject onScreenDamage;

    [SerializeField] private TMP_Text damageText;

    [Tooltip("How long floating damage text remains visible.")]
    [SerializeField] private float damageTextDuration = 0.5f;

    [Header("Target")]
    [SerializeField] private Transform player;

    [Tooltip("The tag used to find the player.")]
    [SerializeField] private string playerTag = "Player";

    [Header("Movement")]
    [SerializeField] private NavMeshAgent agent;

    [Tooltip("Maximum range at which the boss notices the player.")]
    [SerializeField] private float sightRange = 30f;

    [Tooltip("How close the boss normally tries to get.")]
    [SerializeField] private float stoppingDistance = 6f;

    [Tooltip("How quickly the boss manually faces the player.")]
    [SerializeField] private float rotateSpeed = 8f;

    [Header("Toxic Slam")]
    [Tooltip("Point used as the center of the slam damage area.")]
    [SerializeField] private Transform meleePoint;

    [Tooltip("Radius of the Toxic Slam damage area.")]
    [Range(1f, 10f)]
    [SerializeField] private float meleeRadius = 3f;

    [Tooltip("Damage dealt by Toxic Slam.")]
    [Range(1, 100)]
    [SerializeField] private int meleeDamage = 25;

    [Tooltip("Layers that can be damaged by Toxic Slam.")]
    [SerializeField] private LayerMask meleeTargetLayers;

    [Tooltip("How long the boss waits before using Toxic Slam again.")]
    [Range(0.5f, 10f)]
    [SerializeField] private float meleeCooldown = 3f;

    [Header("Toxic Projectile")]
    [Tooltip("How long the boss waits before firing another projectile.")]
    [SerializeField] private float projectileCooldown = 4f;

    [SerializeField] private GameObject toxicProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Toxic Gas")]
    [SerializeField] private GameObject toxicGasCloudPrefab;
    [SerializeField] private Transform gasSpawnPoint;

    [Tooltip("How long the boss waits before using another gas attack.")]
    [SerializeField] private float gasCooldown = 10f;

    [Range(0f, 1f)]
    [Tooltip("Chance to use moving gas instead of a slam while the player is close.")]
    [SerializeField] private float closeGasChance = 0.35f;

    [Range(0f, 1f)]
    [Tooltip("Chance to spawn gas at the player instead of firing a projectile.")]
    [SerializeField] private float gasOnPlayerChance = 0.20f;

    [Tooltip("How fast gas spawned at the boss moves toward the player's previous position.")]
    [SerializeField] private float gasMoveSpeed = 2f;

    [Header("Attack Timing")]
    [Tooltip("Delay before the selected attack happens.")]
    [SerializeField] private float attackWindup = 0.75f;

    [Tooltip("Delay after an attack before movement resumes.")]
    [SerializeField] private float attackRecovery = 1f;

    [Header("Currency")]
    [SerializeField] private int minCurrencyDrop = 10;
    [SerializeField] private int maxCurrencyDrop = 25;

    [Header("Death")]
    [Tooltip("How long the boss remains before being destroyed.")]
    [SerializeField] private float destroyDelay = 2f;

    [Tooltip("Optional portal or exit that appears when this boss dies.")]
    [SerializeField] private GameObject exitPortal;

    [Header("Events")]
    [Tooltip("Called whenever the boss takes damage.")]
    [SerializeField] private UnityEvent onHit;

    [Tooltip("Called once when the boss dies.")]
    [SerializeField] private UnityEvent onDeath;

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int BasicAttackHash = Animator.StringToHash("BasicAttack");
    private static readonly int ProjectileThrowHash = Animator.StringToHash("ProjectileThrow");
    private static readonly int GasAttackHash = Animator.StringToHash("GasAttack");
    private static readonly int IsDeadHash =  Animator.StringToHash("IsDead");

    private BossState currentState = BossState.Idle;

    private float currentHealth;

    private float nextMeleeTime;
    private float nextProjectileTime;
    private float nextGasTime;

    private bool isAttacking;
    private bool isDead;

    private Coroutine damageTextCoroutine;

    public BossState CurrentState => currentState;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public int AttackDamage => attackDamage;
    public int XPGive => xpGive;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (agent == null)
        {
            Debug.LogError(
                "ToxicBossAI requires a NavMeshAgent component.",
                gameObject
            );

            enabled = false;
            return;
        }

        agent.stoppingDistance = stoppingDistance;
    }

    private void Start()
    {
        currentHealth = maxHealth;

        FindPlayer();
        InitializeUI();

        nextMeleeTime = Time.time;
        nextProjectileTime = Time.time;

        // Prevent the boss from starting the fight with gas.
        nextGasTime = Time.time + gasCooldown;

        if (exitPortal != null)
        {
            exitPortal.SetActive(false);
        }
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        if (player == null)
        {
            FindPlayer();
            StopMoving();
            return;
        }

        float distanceToPlayer = Vector3.Distance(
            transform.position,
            player.position
        );

        if (isAttacking)
        {
            FacePlayer();
            return;
        }

        if (distanceToPlayer > sightRange)
        {
            EnterIdleState();
            return;
        }

        HandleCombat(distanceToPlayer);
        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation()
    {
        if (animator == null)
        {
            return;
        }
        float movementSpeed = 0f;

        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh &&
            !agent.isStopped)
        {
            movementSpeed = agent.velocity.magnitude;
        }

        animator.SetFloat(
            SpeedHash,
            movementSpeed,
            0.1f,
            Time.deltaTime
        );
    }

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject == null)
        {
            if (showDebugMessages)
            {
                Debug.LogWarning(
                    $"Toxic Boss could not find an object tagged {playerTag}.",
                    gameObject
                );
            }

            return;
        }

        player = playerObject.transform;

        if (showDebugMessages)
        {
            Debug.Log(
                "Toxic Boss found the player.",
                gameObject
            );
        }
    }

    private void HandleCombat(float distanceToPlayer)
    {

        if (distanceToPlayer <= meleeRadius)
        {
            HandleCloseRangeCombat();
            return;
        }

        HandleRangedCombat();
    }

    private void HandleCloseRangeCombat()
    {
        bool gasReady = Time.time >= nextGasTime;
        bool meleeReady = Time.time >= nextMeleeTime;

        if (gasReady && Random.value <= closeGasChance)
        {
            StartCoroutine(PerformGasAttack(false));
            return;
        }

        if (meleeReady)
        {
            StartCoroutine(PerformMeleeAttack());
            return;
        }

        if (gasReady)
        {
            StartCoroutine(PerformGasAttack(false));
            return;
        }

        ChasePlayer();
    }

    private void HandleRangedCombat()
    {
        if (Time.time < nextProjectileTime)
        {
            ChasePlayer();
            return;
        }

        bool gasReady = Time.time >= nextGasTime;

        if (gasReady && Random.value <= gasOnPlayerChance)
        {
            StartCoroutine(PerformGasAttack(true));
            return;
        }

        StartCoroutine(PerformProjectileAttack());
    }

    private void ChasePlayer()
    {
        if (agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh)
        {
            return;
        }

        currentState = BossState.Chasing;

        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void EnterIdleState()
    {
        currentState = BossState.Idle;
        StopMoving();
    }

    private void StopMoving()
    {
        if (agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = true;

        if (agent.hasPath)
        {
            agent.ResetPath();
        }
    }

    private void FacePlayer()
    {
        if (player == null)
        {
            return;
        }

        Vector3 directionToPlayer =
            player.position - transform.position;

        // Keep the boss upright.
        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude <= 0.01f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(directionToPlayer.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }

    private IEnumerator PerformMeleeAttack()
    {
        BeginAttack();
        animator?.SetTrigger(BasicAttackHash);

        nextMeleeTime = Time.time + meleeCooldown;

        DebugAttack("preparing TOXIC SLAM");

        yield return new WaitForSeconds(attackWindup);

        if (meleePoint == null)
        {
            Debug.LogWarning(
                "Toxic Slam cannot run because Melee Point is not assigned.",
                gameObject
            );
        }
        else
        {
            Collider[] hits = Physics.OverlapSphere(
                meleePoint.position,
                meleeRadius,
                meleeTargetLayers,
                QueryTriggerInteraction.Collide
            );

            foreach (Collider hit in hits)
            {
                IDamage damageable =
                    hit.transform.root.GetComponentInChildren<IDamage>();

                if (damageable == null)
                {
                    continue;
                }

                damageable.takeDamage(meleeDamage);

                if (showDebugMessages)
                {
                    Debug.Log(
                        $"Toxic Slam dealt {meleeDamage} damage to {hit.name}.",
                        hit.gameObject
                    );
                }

                // Prevent multiple player colliders from causing repeat damage.
                break;
            }
        }

        DebugAttack("used TOXIC SLAM");

        yield return new WaitForSeconds(attackRecovery);

        EndAttack();
    }

    private IEnumerator PerformProjectileAttack()
    {
        BeginAttack();
        animator?.SetTrigger(ProjectileThrowHash);

        nextProjectileTime =
            Time.time + projectileCooldown;

        DebugAttack("preparing TOXIC PROJECTILE");

        yield return new WaitForSeconds(attackWindup);

        if (toxicProjectilePrefab == null)
        {
            Debug.LogWarning(
                "Toxic projectile prefab has not been assigned.",
                gameObject
            );
        }
        else if (projectileSpawnPoint == null)
        {
            Debug.LogWarning(
                "Projectile Spawn Point has not been assigned.",
                gameObject
            );
        }
        else if (player != null)
        {
            GameObject projectileObject = Instantiate(
                toxicProjectilePrefab,
                projectileSpawnPoint.position,
                projectileSpawnPoint.rotation
            );

            ToxicProjectile projectile =
                projectileObject.GetComponentInChildren<ToxicProjectile>();

            if (projectile != null)
            {
                /*
                 * Add height so the projectile aims toward the player's
                 * body rather than directly at the ground.
                 */
                Vector3 targetPosition =
                    player.position + Vector3.up;

                projectile.Launch(
                    targetPosition,
                    transform
                );
            }
            else
            {
                Debug.LogError(
                    "The toxic projectile prefab does not have a ToxicProjectile component.",
                    projectileObject
                );

                Destroy(projectileObject);
            }
        }

        DebugAttack("used TOXIC PROJECTILE");

        yield return new WaitForSeconds(attackRecovery);

        EndAttack();
    }

    private IEnumerator PerformGasAttack(bool spawnOnPlayer)
    {
        BeginAttack();
        animator?.SetTrigger(GasAttackHash);

        nextGasTime = Time.time + gasCooldown;

        if (spawnOnPlayer)
        {
            DebugAttack(
                "preparing TOXIC GAS at the player's position"
            );
        }
        else
        {
            DebugAttack(
                "preparing MOVING TOXIC GAS from the boss"
            );
        }

        yield return new WaitForSeconds(attackWindup);

        if (toxicGasCloudPrefab == null)
        {
            Debug.LogWarning(
                "Toxic Gas Cloud prefab has not been assigned.",
                gameObject
            );
        }
        else if (player == null)
        {
            Debug.LogWarning(
                "Toxic Gas Cloud cannot spawn because the player is missing.",
                gameObject
            );
        }
        else if (!spawnOnPlayer && gasSpawnPoint == null)
        {
            Debug.LogWarning(
                "Gas Spawn Point has not been assigned.",
                gameObject
            );
        }
        else
        {
            Vector3 spawnPosition;

            if (spawnOnPlayer)
            {
                spawnPosition = player.position;
            }
            else
            {
                spawnPosition = gasSpawnPoint.position;
            }

            /*
             * Save where the player was standing at the moment the
             * gas was created. The moving cloud will travel toward
             * this position but will not constantly follow the player.
             */
            Vector3 playerTargetPosition = player.position;

            GameObject gasObject = Instantiate(
                toxicGasCloudPrefab,
                spawnPosition,
                Quaternion.identity
            );

            ToxicGasCloud gasCloud =
                gasObject.GetComponent<ToxicGasCloud>();

            if (gasCloud == null)
            {
                Debug.LogWarning(
                    "The gas prefab does not contain a ToxicGasCloud component.",
                    gasObject
                );
            }
            else if (spawnOnPlayer)
            {
                gasCloud.InitializeStationary();
            }
            else
            {
                gasCloud.InitializeMoving(
                    playerTargetPosition,
                    gasMoveSpeed
                );
            }
        }

        DebugAttack("used TOXIC GAS CLOUD");

        yield return new WaitForSeconds(attackRecovery);

        EndAttack();
    }

    private void BeginAttack()
    {
        currentState = BossState.Attacking;
        isAttacking = true;

        StopMoving();
        FacePlayer();
    }

    private void EndAttack()
    {
        if (isDead)
        {
            return;
        }

        isAttacking = false;
        currentState = BossState.Chasing;

        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }

    private void DebugAttack(string message)
    {
        if (!showDebugMessages)
        {
            return;
        }

        Debug.Log(
            $"Toxic Boss {message}.",
            gameObject
        );
    }

    public void takeDamage(int amount)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        currentHealth -= amount;

        currentHealth = Mathf.Clamp(
            currentHealth,
            0f,
            maxHealth
        );

        UpdateHealthUI();
        ShowDamageText(amount);

        onHit?.Invoke();

        if (showDebugMessages)
        {
            Debug.Log(
                $"Toxic Boss took {amount} damage. " +
                $"Health: {currentHealth}/{maxHealth}",
                gameObject
            );
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void InitializeUI()
    {
        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (onScreenDamage != null)
        {
            onScreenDamage.SetActive(false);
        }

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text =
                $"{Mathf.CeilToInt(currentHealth)} / " +
                $"{Mathf.CeilToInt(maxHealth)}";
        }
    }

    private void ShowDamageText(float damageAmount)
    {
        if (damageText == null ||
            onScreenDamage == null)
        {
            return;
        }

        damageText.text =
            Mathf.CeilToInt(damageAmount).ToString();

        if (damageTextCoroutine != null)
        {
            StopCoroutine(damageTextCoroutine);
        }

        damageTextCoroutine =
            StartCoroutine(DamageTextRoutine());
    }

    private IEnumerator DamageTextRoutine()
    {
        onScreenDamage.SetActive(true);

        yield return new WaitForSeconds(
            damageTextDuration
        );

        onScreenDamage.SetActive(false);
        damageTextCoroutine = null;
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        gameManager.instance.BossDie();
        animator?.SetBool(IsDeadHash, true);
        isAttacking = false;
        currentState = BossState.Dead;

        StopAllCoroutines();
        StopMoving();

        if (agent != null && agent.enabled)
        {
            agent.enabled = false;
        }

        if (healthBar != null)
        {
            healthBar.value = 0f;
        }

        int currencyReward = Random.Range(
            minCurrencyDrop,
            maxCurrencyDrop + 1
        );

        if (showDebugMessages)
        {
            Debug.Log(
                $"Toxic Boss died. Rewards: " +
                $"{xpGive} XP and {currencyReward} currency.",
                gameObject
            );
        }

        /*
         * Connect XP and currency rewards to your existing
         * gameManager methods when their method names are confirmed.
         */

        if (exitPortal != null)
        {
            exitPortal.SetActive(true);
        }

        onDeath?.Invoke();
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Sight range.
        Gizmos.DrawWireSphere(
            transform.position,
            sightRange
        );

        // Toxic Slam range.
        if (meleePoint != null)
        {
            Gizmos.DrawWireSphere(
                meleePoint.position,
                meleeRadius
            );
        }
    }

    public void TriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (showDebugMessages)
        {
            Debug.Log(
                "Player entered Toxic Boss arena.",
                gameObject
            );
        }
    }
}