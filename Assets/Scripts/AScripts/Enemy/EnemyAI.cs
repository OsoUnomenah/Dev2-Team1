using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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

    // Update is called once per frame
    void Update()
    {
        if (currentState == ZombieState.Dead)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {

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
