using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    // Detection
    [SerializeField] private float sightRange = 15.0f;
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private float hearingRange = 20.0f;

    // Combat
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1.5f;

    // wandering
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderTimer = 5f;

    private int currentHealth;
    private float nextAttackTime;
    private float wanderTime;

    private Transform player;
    private NavMeshAgent agent;

    private enum ZombieState
    {
        Wander,
        Chase,
        Attack,
        Dead
    }

    private ZombieState currentState;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();

        currentHealth = maxHealth;
        currentState = ZombieState.Wander;

        wanderTimer = wanderTimer;
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (currentState == ZombieState.Dead)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        switch (currentState)
        {
            case ZombieState.Wander:
                Wander();

                if (distanceToPlayer <= sightRange)
                {
                    currentState = ZombieState.Chase;
                }
                break;  
            case ZombieState.Chase:
                agent.SetDestination(player.position);

                if (distanceToPlayer <= attackRange)
                {
                    currentState = ZombieState.Attack;
                }
                else if (distanceToPlayer > sightRange * 1.5f)
                {
                    currentState = ZombieState.Wander;
                }
                break;

            case ZombieState.Attack:
                agent.SetDestination(transform.position);

                AttackPlayer();

                if (distanceToPlayer > attackRange)
                {
                    currentState = ZombieState.Chase;
                }
                break;
        }
    }
    private void Wander()
    {
        wanderTime += Time.deltaTime;

        if (wanderTime >= wanderTimer)
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;

            randomDirection += transform.position;

            NavMeshHit hit;

            if (NavMesh.SamplePosition (randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination (hit.position);
            }
            wanderTime = 0;
        }
    }
    private void AttackPlayer()
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            Debug.Log("zombie attacked player!");
            // add player health script here later
        }
        
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        Debug.Log("Zombie took damage: " + damageAmount);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            currentState = ZombieState.Chase;
        }

    }
    private void Die()
    {
        currentState = ZombieState.Dead; 
        agent.isStopped = true; 
        Debug.Log("Zombie died"); 
        Destroy(gameObject, 3f);

    }
    public void HearNoise(Vector3 noisePosition)
    {

    }
}
