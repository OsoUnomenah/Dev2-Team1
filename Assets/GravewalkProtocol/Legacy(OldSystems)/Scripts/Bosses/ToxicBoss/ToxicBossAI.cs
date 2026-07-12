using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ToxicBossAI : MonoBehaviour
{
    public enum BossState
    {
        Idle,
        Chasing,
        Attacking,
        Dead
    }

    [Header("References")]

    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent agent;

    [Header("Detection")]

    [Tooltip("The maximum distance at which the boss can detect the player.")]
    [Range(5f, 100f)]
    [SerializeField] private float detectionRange = 30f;

    [Header("Movement")]

    [Tooltip("How close the boss tries to get to the player.")]
    [Range(1f, 15f)]
    [SerializeField] private float stoppingDistance = 6f;

    [Tooltip("How quickly the boss rotates toward the player while attacking.")]
    [Range(1f, 20f)]
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Melee Attack")]

    [Tooltip("The maximum distance for the Toxic Slam attack.")]
    [Range(1f, 10f)]
    [SerializeField] private float meleeRange = 3f;

    [Tooltip("How long the boss must wait before using Toxic Slam again.")]
    [Range(0.5f, 10f)]
    [SerializeField] private float meleeCooldown = 3f;

    [Header("Projectile Attack")]

    [Tooltip("The maximum distance from which the boss can fire a toxic projectile.")]
    [Range(5f, 50f)]
    [SerializeField] private float projectileRange = 20f;

    [Tooltip("How long the boss must wait before firing another projectile.")]
    [Range(0.5f, 15f)]
    [SerializeField] private float projectileCooldown = 4f;

    [Header("Gas Cloud Attack")]

    [Tooltip("The maximum distance at which the boss can activate its gas attack.")]
    [Range(1f, 20f)]
    [SerializeField] private float gasAttackRange = 8f;

    [Tooltip("How long the boss must wait before using the gas attack again.")]
    [Range(1f, 30f)]
    [SerializeField] private float gasCooldown = 10f;

    [Tooltip("The chance that the boss chooses gas when the gas attack is available.")]
    [Range(0f, 1f)]
    [SerializeField] private float gasAttackChance = 0.35f;

    [Header("Attack Timing")]

    [Tooltip("Delay before an attack happens. This acts as a warning period.")]
    [Range(0f, 3f)]
    [SerializeField] private float attackWindupTime = 0.75f;

    [Tooltip("Delay after an attack before the boss can move again.")]
    [Range(0f, 5f)]
    [SerializeField] private float attackRecoveryTime = 1f;

    [Header("Debug")]

    [Tooltip("Displays boss state and attack information in the Console.")]
    [SerializeField] private bool showDebugMessages = true;

    private BossState currentState = BossState.Idle;

    private float nextMeleeAttackTime;
    private float nextProjectileAttackTime;
    private float nextGasAttackTime;

    private bool isAttacking;

    public BossState CurrentState => currentState;

    private void Awake()
    {
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
        FindPlayer();

        // Attacks available when fight starts
        nextMeleeAttackTime = Time.time;
        nextProjectileAttackTime = Time.time;

        // Prevent the boss from immediately opening with the gas attack.
        nextGasAttackTime = Time.time + gasCooldown;
    }

    private void Update()
    {
        if (currentState == BossState.Dead)
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

        if (distanceToPlayer > detectionRange)
        {
            EnterIdleState();
            return;
        }

        HandleCombat(distanceToPlayer);
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;

            if (showDebugMessages)
            {
                Debug.Log("Toxic Boss found the player.", gameObject);
            }
        }
        else if (showDebugMessages)
        {
            Debug.LogWarning(
                "Toxic Boss could not find an object tagged Player.",
                gameObject
            );
        }
    }

    private void HandleCombat(float distanceToPlayer)
    {
        /*
         * Attack priority:
         *
         * 1. Gas cloud, if available and its random chance succeeds
         * 2. Melee slam, if the player is close
         * 3. Projectile, if the player is within projectile range
         * 4. Chase the player
         */

        if (CanUseGasAttack(distanceToPlayer))
        {
            StartCoroutine(PerformGasAttack());
            return;
        }

        if (CanUseMeleeAttack(distanceToPlayer))
        {
            StartCoroutine(PerformMeleeAttack());
            return;
        }

        if (CanUseProjectileAttack(distanceToPlayer))
        {
            StartCoroutine(PerformProjectileAttack());
            return;
        }

        ChasePlayer();
    }

    private bool CanUseMeleeAttack(float distanceToPlayer)
    {
        return distanceToPlayer <= meleeRange &&
               Time.time >= nextMeleeAttackTime;
    }

    private bool CanUseProjectileAttack(float distanceToPlayer)
    {
        return distanceToPlayer <= projectileRange &&
               Time.time >= nextProjectileAttackTime;
    }

    private bool CanUseGasAttack(float distanceToPlayer)
    {
        if (distanceToPlayer > gasAttackRange)
        {
            return false;
        }

        if (Time.time < nextGasAttackTime)
        {
            return false;
        }

        return Random.value <= gasAttackChance;
    }

    private void ChasePlayer()
    {
        if (!agent.isOnNavMesh)
        {
            return;
        }

        currentState = BossState.Chasing;
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void EnterIdleState()
    {
        if (currentState != BossState.Idle && showDebugMessages)
        {
            Debug.Log("Toxic Boss entered Idle state.", gameObject);
        }

        currentState = BossState.Idle;
        StopMoving();
    }

    private void StopMoving()
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = true;
        agent.ResetPath();
    }

    private void FacePlayer()
    {
        if (player == null)
        {
            return;
        }

        Vector3 directionToPlayer =
            player.position - transform.position;

        // Ignore vertical differences so the boss does not tilt.
        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude <= 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(
            directionToPlayer.normalized
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private IEnumerator PerformMeleeAttack()
    {
        BeginAttack();

        // Set the cooldown when the attack begins.
        nextMeleeAttackTime = Time.time + meleeCooldown;

        if (showDebugMessages)
        {
            Debug.Log(
                "Toxic Boss is preparing TOXIC SLAM.",
                gameObject
            );
        }

        yield return new WaitForSeconds(attackWindupTime);

        /*
         * Toxic Slam damage will be added next.
         *
         * This is where we will use Physics.OverlapSphere
         * to find and damage the player.
         */

        if (showDebugMessages)
        {
            Debug.Log(
                "Toxic Boss used TOXIC SLAM.",
                gameObject
            );
        }

        yield return new WaitForSeconds(attackRecoveryTime);

        EndAttack();
    }

    private IEnumerator PerformProjectileAttack()
    {
        BeginAttack();

        nextProjectileAttackTime =
            Time.time + projectileCooldown;

        if (showDebugMessages)
        {
            Debug.Log(
                "Toxic Boss is preparing TOXIC PROJECTILE.",
                gameObject
            );
        }

        yield return new WaitForSeconds(attackWindupTime);

        /*
         * The toxic projectile will be created here
         * after we build the ToxicProjectile script and prefab.
         */

        if (showDebugMessages)
        {
            Debug.Log(
                "Toxic Boss used TOXIC PROJECTILE.",
                gameObject
            );
        }

        yield return new WaitForSeconds(attackRecoveryTime);

        EndAttack();
    }

    private IEnumerator PerformGasAttack()
    {
        BeginAttack();

        nextGasAttackTime = Time.time + gasCooldown;

        if (showDebugMessages)
        {
            Debug.Log(
                "Toxic Boss is preparing TOXIC GAS CLOUD.",
                gameObject
            );
        }

        yield return new WaitForSeconds(attackWindupTime);

        /*
         * The toxic gas cloud will be created here
         * after we build the ToxicGasCloud script and prefab.
         */

        if (showDebugMessages)
        {
            Debug.Log(
                "Toxic Boss used TOXIC GAS CLOUD.",
                gameObject
            );
        }

        yield return new WaitForSeconds(attackRecoveryTime);

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
        if (currentState == BossState.Dead)
        {
            return;
        }

        isAttacking = false;
        currentState = BossState.Chasing;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }

    public void SetBossDead()
    {
        currentState = BossState.Dead;
        isAttacking = false;

        StopAllCoroutines();
        StopMoving();

        if (agent != null)
        {
            agent.enabled = false;
        }

        if (showDebugMessages)
        {
            Debug.Log("Toxic Boss entered Dead state.", gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        // Melee range
        Gizmos.DrawWireSphere(
            transform.position,
            meleeRange
        );

        // Gas attack range
        Gizmos.DrawWireSphere(
            transform.position,
            gasAttackRange
        );
    }
}