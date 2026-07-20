using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;

public class GrannyBossAI : MonoBehaviour, IDamage, IInteract, IFreeze
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
    [SerializeField] List<GameObject> movementPos;
    [SerializeField] private int rotateSpeed;

    [SerializeField] public GameObject exitPortal;


    [SerializeField] private float wanderRadius;
    [SerializeField] private float wanderTimer;

    [Header("Audio")]
    [SerializeField] BaseSoundSO _hit;
    [SerializeField] BaseSoundSO _dead;
    [SerializeField] BaseSoundSO _lightning;
    [SerializeField] BaseSoundSO _grunts;

    [Header("Weapon")]
    // may need multiple types of bullets so make another one if need be
    //[SerializeField] GameObject bullet;

    //only needed if you are using the same type of gun pivot as the enemies
    

    //might need multiple different shoot rates for a boss
    [Range(0.1f, 2f)][SerializeField] float shootRate;

    [Header("Currency")]
    [SerializeField] private int minCurrencyDrop = 10;
    [SerializeField] private int maxCurrencyDrop = 25;

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


        currentHealth = maxHealth;

        originalColor = model.material.color;

        player = GameObject.FindGameObjectWithTag("Player").transform;

        currentState = BossState.Rest;

        gameManager.instance.updateGameGoal(1);

        timer = -100;
        phasePicker = 0;
        firespawn = transform.position;
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
        if (allowedAttack)
        {
            FacePlayer();
            switch (currentState)
            {
                case BossState.Rest:
                    Rest();

                    break;

                case BossState.Attack1:
                    Attack1();

                    break;

                case BossState.Attack2:
                    Attack2();

                    break;
                case BossState.Attack3:
                    Attack3();
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
    }
    private bool allowedMovement = false;
    private bool allowedAttack = true;
    private void Movement()
    {
        if (!allowedMovement)
        { return; }
        allowedAttack = false;

        int pathPicker = Random.Range(1, movementPos.Count);
        UnityEngine.Vector3 end;

        end = movementPos[pathPicker].transform.position;
        end.y = 2f;


        int willJump = Random.Range(1, 3);

        StartCoroutine(Turn(end, willJump));

        allowedMovement = false;
    }
    bool firstmove = true;
    IEnumerator Turn(UnityEngine.Vector3 end, int willJump)
    {
        UnityEngine.Vector3 dir = end - transform.position;
        UnityEngine.Quaternion target;
        target = UnityEngine.Quaternion.LookRotation(dir);

        if (firstmove)
        {
            while (UnityEngine.Quaternion.Angle(transform.rotation, target) > 1f)
            {
                transform.rotation = UnityEngine.Quaternion.RotateTowards(
                    transform.rotation,
                    target,
                    2000 * Time.deltaTime);
                yield return null;
            }
            firstmove = false;
        }
        else
        {
            while (UnityEngine.Quaternion.Angle(transform.rotation, target) > 1f)
            {
                transform.rotation = UnityEngine.Quaternion.RotateTowards(
                    transform.rotation,
                    target,
                    rotateSpeed * Time.deltaTime);
                yield return null;
            }
        }
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
    public float moveTime;
    IEnumerator Moving(UnityEngine.Vector3 startPos, UnityEngine.Vector3 endPos)
    {
        
        float time = 0f;

        while (time < moveTime)
        {
            time += Time.deltaTime;
            transform.position = UnityEngine.Vector3.Lerp(startPos, endPos, time / moveTime);
            yield return null;
        }
        transform.position = endPos;
        allowedAttack = true;
    }
    [SerializeField] int jumpHeight;
    IEnumerator Jumping(UnityEngine.Vector3 startPos, UnityEngine.Vector3 endPos)
    {
        float time = 0f;

        while (time < moveTime)
        {
            time += Time.deltaTime;

            UnityEngine.Vector3 position = UnityEngine.Vector3.Lerp(startPos, endPos, time / moveTime);
            position.y += Mathf.Sin(time / moveTime * Mathf.PI) * jumpHeight;
            transform.position = position;

            yield return null;
        }
        transform.position = endPos;
        allowedAttack = true;
    }

    private void Rest()
    {
        if (PlayerInTrigger)
        {
            currentState = BossState.Decide;
        }
    }
    bool canAttack1 = true;
    IEnumerator attack1Cooldown()
    {
        allowedMovement = true;
        yield return new WaitForSeconds(15f);
        canAttack1 = false;
    }
    bool canAttack2 = true;
    IEnumerator attack2Cooldown()
    {
        allowedMovement = true;
        attackEventFired = false;
        yield return new WaitForSeconds(16f);
        canAttack2 = true;
    }
    bool canAttack3 = true;
    IEnumerator attack3Cooldown()
    {
        timer = -100;
        allowedMovement = true;
        yield return new WaitForSeconds(16f);
        canBomb = true;

    }
    bool isAttack1 = false;

    [Header("Lightning Strike Attack")]
    [SerializeField] GameObject lightningStrikes;
    [SerializeField] GameObject lightningStrikeIndicator;
    [Range(1, 100)][SerializeField] int totalStrikes;
    [Range(1, 50)][SerializeField] int strikesAttackArea;
    [SerializeField] float lightningChargeUpTime;
    [SerializeField] float lightningDestroyTime;
    private bool attack1On = false;
    private bool attack2On = false;
    private bool attack3On = false;
    private int numStrikes;
    private Vector3 posOrig;
    private void Attack1()
    {
        if (!canAttack1)
        {
            currentState = BossState.Decide;
            return;
        }
        if (!isAttack1)
        {
            timer = Random.Range(1000, 1500);
            AudioManager.instance.PlaySoundFromSource(_grunts, gameObject);
            isAttack1 = true;

        }


        Debug.Log("Attack 1");
        //attacks go here

        if (totalStrikes > numStrikes && isAttack1)
        {
            attack1On = true;
            StartCoroutine(LightningStrikes());
        }

        if (totalStrikes == numStrikes)
        {
            attack1On = false;
        }


        if (timer < 0)
        {
            if (gun != null)
            {
                gun.SetActive(false);
            }

            currentState = BossState.Rest;
            isAttack1 = false;
            canAttack1 = false;
            StartCoroutine(attack1Cooldown());
        }


        timer--;

    }

    IEnumerator LightningStrikes()
    {
        numStrikes++;

        yield return new WaitForSeconds(0.2f);
        Vector3 ranPos = transform.position + Random.insideUnitSphere * strikesAttackArea;
        ranPos.y = transform.position.y + 10f;

        if (Physics.Raycast(ranPos, Vector3.down, out RaycastHit hit, 100f))
        {
            ranPos = hit.point;
        }
        Vector3 groundStandOff = new Vector3(0, 0.2f, 0);
        ranPos += groundStandOff;

        Quaternion spread = Quaternion.Euler(0, Random.Range(0, 360), 0);

        GameObject warning = Instantiate(lightningStrikeIndicator, ranPos, spread);
        yield return new WaitForSeconds(lightningChargeUpTime);
        GameObject lightning = Instantiate(lightningStrikes, ranPos, Quaternion.Euler(0, Random.Range(0, 360), 0));

        AudioManager.instance.PlaySoundAtPosition(_lightning, lightning);

        Collider[] playerHits = Physics.OverlapSphere(lightning.transform.position, 3, LayerMask.GetMask("Player"));

        if (playerHits.Length > 0)
        {
            foreach (Collider struck in playerHits)
            {
                IDamage dmg = gameManager.instance.playerStatHandler.GetComponentInChildren<IDamage>();
                dmg?.takeDamage(attackDamage);
            }
        }

        Destroy(warning, lightningDestroyTime);
        Destroy(lightning, lightningDestroyTime);
    }

    private void Attack2()
    {
        Debug.Log("missing");
        if (!canAttack2)
        {
            currentState = BossState.Decide;
            return;
        }
        Debug.Log("Attack 2");
        StartCoroutine(BeginThermalBacklash());
    }
    [Header("Thermal Backlash")]
    [SerializeField] private float backlashHealPercent = 0.10f;
    [SerializeField] private float backlashHealDuration = 3f;
    [SerializeField] private int backlashBaseDamage = 15;
    [SerializeField] private float backlashCooldown = 18f;
    [SerializeField] private GameObject selfFireEffect;
    [SerializeField] private GameObject backlashShockwavePrefab;
    private IEnumerator BeginThermalBacklash()
    {
        Debug.Log("Attack 2 Actual");
        canAttack2 = false;
        allowedMovement = false;
        allowedAttack = false;
        isInBacklash = true;

        //Play Start-Up animation
        
        selfFireEffect.SetActive(true);

        //PlayAnim(backlashState);

        

        int startHealth = currentHealth;
        int totalHeal = Mathf.RoundToInt(maxHealth * backlashHealPercent);
        int targetHealth = Mathf.Min(maxHealth, startHealth + totalHeal);

        float elapsed = 0f;
        while (elapsed < backlashHealDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / backlashHealDuration);
            currentHealth = Mathf.RoundToInt(Mathf.Lerp(startHealth, targetHealth, t));
            yield return null;
        }

        currentHealth = targetHealth;
        
            selfFireEffect.SetActive(false);


        yield return new WaitForSeconds(0.6f);

        //PlayAnim(idleState);
        allowedAttack = true;
        allowedMovement = true;
        currentState = BossState.Rest;
        isInBacklash = false;
        SpawnBacklashShockwave();
        StartCoroutine(attack2Cooldown());
    }
    private bool attackEventFired;
    private int storedBacklashDamage;
    private Vector3 firespawn;
    public void SpawnBacklashShockwave()
    {
        if (attackEventFired) return;
        attackEventFired = true;

        if (backlashShockwavePrefab == null)
            return;

        GameObject shockwave = Instantiate(backlashShockwavePrefab,firespawn , Quaternion.identity);
        shockwave.SetActive(true);
        ThermalBacklashShockwave shockwaveScript = shockwave.GetComponent<ThermalBacklashShockwave>();
        if (shockwaveScript != null)
            shockwaveScript.SetDamage(backlashBaseDamage + storedBacklashDamage);
    }
    bool isAttack3 = false;
    bool attacking = true;
    bool isBombing = false;
    private void Attack3()
    {
        if (!canAttack3)
        {
            currentState = BossState.Decide;
            return;
        }
        if (attacking)
        {
            if (!isBombing)
            {
               
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
            StartCoroutine(attack3Cooldown());
            currentState = BossState.Decide;
        }
    }
    bool canBomb = true;
    [SerializeField] BoxCollider bombSpawnArea;
    [SerializeField] GameObject bomb;
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


    private void Decide()
    {
        if (!PlayerInTrigger)
        {
            Rest();
        }
        else if (timer < -100)
        {
            timer = Random.Range(1, 3000); //1200
        }

        timer -= 1;

        if (timer < 0)
        {
            //phasePicker = Random.Range(1, 4);// picks from a range of 1 2 or 3
            phasePicker = 3;
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
    public bool isInBacklash = false;
    public void takeDamage(int amount)
    {
        if (isInBacklash)
        {
            storedBacklashDamage += 2;
        }
            PlayerInTrigger = true;
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

            int currencyDrop = GetCurrencyDrop();
            gameManager.instance.addCurrency(currencyDrop);

            if (exitPortal != null)
            {
                exitPortal.SetActive(true);
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
        PlayerInTrigger = true;
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
