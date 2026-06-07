using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR.Haptics;

public class enemyAI : MonoBehaviour, IDamage, IInteract
{
    [SerializeField] int HP;
    [SerializeField] Renderer model;

    [SerializeField] private float sightRange = 15f;
    [SerializeField] private float attackRange = 2f;

    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderTimer = 5f;

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
    void Start()
    {
        originalColor = model.material.color;

        player = GameObject.FindGameObjectWithTag("Player").transform;

        agent = GetComponent<NavMeshAgent>();

        currentState = ZombieState.Wander;
        
        gameManager.instance.updateGameGoal(1);
        wanderTime = wanderTimer;
    }

    private void Update()
    {
        if (currentState == ZombieState.Dead)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {

        }

        distance = Vector3.Distance(transform.position, player.position);

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
    
    IEnumerator PlaySound(BaseSoundSO sound)
    {
        if (sound != null)
        {
            GameObject soundObject = new GameObject("Temp Audio Source");
            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.clip = sound.clip;
            audioSource.Play();
            yield return new WaitForSeconds(0.3f);
            Destroy(soundObject);
        }
    }
    [SerializeField] private BaseSoundSO _hit;
    [SerializeField] private BaseSoundSO _dead;

    public void takeDamage(int amount)
    {
        HP -= amount;

        if (HP <= 0)
        {
            currentState = ZombieState.Dead;
            if (agent != null)
                agent.isStopped = true;

            StartCoroutine(PlaySound(_dead));
            gameManager.instance.updateGameGoal(-1);
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(PlaySound(_hit));
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
        StartCoroutine(flashGreen());
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
