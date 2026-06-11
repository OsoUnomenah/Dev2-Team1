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

public class BossAI : MonoBehaviour, IDamage, IInteract
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


    [SerializeField] private float wanderRadius;
    [SerializeField] private float wanderTimer;

    [Header("Audio")]
    [SerializeField] BaseSoundSO _hit;
    [SerializeField] BaseSoundSO _dead;

    [Header("Weapon")]
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gunPivot;
    [SerializeField] Transform shootPos;
    [Range(0.1f, 2f)][SerializeField] float shootRate;
    [Range(1, 10)][SerializeField] int gunRotateSpeed;

    [Header("Don't touch unles debugging")]
    [SerializeField] List<int> Modifiers;

    private Vector3 lastHeardPosition;
    private bool heardNoise;
    private Transform player;
    

    private float nextAttackTime;
    private float attackCooldown = 1.5f;
    private float RestTime;
    private bool PlayerInTrigger;
    private float timer;
    private enum ZombieState
    {
        Rest,
        Attack,
        ShootPhase,
        SpinPhase,
        LavaPhase,
        Dead
    }

    private ZombieState currentState;
    int phasePicker;

    Vector3 playerDir;

    Color originalColor;
    [SerializeField] GameObject gun;
    [SerializeField] GameObject lava1;
    [SerializeField] GameObject lava2;
    [SerializeField] GameObject lava3;
    [SerializeField] GameObject lava4;
    [SerializeField] GameObject lava5;
    [SerializeField] GameObject lava6;
    [SerializeField] GameObject lava7;
    [SerializeField] GameObject lava8;

    private NavMeshAgent agent1;
    private NavMeshAgent agent2;
    private NavMeshAgent agent3;
    private NavMeshAgent agent4;
    private NavMeshAgent agent5;
    private NavMeshAgent agent6;
    private NavMeshAgent agent7;
    private NavMeshAgent agent8;

    private Vector3 transform1;
    private Vector3 transform2;
    private Vector3 transform3;
    private Vector3 transform4;
    private Vector3 transform5;
    private Vector3 transform6;
    private Vector3 transform7;
    private Vector3 transform8;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        agent1 = lava1.GetComponent<NavMeshAgent>();
        agent2 = lava2.GetComponent<NavMeshAgent>();
        agent3 = lava3.GetComponent<NavMeshAgent>();
        agent4 = lava4.GetComponent<NavMeshAgent>();
        agent5 = lava5.GetComponent<NavMeshAgent>();
        agent6 = lava6.GetComponent<NavMeshAgent>();
        agent7 = lava7.GetComponent<NavMeshAgent>();
        agent8 = lava8.GetComponent<NavMeshAgent>();

        transform1 = lava1.transform.position;
        transform2 = lava2.transform.position;
        transform3 = lava3.transform.position;
        transform4 = lava4.transform.position;
        transform5 = lava5.transform.position;
        transform6 = lava6.transform.position;
        transform7 = lava7.transform.position;
        transform8 = lava8.transform.position;

        currentHealth = maxHealth;
        
        originalColor = model.material.color;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        currentState = ZombieState.Rest;
        
        gameManager.instance.updateGameGoal(1);

        timer = -100;
        phasePicker = 0;
        
    }

    private void Update()
    {
        playerDir = gameManager.instance.player.transform.position - transform.position;
        updateHealthBar();
        if (currentState == ZombieState.Dead)
            return;
        if (player == null)
        {
            currentState = ZombieState.Rest;
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        switch(currentState)
        {
            case ZombieState.Rest:                
                Rest();
                
                break;

            case ZombieState.SpinPhase:
                Spin();

                break;

            case ZombieState.ShootPhase:
                Shoot();

                break;
            case ZombieState.LavaPhase:
                Lava();
                break;

            case ZombieState.Attack:
                Attack();

                break;
        }
    }

    private void Rest()
    {
        if (PlayerInTrigger)
        {
            currentState = ZombieState.Attack;
        }
    }

    private void Shoot()
    {       
        if (!gun.activeSelf)
        {
            gun.SetActive(true);
            timer = Random.Range(120, 1200);
        }
        if (!PlayerInTrigger || timer < 0)
        {            
            gun.SetActive(false);
            currentState = ZombieState.Rest;
            timer = -100;
            
        }
        Quaternion rot = Quaternion.LookRotation(playerDir);
        gunPivot.rotation = Quaternion.Lerp(gunPivot.rotation, rot, 3 * Time.deltaTime);
    
        Instantiate(bullet, shootPos.position, gunPivot.rotation);
        timer--;
    }
    private void Lava()
    {
        if (!lava1.activeSelf)
        {
            lava1.SetActive(true);
            lava2.SetActive(true);
            lava3.SetActive(true);
            lava4.SetActive(true);
            lava5.SetActive(true);
            lava6.SetActive(true);
            lava7.SetActive(true);
            lava8.SetActive(true);
            timer = Random.Range(1200, 3000);
            
        }
        if (!PlayerInTrigger || timer < 0)
        {
            gun.SetActive(false);
            currentState = ZombieState.Rest;

            lava1.SetActive(false);
            lava2.SetActive(false);
            lava3.SetActive(false);
            lava4.SetActive(false);
            lava5.SetActive(false);
            lava6.SetActive(false);
            lava7.SetActive(false);
            lava8.SetActive(false);

            lava1.transform.position = transform1;
            lava2.transform.position = transform2;
            lava3.transform.position = transform3;
            lava4.transform.position = transform4;
            lava5.transform.position = transform5;
            lava6.transform.position = transform6;
            lava7.transform.position = transform7;
            lava8.transform.position = transform8;

            timer = -100;
            return;
        }
        

        agent1.SetDestination(player.position);
        agent2.SetDestination(player.position);
        agent3.SetDestination(player.position);
        agent4.SetDestination(player.position);
        agent5.SetDestination(player.position);
        agent6.SetDestination(player.position);
        agent7.SetDestination(player.position);
        agent8.SetDestination(player.position);

        timer--;
    }
    private void Spin()
    {
        if (!PlayerInTrigger)
        {
            currentState = ZombieState.Rest;
        }
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

    private void Attack()
    {
        //if (!PlayerInTrigger)
        //{
        //    Rest();
        //}
        //else if(timer == -100)
        //{
        //    timer = Random.Range(1, 3000); //1200
        //}

        //timer -= 1;

        //if (timer < 0)
        //{
        //    phasePicker = Random.Range(1, 3);
        //}
        if(phasePicker < 2)
        {
            phasePicker = 2;
        }
        else
        {
            phasePicker = 1;
        }
        switch (phasePicker)
        {
            case 0:
                break;
            case 1:
                currentState = ZombieState.ShootPhase;
                break;
            case 2:
                currentState = ZombieState.LavaPhase;
                break;
            case 3:
                currentState = ZombieState.SpinPhase;
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
            currentState = ZombieState.Dead;
            if (agent0 != null)
                agent0.isStopped = true;

            AudioManager.instance.PlaySoundAtPosition(_dead, gameObject);

            gameManager.instance.updateGameGoal(-1);
            gameManager.instance.addXp(xpGive);
            TempUI.OffHover();
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
        TempUI.OnHover(1);
    }
    public void OnHoverExit()
    {
        TempUI.OffHover();
    }
}
