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
    [SerializeField] BoxCollider bombSpawnArea;
    [SerializeField] BoxCollider fallingBombSpawnArea;

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
    [SerializeField] GameObject bomb;
    [SerializeField] GameObject fallingBomb;

    [SerializeField] GameObject weakpoint1;
    [SerializeField] GameObject weakpoint2;
    [SerializeField] GameObject weakpoint3;
    [SerializeField] GameObject weakpoint4;

    [SerializeField] List<GameObject> skyBombs;
    [SerializeField] List<GameObject> laser;
    [SerializeField] List<GameObject> skyAreas1;
    [SerializeField] List<GameObject> skyAreas2;

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
    private bool attacking = false;
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
        
       for(int i = 0; i < skyBombs.Count; i++)
        {
            GameObject newBomb = Instantiate(skyBombs[i]);
            newBomb.SetActive(false);

            skyBombs[i] = newBomb;

            GameObject newLaser = Instantiate(laser[i]);
            newLaser.SetActive(false);

            laser[i] = newLaser;
            
        }

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
                CrystalBomb();

                break;

            case BossState.Attack2:
                ArmoredUp();

                break;
            case BossState.Attack3:
                FallingBombs();
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
    bool canBomb = true;
    IEnumerator BombCooldown()
    {
        yield return new WaitForSeconds(15f);
        canBomb = true;
    }
    bool canFallBomb = true;
    IEnumerator FallBombCooldown()
    {
        yield return new WaitForSeconds(15f);
        canFallBomb = true;
        noBomb = false;
        fallBombReady = false;
    }
    bool fallBombReady = false;
    bool noBomb = false;
    int patternPicker;
    private void FallingBombs()
    {
        Debug.Log("Attack 3");
        if(!canFallBomb)
        {
            currentState = BossState.Decide;
            return;
        }
        else if(!noBomb)
        {
           patternPicker = Random.Range(1, 3);
        }

            

        switch (patternPicker)
        {
            case 1:
                noBomb = true;
                if (!fallBombReady)
                {
                    if (!laser[0].activeSelf)
                    {
                        StartCoroutine(FallBombStartUp());
                    }
                    
                    for(int i = 0; i < skyBombs.Count; i++)
                    {
                        laser[i].transform.position = skyAreas1[i].transform.position;                        
                    }
                    return;
                }
                for (int i = 0; i < skyBombs.Count; i++)
                {
                    skyBombs[i].transform.position = skyAreas1[i].transform.position;
                    skyBombs[i].SetActive(true);                   
                }
                canFallBomb = false;
                StartCoroutine(FallBombCooldown());
                    break;
            case 2:
                noBomb = true;
                if (!fallBombReady)
                {
                    if (!laser[0].activeSelf)
                    {
                        StartCoroutine(FallBombStartUp());
                    }

                    for (int i = 0; i < skyBombs.Count; i++)
                    {
                        laser[i].transform.position = skyAreas2[i].transform.position;
                    }
                    return;
                }
                for (int i = 0; i < skyBombs.Count; i++)
                {
                    skyBombs[i].transform.position = skyAreas2[i].transform.position;
                    skyBombs[i].SetActive(true);
                }
                canFallBomb = false;
                StartCoroutine(FallBombCooldown());
                break;
        }
       
        currentState = BossState.Decide;
    }
    IEnumerator FallBombStartUp()
    {
        for (int i = 0; i < laser.Count; i++)
        {
            laser[i].SetActive(true);
            laser[i].GetComponent<LaserEnter>().Grower();
        }
        yield return new WaitForSeconds(10f);

        fallBombReady = true;
    }
    private void CrystalBomb()
    {
        if(!canBomb)
        {
            currentState = BossState.Decide;
            return;
        }
        if(attacking)
        {
            if (!isBombing)
            {
                isBombing = true;
                StartCoroutine(BombPlacer());

            }
            timer -= Time.deltaTime;
            Debug.Log("Timer: " + timer);
           
        }
        else
        {
            attacking = true;
            timer = Random.Range(5f, 10f);
        }
            Debug.Log("Attack 1");      

        
        if (!PlayerInTrigger)
        {
            currentState = BossState.Rest;
        }
        if(timer <= 0f)
        {
            Debug.Log("Timer: " + timer);
            attacking = false;
            timer = -100;
            canBomb = false;
            StartCoroutine(BombCooldown());
            currentState = BossState.Decide;
        }
        
    }
    private bool isBombing = false;
    IEnumerator BombPlacer()
    {
        
            //place a bomb
            Bounds bounds = bombSpawnArea.bounds;

            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.max.y, bounds.min.y);
            float z = Random.Range(bounds.max.z, bounds.min.z);
            Vector3 vec = new Vector3(x, y, z);

            Quaternion rot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            Instantiate(bomb, vec, rot);
       float rand = Random.Range(1f, 2f);
            yield return new WaitForSeconds(rand);

        isBombing=false;
        
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
            timer = Random.Range(300, 1200); //1200
        }

        timer -= 1;

        if (timer < 0)
        {
            phasePicker = Random.Range(1, 4);// picks from a range of 1 2 or 3
            //phasePicker = 3;
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

            for (int i = 0; i < laser.Count; i++)
            {
                laser[i].SetActive(false);
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
