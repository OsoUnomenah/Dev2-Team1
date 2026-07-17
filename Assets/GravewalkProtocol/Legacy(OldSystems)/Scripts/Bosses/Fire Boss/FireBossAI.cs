using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class FireBossAI : MonoBehaviour, IDamage, IInteract, IBossTrigger
{


    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] int xpGive = 100;
    [SerializeField] Renderer model;
    private NavMeshAgent agent0;
    public UnityEngine.UI.Slider healthbar;
    public TMP_Text healthText;
    private bool hasUsedAttack2;
    private bool hasUsedAttack3;
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

    [Header("Weapon")]
    // may need multiple types of bullets so make another one if need be
    [SerializeField] GameObject bullet;

    //only needed if you are using the same type of gun pivot as the enemies
    [SerializeField] Transform gunPivot;
    [SerializeField] Transform shootPos;
    [Range(1, 10)][SerializeField] int gunRotateSpeed;

    //might need multiple different shoot rates for a boss
    [Range(0.1f, 2f)][SerializeField] float shootRate;

    [Header("Flame Sweep")]
    [SerializeField] private GameObject flameSweepPrefab;
    [SerializeField] private Transform flameSweepSpawnPoint;
    [SerializeField] private int flameSweepCount = 3;
    [SerializeField] private float flameSweepSpacing = 0.4f;
    [SerializeField] private float flameSweepCooldown = 8f;

    [Header("Magma Pods")]
    [SerializeField] private GameObject magmaPodPrefab;
    [SerializeField] private Transform[] podLaunchPoints;
    [SerializeField] private int podCount = 4;
    [SerializeField] private float podLaunchInterval = 0.35f;
    [SerializeField] private float magmaPodCooldown = 12f;

    [Header("Thermal Backlash")]
    [SerializeField] private float backlashHealPercent = 0.10f;
    [SerializeField] private float backlashHealDuration = 3f;
    [SerializeField] private int backlashBaseDamage = 15;
    [SerializeField] private float backlashCooldown = 18f;
    [SerializeField] private GameObject backlashShockwavePrefab;
    [SerializeField] private TMP_Text bossWarningText;
    [SerializeField] private GameObject[] coverRocks;
    [SerializeField] private GameObject selfFireEffect;

    private bool isInBacklash;
    private int storedBacklashDamage;

    [Header("Currency")]
    [SerializeField] private int minCurrencyDrop = 10;
    [SerializeField] private int maxCurrencyDrop = 25;

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
        if (exitPortal != null)
        {
            exitPortal.SetActive(false);
        }

        currentHealth = maxHealth;
        originalColor = model.material.color;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentState = BossState.Rest;


        gameManager.instance.updateGameGoal(1);
    }
    private void Update()
    {
        playerDir = gameManager.instance.player.transform.position - transform.position;
        updateHealthBar();
        if (currentState == BossState.Dead)
            return;

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
        if (!allowedMovement || movementPos == null || movementPos.Count == 0)
        {
            return;
        }

        allowedAttack = false;

        int pathPicker = Random.Range(0, movementPos.Count);
        GameObject moveTarget = (GameObject)movementPos[pathPicker];

        Vector3 end = moveTarget.transform.position;
        end.y = 2f;

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
            transform.rotation = UnityEngine.Quaternion.RotateTowards(
                transform.rotation,
                target,
                rotateSpeed * Time.deltaTime);
            yield return null;
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
    IEnumerator Moving(UnityEngine.Vector3 startPos, UnityEngine.Vector3 endPos)
    {
        float moveTime = 2f;
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
        float moveTime = 2f;
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
    bool isAttack1 = false;

    private bool isPerformingAttack;

    private void Attack1()
    {
        Debug.Log("Boss trying Attack1");
        if (!canAttack1 || isPerformingAttack)
        {
            Debug.LogWarning("Attack1 missing flame sweep prefab or spawn point.");
            currentState = BossState.Decide;
            return;
        }

        StartCoroutine(DoFlameSweep());
    }

    private IEnumerator DoFlameSweep()
    {
        isPerformingAttack = true;
        canAttack1 = false;
        allowedMovement = false;
        allowedAttack = false;

        FacePlayer();
        yield return new WaitForSeconds(0.6f);

        for (int i = 0; i < flameSweepCount; i++)
        {
            Instantiate(flameSweepPrefab, flameSweepSpawnPoint.position, transform.rotation);
            yield return new WaitForSeconds(flameSweepSpacing);
        }

        yield return new WaitForSeconds(0.75f);

        allowedAttack = true;
        allowedMovement = true;
        currentState = BossState.Rest;
        isPerformingAttack = false;

        StartCoroutine(ResetAttack1Cooldown());
    }

    private IEnumerator ResetAttack1Cooldown()
    {
        yield return new WaitForSeconds(flameSweepCooldown);
        canAttack1 = true;
    }

    bool canAttack2 = true;
    bool isAttack2 = false;

    private void Attack2()
    {
        Debug.Log("Boss trying Attack2");
        if (!canAttack2 || isPerformingAttack)
        {
            Debug.LogWarning("Attack2 missing magma pod prefab or launch points.");
            currentState = BossState.Decide;
            return;
        }

        StartCoroutine(DoMagmaPods());
    }

    private IEnumerator DoMagmaPods()
    {
        isPerformingAttack = true;
        canAttack2 = false;
        allowedMovement = false;
        allowedAttack = false;
        hasUsedAttack2 = true;

        FacePlayer();
        yield return new WaitForSeconds(0.75f);

        if (podLaunchPoints == null || podLaunchPoints.Length == 0)
        {
            allowedAttack = true;
            allowedMovement = true;
            currentState = BossState.Rest;
            isPerformingAttack = false;
            yield break;
        }

        for (int i = 0; i < podCount; i++)
        {
            Transform launchPoint = podLaunchPoints[i % podLaunchPoints.Length];
            Instantiate(magmaPodPrefab, launchPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(podLaunchInterval);
        }

        yield return new WaitForSeconds(1f);

        allowedAttack = true;
        allowedMovement = true;
        currentState = BossState.Rest;
        isPerformingAttack = false;

        StartCoroutine(ResetAttack2Cooldown());
    }

    private IEnumerator ResetAttack2Cooldown()
    {
        yield return new WaitForSeconds(magmaPodCooldown);
        canAttack2 = true;
    }

    public void TriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInTrigger = true;
            model.material.color = Color.orange;
        }
    }


    private float decisionTimer = 0f;

    private void Decide()
    {
        if (!PlayerInTrigger)
        {
            currentState = BossState.Rest;
            return;
        }

        decisionTimer -= Time.deltaTime;

        if (decisionTimer > 0f)
            return;

        decisionTimer = Random.Range(1.5f, 3f);

        if (!canAttack1 && !canAttack2 && !canAttack3)
        {
            currentState = BossState.Rest;
            return;
        }

        if (!hasUsedAttack2 && canAttack2)
        {
            currentState = BossState.Attack2;
            return;
        }

        if (hasUsedAttack2 && !hasUsedAttack3 && canAttack3)
        {
            currentState = BossState.Attack3;
            return;
        }

        List<BossState> availableAttacks = new List<BossState>();

        if (canAttack1)
            availableAttacks.Add(BossState.Attack1);

        if (canAttack2)
            availableAttacks.Add(BossState.Attack2);

        if (hasUsedAttack2 && canAttack3)
            availableAttacks.Add(BossState.Attack3);

        if (availableAttacks.Count == 0)
        {
            currentState = BossState.Rest;
            return;
        }

        currentState = availableAttacks[Random.Range(0, availableAttacks.Count)];
    }

    bool canAttack3 = true;
    bool isAttack3 = false;

    private void Attack3()
    {
        Debug.Log("Boss trying Attack3");
        if (!canAttack3 || isPerformingAttack)
        {
            currentState = BossState.Decide;
            return;
        }

        StartCoroutine(DoThermalBacklash());
    }

    private IEnumerator DoThermalBacklash()
    {
        isPerformingAttack = true;
        canAttack3 = false;
        allowedMovement = false;
        allowedAttack = false;
        isInBacklash = true;
        storedBacklashDamage = 0;
        hasUsedAttack3 = true;

        Debug.Log("Thermal Backlash started");

        if (bossWarningText != null)
        {
            bossWarningText.gameObject.SetActive(true);
            bossWarningText.enabled = true;
            bossWarningText.text = "THERMAL BACKLASH - FIND COVER!";
        }

        if (selfFireEffect != null)
            selfFireEffect.SetActive(true);

        if (coverRocks != null)
        {
            for (int i = 0; i < coverRocks.Length; i++)
            {
                if (coverRocks[i] != null)
                    coverRocks[i].SetActive(true);
            }
        }

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
        isInBacklash = false;

        if (selfFireEffect != null)
            selfFireEffect.SetActive(false);

        if (bossWarningText != null)
            bossWarningText.text = "";

        if (backlashShockwavePrefab != null)
        {
            GameObject shockwave = Instantiate(backlashShockwavePrefab, transform.position, Quaternion.identity);
            ThermalBacklashShockwave shockwaveScript = shockwave.GetComponent<ThermalBacklashShockwave>();
            if (shockwaveScript != null)
                shockwaveScript.SetDamage(backlashBaseDamage + storedBacklashDamage);
        }

        yield return new WaitForSeconds(0.5f);

        allowedAttack = true;
        allowedMovement = true;
        currentState = BossState.Rest;
        isPerformingAttack = false;

        StartCoroutine(ResetAttack3Cooldown());
    }

    private IEnumerator ResetAttack3Cooldown()
    {
        yield return new WaitForSeconds(backlashCooldown);
        canAttack3 = true;
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
        gameManager.instance.playerDamageOut = amount;
        StartCoroutine(updateDamageText());

        if (isInBacklash)
        {
            storedBacklashDamage += amount;
            AudioManager.instance.PlaySoundAtPosition(_hit, gameObject);
            StartCoroutine(flashRed());
            return;
        }

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

            ClearAllMagmaRocks();

            if (bossWarningText != null)
                bossWarningText.text = "";

            if (selfFireEffect != null)
                selfFireEffect.SetActive(false);

            if (exitPortal != null)
                exitPortal.SetActive(true);

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

    private int GetCurrencyDrop()
    {
        return Random.Range(minCurrencyDrop, maxCurrencyDrop + 1);
    }

    private void ClearAllMagmaRocks()
    {
        MagmaRock[] rocks = FindObjectsByType<MagmaRock>();

        foreach (MagmaRock rock in rocks)
        {
            if (rock != null)
            {
                rock.DestroyByShockwave();
            }
        }
    }
}
