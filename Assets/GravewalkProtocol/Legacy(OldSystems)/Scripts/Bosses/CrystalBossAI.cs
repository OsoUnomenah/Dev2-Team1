using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class CrystalBossAI : MonoBehaviour, IDamage, IInteract, IFreeze, IBossTrigger
{


    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] int xpGive = 100;
    [SerializeField] private int minCurrencyDrop = 10;
    [SerializeField] private int maxCurrencyDrop = 25;
    [SerializeField] Renderer model;
    [SerializeField] BoxCollider bombSpawnArea;
    [SerializeField] BoxCollider fallingBombSpawnArea;
    [SerializeField] List<GameObject> movementPos;
    [SerializeField] private int rotateSpeed;
    [SerializeField] private Animator animator;

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
    [SerializeField] BaseSoundSO _slam;
    [SerializeField] BaseSoundSO _laserStartUp;

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

    private UnityEngine.Vector3 lastHeardPosition;
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
    int phasePicker = 1;

    UnityEngine.Vector3 playerDir;

    Color originalColor;
    [SerializeField] GameObject gun;


    // Start is called once before the first execution of Update after the MonoBehaviour is created


    private void Start()
    {
        exitPortal = GameObject.FindGameObjectWithTag("Portal");
        exitPortal.SetActive(false);

        for (int i = 0; i < skyBombs.Count; i++)
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
    IEnumerator NeedArmor()
    {
        mustArmor = false;
        if(armorRoutine)
        {
            yield break;
        }

        yield return new WaitForSeconds(50f);

        if (!isArmored)
        {
            mustArmor = true;
            armorRoutine = false;
        }
    }
    private bool mustArmor = false;
    bool armorRoutine = false;
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
        if (mustArmor)
        {
            armorRoutine = true;
            StartCoroutine(NeedArmor());
        }

        float distance = UnityEngine.Vector3.Distance(transform.position, player.position);
        // Debug.Log("Attack" + allowedAttack);
        ArmorCheck();
        if (allowedAttack)
        {
            FacePlayer();
            switch (currentState)
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
        // Debug.Log("Movement" + allowedMovement);
        Movement();
    }
    private void FacePlayer()
    {
        UnityEngine.Vector3 dir = player.transform.position - transform.position;
        dir.y = 0f;

        UnityEngine.Quaternion target = UnityEngine.Quaternion.LookRotation(dir);

        transform.rotation = UnityEngine.Quaternion.RotateTowards(
            transform.rotation,
            target,
            rotateSpeed * Time.deltaTime);
        float angle = Quaternion.Angle(transform.rotation, target);
        if (angle > 1f)
        {
            animator.SetBool("Mutant Walking", true);
        }
        else
        {
            animator.SetBool("Mutant Walking", false);
        }
    }
    private bool allowedMovement = false;
    private bool allowedAttack = true;
    private void Movement()
    {
        if (!allowedMovement)
        { return; }
        allowedAttack = false;

        int pathPicker = Random.Range(1, 5);
        UnityEngine.Vector3 end;

        end = movementPos[pathPicker].transform.position;
        


        int willJump = Random.Range(1, 3);

        StartCoroutine(Turn(end, willJump));

        allowedMovement = false;
    }
    IEnumerator Turn(UnityEngine.Vector3 end, int willJump)
    {
        UnityEngine.Vector3 dir = end - transform.position;
        UnityEngine.Quaternion target;
        target = UnityEngine.Quaternion.LookRotation(dir);

        
        while (UnityEngine.Quaternion.Angle(transform.rotation, target) > 1f)
        {
            animator.SetBool("Mutant Walking", true);
            transform.rotation = UnityEngine.Quaternion.RotateTowards(
                transform.rotation,
                target,
                rotateSpeed * Time.deltaTime);
            yield return null;
        }
        animator.SetBool("Mutant Walking", false);
        switch (willJump)
        {
            case 1:    //not jumping
                Debug.Log("walking");
                StartCoroutine(Moving(transform.position, end));
                break;
            case 2:    //jumping
                Debug.Log("jumping");
                StartCoroutine(Jumping(transform.position, end));
                break;
        }
    }
    IEnumerator Moving(UnityEngine.Vector3 startPos, UnityEngine.Vector3 endPos)
    {
        float moveTime = 2f;
        float time = 0f;

        animator.SetBool("Mutant Walking", true);
        while (time < moveTime)
        {
            time += Time.deltaTime;
            transform.position = UnityEngine.Vector3.Lerp(startPos, endPos, time / moveTime);
            yield return null;
        }
        transform.position = endPos;
        allowedAttack = true;

        animator.SetBool("Mutant Walking", false);
    }
    [SerializeField] int jumpHeight;
    IEnumerator Jumping(UnityEngine.Vector3 startPos, UnityEngine.Vector3 endPos)
    {
        float moveTime = 2f;
        float time = 0f;
        animator.SetBool("Mutant Jumping", true);
        while (time < moveTime)
        {
            time += Time.deltaTime;

            UnityEngine.Vector3 position = UnityEngine.Vector3.Lerp(startPos, endPos, time / moveTime);
            position.y += Mathf.Sin(time / moveTime * Mathf.PI) * jumpHeight;
            transform.position = position;

            yield return null;
        }
        animator.SetBool("Mutant Jumping", false);
        transform.position = endPos;
        allowedAttack = true;
    }

    private void Rest()
    {

       // animator.SetTrigger("Mutant Breathing Idle");
        if (PlayerInTrigger)
        {
            currentState = BossState.Decide;
        }
    }
    bool isArmored = false;
    bool brokenArmor = false;
    private void ArmoredUp()
    {
        Debug.Log("Attack 2");
        if (armor.activeSelf)
        {
            Debug.Log("leave method");
            currentState = BossState.Decide;
            return;
        }
        if (!armor.activeSelf)
        {
            Debug.Log("does method again");
            isArmored = true;
            canDamage = false;
            timer = -100;
            animator.SetTrigger("Mutant Flexing Muscles");
            armor.SetActive(true);
            StartCoroutine(ChargeUp());
            weakpoint1.SetActive(true);
            weakpoint1.GetComponent<Weakpoints>().activateWeakpoint();

            weakpoint2.SetActive(true);
            weakpoint2.GetComponent<Weakpoints>().activateWeakpoint();

            weakpoint3.SetActive(true);
            weakpoint3.GetComponent<Weakpoints>().activateWeakpoint();

            weakpoint4.SetActive(true);
            weakpoint4.GetComponent<Weakpoints>().activateWeakpoint();
            
        }
        if (!PlayerInTrigger)
        {
            armor.SetActive(false);
            currentState = BossState.Rest;
            timer = -100;

        }
    }
    public Material material;
    IEnumerator ChargeUp()
    {
       // material.EnableKeyword("_EMISSION");

        //Color color = Color.white;

        float time = 0f;
        float duration = 1f;

        while (time < duration)
        {
            //float intensity = Mathf.Lerp(0f, 4f, time / duration);

            //material.SetColor("_EmissionColor", color * intensity);

            time += Time.deltaTime;
            yield return null;
        }
    }
    private void ArmorCheck()
    {
        if (!weakpoint1.activeSelf && !weakpoint2.activeSelf && !weakpoint3.activeSelf && !weakpoint4.activeSelf)
        {
            //Add Armor
            armor.SetActive(false);
           brokenArmor = true;

            StartCoroutine(armorCooldown());
        }
    }
    bool canDamage = true;
    IEnumerator armorCooldown()
    {
        timer = -100;
        canDamage = true;
        yield return new WaitForSeconds(15f);
        isArmored = false;
        brokenArmor = false;
    }
    bool canBomb = true;
    IEnumerator BombCooldown()
    {
        timer = -100;
        allowedMovement = true;
        yield return new WaitForSeconds(16f);
        canBomb = true;
    }
    bool canFallBomb = true;
    IEnumerator FallBombCooldown()
    {
        timer = -100;
        allowedMovement = true;
        yield return new WaitForSeconds(18f);
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
        if (!canFallBomb)
        {
            currentState = BossState.Decide;
            return;
        }
        else if (!noBomb)
        {
            patternPicker = Random.Range(1, 3);
            allowedMovement = false;
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

                    for (int i = 0; i < skyBombs.Count; i++)
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

                    animator.SetTrigger("Mutant Swiping");
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
        Debug.Log(_laserStartUp);
        Debug.Log(_laserStartUp == null ? "NULL" : _laserStartUp.name);
        AudioManager.instance.PlaySound(_laserStartUp);
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
        if (!canBomb)
        {
            currentState = BossState.Decide;
            return;
        }
        if (attacking)
        {            
            if (!isBombing)
            {
                if (!allowedMovement)
                {
                    animator.SetTrigger("Mutant Rise");
                }
                allowedMovement = false;
                isBombing = true;
                StartCoroutine(BombPlacer());

            }
            timer -= Time.deltaTime;
           // Debug.Log("Timer: " + timer);

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
        if (timer <= 0f)
        {
            // Debug.Log("Timer: " + timer);
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
        UnityEngine.Vector3 vec = new UnityEngine.Vector3(x, y, z);

        UnityEngine.Quaternion rot = UnityEngine.Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        Instantiate(bomb, vec, rot);
        float rand = Random.Range(1f, 2f);
        yield return new WaitForSeconds(rand);

        isBombing = false;

    }
    [SerializeField] BoxCollider BossRoom;
    public void TriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !PlayerInTrigger)
        {
            PlayerInTrigger = true;
            model.material.color = Color.orange;
        }
    }


    private void Decide()
    {
        if (!PlayerInTrigger)
        {
            Rest();
        }
        else if (phasePicker != 0)
        {
            timer = Random.Range(300, 1200); //1200
        }


        timer -= 1;
        //Debug.Log(timer);
        if (timer <= 0)
        {
            phasePicker = Random.Range(1, 4);// picks from a range of 1 2 or 3
                                             //phasePicker = 3;
            if (phasePicker == 2 && isArmored)
            {
                while (phasePicker != 2)
                {
                    phasePicker = Random.Range(1, 4);
                }
            }
        }
        else
        {
            phasePicker = 0;
        }
        if (mustArmor)
        {
            phasePicker = 2;
            mustArmor = false;
        }

        switch (phasePicker )
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
        if(canDamage == false)
        {
            return;
        }

        //Set the damage to display on the damage text
        gameManager.instance.playerDamageOut = amount;

        //Show the damage text
        StartCoroutine(updateDamageText());

        currentHealth -= amount;


        if (currentHealth <= 0)
        {
            gameManager.instance.BossDie();
            allowedMovement = false;
            currentState = BossState.Dead;
            if (agent0 != null)
                agent0.isStopped = true;

            AudioManager.instance.PlaySoundAtPosition(_dead, gameObject);

            gameManager.instance.updateGameGoal(-1);
            gameManager.instance.addXp(xpGive);
            RecticleBehaviour.OffHover();

            int currencyDrop = GetCurrencyDrop();
            gameManager.instance.addCurrency(currencyDrop);

            if (exitPortal != null)
            {
                exitPortal.SetActive(true);
            }

            for (int i = 0; i < laser.Count; i++)
            {
                laser[i].SetActive(false);
            }
            animator.SetBool("Mutant Dying", true);
            StartCoroutine(Dying());
            canDamage = false;
        }
        else
        {
            AudioManager.instance.PlaySoundAtPosition(_hit, gameObject);
            StartCoroutine(flashRed());
        }
    }
    IEnumerator Dying()
    {
        
        yield return new WaitForSeconds(2f);
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * 2.5f;

        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);

            elapsed += Time.deltaTime;
            yield return null;
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

    private int GetCurrencyDrop()
    {
        return Random.Range(minCurrencyDrop, maxCurrencyDrop + 1);
    }
}
