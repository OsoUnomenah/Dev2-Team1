using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class FireBossAI : MonoBehaviour, IDamage, IInteract, IFreeze, IBossTrigger
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private int xpGive = 100;
    [SerializeField] private Renderer model;
    [SerializeField] private int rotateSpeed = 180;
    [SerializeField] private int jumpHeight = 3;

    [Header("UI")]
    public UnityEngine.UI.Slider healthbar;
    public TMP_Text healthText;
    public GameObject onScreenDMG;
    public TMP_Text damageText;

    [Header("Movement")]
    [SerializeField] private List<GameObject> movementPos;
    [SerializeField] private float sightRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float hearingRange;
    [SerializeField] private float wanderRadius;
    [SerializeField] private float wanderTimer;

    [Header("Audio")]
    [SerializeField] private BaseSoundSO _hit;
    [SerializeField] private BaseSoundSO _dead;

    [Header("Weapon")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform gunPivot;
    [SerializeField] private Transform shootPos;
    [Range(1, 10)][SerializeField] private int gunRotateSpeed = 5;
    [Range(0.1f, 2f)][SerializeField] private float shootRate = 1f;
    [SerializeField] private GameObject gun;

    [Header("Animation")]
    [SerializeField] private Animator bossAnimator;
    [SerializeField] private string idleState = "Idle1";
    [SerializeField] private string flameSweepState = "attack1";
    [SerializeField] private string magmaPodsState = "attack2";
    [SerializeField] private string backlashState = "Rage";
    [SerializeField] private string deathState = "Death1";

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

    [Header("Currency")]
    [SerializeField] private int minCurrencyDrop = 10;
    [SerializeField] private int maxCurrencyDrop = 25;

    [Header("Debug")]
    [SerializeField] private List<int> Modifiers;

    [Header("Scene")]
    [SerializeField] public GameObject exitPortal;

    private NavMeshAgent agent0;
    private Transform player;
    private int currentHealth;
    private Color originalColor;

    private bool PlayerInTrigger;
    private bool allowedMovement = false;
    private bool allowedAttack = true;
    private bool isPerformingAttack;
    private bool isInBacklash;
    private bool isFroze;
    private bool heardNoise;

    private bool canAttack1 = true;
    private bool canAttack2 = true;
    private bool canAttack3 = true;

    private bool hasUsedAttack2;
    private bool hasUsedAttack3;

    private int storedBacklashDamage;
    private float decisionTimer = 0f;
    private Vector3 playerDir;
    private Vector3 lastHeardPosition;

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

    private void Start()
    {
        agent0 = GetComponent<NavMeshAgent>();

        exitPortal = GameObject.FindGameObjectWithTag("Portal");
        if (exitPortal != null)
            exitPortal.SetActive(false);

        currentHealth = maxHealth;

        if (model != null)
            originalColor = model.material.color;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        currentState = BossState.Rest;
        PlayAnim(idleState);

        if (gameManager.instance != null)
            gameManager.instance.updateGameGoal(1);
    }

    private void Update()
    {
        if (currentState == BossState.Dead)
            return;

        if (gameManager.instance != null && gameManager.instance.player != null)
            playerDir = gameManager.instance.player.transform.position - transform.position;

        updateHealthBar();

        if (player == null || isFroze)
        {
            currentState = BossState.Rest;
            return;
        }

        if (allowedAttack)
        {
            FacePlayer();

            switch (currentState)
            {
                case BossState.Rest:
                    Rest();
                    break;

                case BossState.Decide:
                    Decide();
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
            }
        }

        Movement();
    }

    private void PlayAnim(string stateName)
    {
        if (bossAnimator != null && !string.IsNullOrEmpty(stateName))
            bossAnimator.CrossFade(stateName, 0.1f);
    }

    private void FacePlayer()
    {
        if (player == null)
            return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotateSpeed * Time.deltaTime);
    }

    private void Movement()
    {
        if (!allowedMovement || movementPos == null || movementPos.Count == 0 || isPerformingAttack)
            return;

        allowedAttack = false;

        int pathPicker = Random.Range(0, movementPos.Count);
        GameObject moveTarget = movementPos[pathPicker];

        if (moveTarget == null)
        {
            allowedAttack = true;
            allowedMovement = false;
            return;
        }

        Vector3 end = moveTarget.transform.position;
        end.y = transform.position.y;

        int willJump = Random.Range(1, 3);

        StartCoroutine(Turn(end, willJump));
        allowedMovement = false;
    }

    private IEnumerator Turn(Vector3 end, int willJump)
    {
        Vector3 dir = end - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion target = Quaternion.LookRotation(dir);

            while (Quaternion.Angle(transform.rotation, target) > 1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotateSpeed * Time.deltaTime);
                yield return null;
            }
        }

        switch (willJump)
        {
            case 1:
                StartCoroutine(Moving(transform.position, end));
                break;

            case 2:
                StartCoroutine(Jumping(transform.position, end));
                break;
        }
    }

    private IEnumerator Moving(Vector3 startPos, Vector3 endPos)
    {
        float moveTime = 2f;
        float time = 0f;

        while (time < moveTime)
        {
            time += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, time / moveTime);
            yield return null;
        }

        transform.position = endPos;
        allowedAttack = true;
        PlayAnim(idleState);
    }

    private IEnumerator Jumping(Vector3 startPos, Vector3 endPos)
    {
        float moveTime = 2f;
        float time = 0f;

        while (time < moveTime)
        {
            time += Time.deltaTime;

            Vector3 position = Vector3.Lerp(startPos, endPos, time / moveTime);
            position.y += Mathf.Sin(time / moveTime * Mathf.PI) * jumpHeight;
            transform.position = position;

            yield return null;
        }

        transform.position = endPos;
        allowedAttack = true;
        PlayAnim(idleState);
    }

    private void Rest()
    {
        if (PlayerInTrigger)
            currentState = BossState.Decide;
    }

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

    private void Attack1()
    {
        Debug.Log("Boss trying Attack1");

        if (flameSweepPrefab == null || flameSweepSpawnPoint == null)
        {
            Debug.LogWarning("Attack1 missing flame sweep prefab or spawn point.");
            currentState = BossState.Decide;
            return;
        }

        if (!canAttack1 || isPerformingAttack)
        {
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
        PlayAnim(flameSweepState);
        yield return new WaitForSeconds(0.6f);

        for (int i = 0; i < flameSweepCount; i++)
        {
            Instantiate(flameSweepPrefab, flameSweepSpawnPoint.position, transform.rotation);
            yield return new WaitForSeconds(flameSweepSpacing);
        }

        PlayAnim(idleState);
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

    private void Attack2()
    {
        Debug.Log("Boss trying Attack2");

        if (magmaPodPrefab == null || podLaunchPoints == null || podLaunchPoints.Length == 0)
        {
            Debug.LogWarning("Attack2 missing magma pod prefab or launch points.");
            currentState = BossState.Decide;
            return;
        }

        if (!canAttack2 || isPerformingAttack)
        {
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
        PlayAnim(magmaPodsState);
        yield return new WaitForSeconds(0.75f);

        for (int i = 0; i < podCount; i++)
        {
            Transform launchPoint = podLaunchPoints[i % podLaunchPoints.Length];
            if (launchPoint != null)
                Instantiate(magmaPodPrefab, launchPoint.position, Quaternion.identity);

            yield return new WaitForSeconds(podLaunchInterval);
        }

        PlayAnim(idleState);
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

    private void Attack3()
    {
        Debug.Log("Boss trying Attack3");

        if (backlashShockwavePrefab == null)
            Debug.LogWarning("Attack3 missing backlash shockwave prefab.");

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

        PlayAnim(backlashState);

        if (bossWarningText != null)
            bossWarningText.text = "THERMAL BACKLASH - FIND COVER!";

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

        PlayAnim(idleState);
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

    public void TriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerInTrigger = true;

        if (model != null)
            model.material.color = Color.orange;
    }

    public void updateHealthBar()
    {
        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;

        if (healthbar != null)
            healthbar.value = (float)currentHealth / maxHealth;
    }

    private IEnumerator updateDamageText()
    {
        if (damageText != null && gameManager.instance != null)
            damageText.text = "DMG: " + gameManager.instance.playerDamageOut;

        if (onScreenDMG != null)
        {
            onScreenDMG.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            onScreenDMG.SetActive(false);
        }
    }

    public void takeDamage(int amount)
    {
        if (currentState == BossState.Dead)
            return;

        if (gameManager.instance != null)
        {
            gameManager.instance.playerDamageOut = amount;
            StartCoroutine(updateDamageText());
        }

        if (isInBacklash)
        {
            storedBacklashDamage += amount;

            if (AudioManager.instance != null)
                AudioManager.instance.PlaySoundAtPosition(_hit, gameObject);

            StartCoroutine(flashRed());
            return;
        }

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            currentState = BossState.Dead;
            allowedAttack = false;
            allowedMovement = false;
            isPerformingAttack = false;
            isInBacklash = false;

            if (agent0 != null)
                agent0.isStopped = true;

            PlayAnim(deathState);

            if (AudioManager.instance != null)
                AudioManager.instance.PlaySoundAtPosition(_dead, gameObject);

            if (gameManager.instance != null)
            {
                gameManager.instance.updateGameGoal(-1);
                gameManager.instance.addXp(xpGive);

                int currencyDrop = GetCurrencyDrop();
                gameManager.instance.addCurrency(currencyDrop);
            }

            RecticleBehaviour.OffHover();
            ClearAllMagmaRocks();

            if (bossWarningText != null)
                bossWarningText.text = "";

            if (selfFireEffect != null)
                selfFireEffect.SetActive(false);

            if (exitPortal != null)
                exitPortal.SetActive(true);

            StartCoroutine(DestroyAfterDeath());
        }
        else
        {
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySoundAtPosition(_hit, gameObject);

            StartCoroutine(flashRed());
        }
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    private IEnumerator flashRed()
    {
        if (model == null)
            yield break;

        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = originalColor;
    }

    private IEnumerator flashGreen()
    {
        if (model == null)
            yield break;

        model.material.color = Color.green;
        yield return new WaitForSeconds(0.3f);
        model.material.color = originalColor;
    }

    public void Interact()
    {
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
        isFroze = true;
        StartCoroutine(freezeHandler(duration));
    }

    private IEnumerator freezeHandler(float duration)
    {
        if (model != null)
            model.material.color = Color.blue;

        yield return new WaitForSeconds(duration);

        if (model != null)
            model.material.color = originalColor;

        isFroze = false;
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
                rock.DestroyByShockwave();
        }
    }
}