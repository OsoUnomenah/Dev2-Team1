using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FreezeBossAI : MonoBehaviour, IDamage, IInteract, IFreeze, IBossTrigger
{
    private enum BossAttack
    {
        WallSmash,
        FreezeZones,
        FloorSpikes
    }

    [Header("Boss Animation")]
    [SerializeField] private Animator bossAnimator;

    [Header("Hit Reaction")]
    [SerializeField] private float baseHitReactionThreshold = 75f;
    [SerializeField] private float thresholdIncreasePerPlayerLevel = 8f;
    [SerializeField] private float hitReactionCooldown = 2.5f;

    private float damageSinceLastReaction;
    private float nextHitReactionTime;


    [Header("Freeze Visual")]
    [SerializeField] private FreezeVisualController freezeVisualController;

    [Header("Boss Stats")]
    [SerializeField] private int maxHealth = 300;
    [SerializeField] private int xpGive = 100;
    [SerializeField] private int minCurrencyDrop = 10;
    [SerializeField] private int maxCurrencyDrop = 25;
    [SerializeField] private Renderer model;

    [Header("UI")]
    [SerializeField] private Slider healthbar;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private GameObject onScreenDMG;
    [SerializeField] private TMP_Text damageText;

    [Header("Rewards / Progress")]
    [SerializeField] private GameObject exitPortal;

    [Header("Audio")]
    [SerializeField] private BaseSoundSO hitSound;
    [SerializeField] private BaseSoundSO deadSound;
    [SerializeField] private BaseSoundSO attackWarningSound;
    [SerializeField] private BaseSoundSO iceLaunchSound;
    [SerializeField] private BaseSoundSO spikeShatterSound;
    [SerializeField] private BaseSoundSO groundPoundSound;
    [SerializeField] private BaseSoundSO frostArcSound;
    [SerializeField] private BaseSoundSO frostPoolImpactSound;
    [SerializeField] private BaseSoundSO icicleRiseSound;
    [SerializeField] private BaseSoundSO bossFreezeSound;

    [SerializeField] private AudioClip bossMusic;

    [Header("Pattern Settings")]
    [SerializeField] private float timeBetweenAttacks = 2f;
    [SerializeField] private float warningTime = 1.5f;
    [SerializeField] private bool preventSameAttackTwice = true;

    [Header("Wall Smash Attack")]
    [SerializeField] private Transform[] spikeWalls;
    [SerializeField] private Transform leftSpikeCastOrigin;
    [SerializeField] private Transform rightSpikeCastOrigin;
    
    [SerializeField] private float firstPunchDelay = 0.35f;
    [SerializeField] private float secondPunchDelay = 0.55f;
    [SerializeField] private ParticleSystem spikeWallShatterEffectPrefab;
    [SerializeField] private float spikeHomingSpeed = 14f;
    [SerializeField] private float spikeTargetHeight = 1.2f;
    [SerializeField] private float spikeMaxFlightTime = 2f;

    [Header("Freeze Zone Attack")]
    [SerializeField] private float freezeZoneActiveTime = 4f;
    [SerializeField] private float groundPoundImpactDelay = 1.1f;
    [SerializeField] private float frostWaveTravelTime = 0.65f;
    

    [SerializeField] private Transform groundPoundImpactOrigin;
    [SerializeField] private ParticleSystem groundPoundImpactEffectPrefab;
    [SerializeField] private GameObject frostArcProjectilePrefab;
    [SerializeField] private float frostArcHeight = 4f;

    [SerializeField] private GameObject bossFrostPoolZonePrefab;

    [Header("Floor Spike Attack")]
    [SerializeField] private float spikeMoveTime = 0.5f;
    [SerializeField] private GameObject icicleTrailSegmentPrefab;
    [SerializeField] private int icicleTrailSegmentCount = 8;
    [SerializeField] private float icicleTrailSpawnInterval = 0.2f;
    [SerializeField] private float icicleTrailSegmentLifetime = 2f;
    [SerializeField] private float icicleTrailBuriedDepth = 2.5f;

    [Header("Movement / Tracking")]
    [SerializeField] private bool followPlayer = true;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stopDistance = 6f;
    [SerializeField] private float turnSpeed = 8f;
    [SerializeField] private bool stopMovingWhileAttacking = true;

    [Header("Boss Freeze Resistance")]
    [SerializeField] private float bossFreezeChance = 0.25f;
    [SerializeField] private float bossFreezeDuration = 0.75f;
    [SerializeField] private float bossFreezeCoolDown = 5f;

    private float nextBossFreezeTime;

    private bool isPerformingAttack;

    private int currentHealth;
    private bool playerInTrigger;
    private bool isDead;
    private bool isFrozen;
    private Coroutine bossLoopRoutine;
    private BossAttack lastAttack;
    private Transform player;
    private Coroutine freezeRoutine;

    private Color originalColor;

    private void Start()
    {
        currentHealth = maxHealth;

        if (bossAnimator == null)
        {
            bossAnimator = GetComponentInChildren<Animator>();
        }

        if (freezeVisualController == null)
        {
            freezeVisualController = GetComponent<FreezeVisualController>();
        }

        if (model != null)
        {
            originalColor = model.material.color;
        }

        if (exitPortal == null)
        {
            exitPortal = GameObject.FindGameObjectWithTag("Portal");
        }

        if (exitPortal != null)
        {
            exitPortal.SetActive(false);
        }

        if (gameManager.instance != null && gameManager.instance.player != null)
        {
            player = gameManager.instance.player.transform;
        }
        else
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
        }

       
        HideSpikeWalls();
        UpdateHealthBar();

        gameManager.instance.updateGameGoal(1);
    }

    private void Update()
    {
        bool shouldMove = false;

        if (isDead || isFrozen || !playerInTrigger || player == null)
        {
            if (bossAnimator != null)
            {
                bossAnimator.SetBool("IsMoving", false);
            }

            return;
        }

        FacePlayer();

        if (followPlayer)
        {
            bool movementBlocked =
                stopMovingWhileAttacking && isPerformingAttack;

            float distance =
                Vector3.Distance(transform.position, player.position);

            shouldMove =
                !movementBlocked && distance > stopDistance;

            if (shouldMove)
            {
                MoveTowardPlayer();
            }
        }

        if (bossAnimator != null)
        {
            bossAnimator.SetBool("IsMoving", shouldMove);
        }
    }

    public void TriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInTrigger = true;
        StartBossMusic();

        if (bossLoopRoutine == null)
        {
            bossLoopRoutine = StartCoroutine(BossAttackLoop());
        }
    }

    private IEnumerator BossAttackLoop()
    {
        yield return new WaitForSeconds(1f);

        while (!isDead)
        {
            if (!isFrozen && playerInTrigger)
            {
                BossAttack chosenAttack = PickRandomAttack();

                yield return StartCoroutine(AttackWarning());

                isPerformingAttack = true;

                switch (chosenAttack)
                {
                    case BossAttack.WallSmash:
                        yield return StartCoroutine(WallSmashAttack());
                        break;

                    case BossAttack.FreezeZones:
                        yield return StartCoroutine(FreezeZoneAttack());
                        break;

                    case BossAttack.FloorSpikes:
                        yield return StartCoroutine(FloorSpikeAttack());
                        break;
                }

                isPerformingAttack = false;

                lastAttack = chosenAttack;
            }

            yield return new WaitForSeconds(timeBetweenAttacks);
        }
    }

    private BossAttack PickRandomAttack()
    {
        BossAttack chosenAttack = (BossAttack)Random.Range(0, 3);

        if (preventSameAttackTwice)
        {
            int safety = 0;

            while (chosenAttack == lastAttack && safety < 10)
            {
                chosenAttack = (BossAttack)Random.Range(0, 3);
                safety++;
            }
        }

        return chosenAttack;
    }

    private IEnumerator AttackWarning()
    {
        if (attackWarningSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySoundAtPosition(attackWarningSound, gameObject);
        }

        if (model != null)
        {
            model.material.color = Color.cyan;
        }

        yield return new WaitForSeconds(warningTime);

        if (model != null)
        {
            model.material.color = originalColor;
        }
    }

    private IEnumerator WallSmashAttack()
    {
        if (spikeWalls == null
            || spikeWalls.Length < 2
            || player == null
            || leftSpikeCastOrigin == null
            || rightSpikeCastOrigin == null)
        {
            yield break;
        }

        if (bossAnimator != null)
        {
            bossAnimator.SetBool("IsMoving", false);
            bossAnimator.SetTrigger("WallSpikeAttack");
        }

        yield return new WaitForSeconds(firstPunchDelay);

        Coroutine firstWallRoutine = StartCoroutine(
            LaunchSpikeWall(
                spikeWalls[0],
                leftSpikeCastOrigin
            )
        );

        yield return new WaitForSeconds(secondPunchDelay);

        Coroutine secondWallRoutine = StartCoroutine(
            LaunchSpikeWall(
                spikeWalls[1],
                rightSpikeCastOrigin
            )
        );

        yield return firstWallRoutine;
        yield return secondWallRoutine;
    }

    private IEnumerator LaunchSpikeWall(
    Transform wall,
    Transform castOrigin
)
    {
        if (wall == null || castOrigin == null || player == null)
        {
            yield break;
        }

        wall.position = castOrigin.position;
        wall.gameObject.SetActive(true);

        if (iceLaunchSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySoundAtPosition(
                iceLaunchSound,
                wall.gameObject
            );
        }

        float flightTimer = 0f;

        while (flightTimer < spikeMaxFlightTime
               && player != null
               && !isDead)
        {
            flightTimer += Time.deltaTime;

            Vector3 targetPosition =
                player.position + Vector3.up * spikeTargetHeight;

            Vector3 direction =
                targetPosition - wall.position;

            // The spike has reached the player's body area.
            if (direction.sqrMagnitude <= 0.36f)
            {
                break;
            }

            direction.Normalize();

            // Continuously face the player's current position.
            wall.rotation = Quaternion.LookRotation(direction);

            // Continuously move toward the player's current position.
            wall.position = Vector3.MoveTowards(
                wall.position,
                targetPosition,
                spikeHomingSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (spikeShatterSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySoundAtPosition(
                spikeShatterSound,
                wall.gameObject
            );
        }

        if (spikeWallShatterEffectPrefab != null)
        {
            ParticleSystem shatterEffect = Instantiate(
                spikeWallShatterEffectPrefab,
                wall.position,
                wall.rotation
            );

            shatterEffect.Play();
        }

        wall.gameObject.SetActive(false);
        wall.position = castOrigin.position;
    }

    private IEnumerator FreezeZoneAttack()
    {
        if (player == null
            || groundPoundImpactOrigin == null
            || frostArcProjectilePrefab == null
            || bossFrostPoolZonePrefab == null)
        {
            yield break;
        }

        if (bossAnimator != null)
        {
            bossAnimator.SetBool("IsMoving", false);
            bossAnimator.SetTrigger("GroundPoundAttack");
        }

        // Wait until the boss's hands strike the ground.
        yield return new WaitForSeconds(groundPoundImpactDelay);

        if (groundPoundSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySoundAtPosition(
                groundPoundSound,
                groundPoundImpactOrigin.gameObject
            );
        }

        if (groundPoundImpactEffectPrefab != null)
        {
            ParticleSystem impactEffect = Instantiate(
                groundPoundImpactEffectPrefab,
                groundPoundImpactOrigin.position,
                groundPoundImpactOrigin.rotation
            );

            impactEffect.Play();
        }

        GameObject frostProjectile = Instantiate(
            frostArcProjectilePrefab,
            groundPoundImpactOrigin.position,
            Quaternion.identity
        );

        if (frostArcSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySoundAtPosition(
                frostArcSound,
                frostProjectile
            );
        }

        Vector3 startPosition = groundPoundImpactOrigin.position;
        Vector3 landingPosition = player.position;
        landingPosition.y = groundPoundImpactOrigin.position.y;

        float timer = 0f;

        while (timer < frostWaveTravelTime
               && player != null
               && !isDead)
        {
            timer += Time.deltaTime;

            float percent = Mathf.Clamp01(
                timer / frostWaveTravelTime
            );

            // Continue aiming toward the player's moving position.
            landingPosition = player.position;
            landingPosition.y = groundPoundImpactOrigin.position.y;

            Vector3 controlPoint =
                Vector3.Lerp(
                    startPosition,
                    landingPosition,
                    0.5f
                )
                + Vector3.up * frostArcHeight;

            float inversePercent = 1f - percent;

            frostProjectile.transform.position =
                inversePercent * inversePercent * startPosition
                + 2f * inversePercent * percent * controlPoint
                + percent * percent * landingPosition;

            yield return null;
        }

        Destroy(frostProjectile);

        GameObject frostPool = Instantiate(
            bossFrostPoolZonePrefab,
            landingPosition,
            Quaternion.Euler(
                0f,
                Random.Range(0f, 360f),
                0f
            )
        );

        if (frostPoolImpactSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySoundAtPosition(
                frostPoolImpactSound,
                frostPool
            );
        }

        yield return new WaitForSeconds(freezeZoneActiveTime);

        if (frostPool != null)
        {
            Destroy(frostPool);
        }
    }

    

    private IEnumerator FloorSpikeAttack()
    {
        if (icicleTrailSegmentPrefab == null || player == null)
        {
            yield break;
        }

        if (bossAnimator != null)
        {
            bossAnimator.SetBool("IsMoving", false);
            bossAnimator.SetTrigger("IcicleSweepAttack");
        }

        float groundHeight =
            groundPoundImpactOrigin != null
                ? groundPoundImpactOrigin.position.y
                : transform.position.y;

        Vector3 trailPosition = transform.position;
        trailPosition.y = groundHeight;

        int segmentCount = Mathf.Max(1, icicleTrailSegmentCount);

        for (int i = 0; i < segmentCount; i++)
        {
            if (player == null || isDead)
            {
                yield break;
            }

            Vector3 targetPosition = player.position;
            targetPosition.y = groundHeight;

            int remainingSegments = segmentCount - i;

            Vector3 nextPosition = Vector3.Lerp(
                trailPosition,
                targetPosition,
                1f / remainingSegments
            );

            Vector3 direction = targetPosition - trailPosition;
            direction.y = 0f;

            Quaternion segmentRotation =
                direction.sqrMagnitude > 0.01f
                    ? Quaternion.LookRotation(direction)
                    : transform.rotation;

            Vector3 buriedPosition =
                nextPosition + Vector3.down * icicleTrailBuriedDepth;

            GameObject segment = Instantiate(
                icicleTrailSegmentPrefab,
                buriedPosition,
                segmentRotation
            );

            StartCoroutine(
                RaiseIcicleTrailSegment(
                    segment,
                    nextPosition
                )
            );

            // Play on alternating segments to avoid excessive overlapping audio.
            if (i % 2 == 0
                && icicleRiseSound != null
                && AudioManager.instance != null)
            {
                AudioManager.instance.PlaySoundAtPosition(
                    icicleRiseSound,
                    segment
                );
            }

            trailPosition = nextPosition;

            yield return new WaitForSeconds(
                icicleTrailSpawnInterval
            );
        }
    }

    private IEnumerator RaiseIcicleTrailSegment(
    GameObject segment,
    Vector3 surfacePosition
)
    {
        if (segment == null)
        {
            yield break;
        }

        Collider damageCollider =
            segment.GetComponent<Collider>();

        if (damageCollider != null)
        {
            damageCollider.enabled = false;
        }

        Vector3 buriedPosition = segment.transform.position;

        yield return StartCoroutine(
            MoveTransform(
                segment.transform,
                buriedPosition,
                surfacePosition,
                spikeMoveTime
            )
        );

        if (damageCollider != null)
        {
            damageCollider.enabled = true;
        }

        yield return new WaitForSeconds(
            icicleTrailSegmentLifetime
        );

        if (segment != null)
        {
            Destroy(segment);
        }
    }

    private IEnumerator MoveTransform(Transform obj, Vector3 start, Vector3 end, float moveTime)
    {
        float timer = 0f;

        while (timer < moveTime)
        {
            timer += Time.deltaTime;
            float percent = timer / moveTime;
            percent = Mathf.SmoothStep(0f, 1f, percent);

            obj.position = Vector3.Lerp(start, end, percent);

            yield return null;
        }

        obj.position = end;
    }


    private void HideSpikeWalls()
    {
        if (spikeWalls == null)
        {
            return;
        }

        for (int i = 0; i < spikeWalls.Length; i++)
        {
            if (spikeWalls[i] != null)
            {
                spikeWalls[i].gameObject.SetActive(false);
            }
        }
    }

    public void takeDamage(int amount)
    {
        if (isDead)
        {
            return;
        }

        playerInTrigger = true;
        StartBossMusic();

        if (bossLoopRoutine == null)
        {
            bossLoopRoutine = StartCoroutine(BossAttackLoop());
        }

        gameManager.instance.playerDamageOut = amount;

        if (damageText != null)
        {
            StartCoroutine(UpdateDamageText());
        }

        currentHealth -= amount;
        damageSinceLastReaction += amount;
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            float playerLevel = 1f;

            if (gameManager.instance != null)
            {
                playerLevel = Mathf.Max(1f, gameManager.instance.level);
            }

            float currentHitReactionThreshold =
                baseHitReactionThreshold
                + ((playerLevel - 1f) * thresholdIncreasePerPlayerLevel);

            if (damageSinceLastReaction >= currentHitReactionThreshold
                && Time.time >= nextHitReactionTime
                && !isPerformingAttack
                && !isFrozen
                && bossAnimator != null)
            {
                damageSinceLastReaction = 0f;
                nextHitReactionTime = Time.time + hitReactionCooldown;

                bossAnimator.SetBool("IsMoving", false);
                bossAnimator.SetTrigger("HitReaction");
            }

            if (AudioManager.instance != null && hitSound != null)
            {
                AudioManager.instance.PlaySoundAtPosition(hitSound, gameObject);
            }

            StartCoroutine(FlashColor(Color.red, 0.1f));
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        if (BackgroundMusic.Instance != null)
        {
            BackgroundMusic.Instance.RestorePreviousMusic();
        }
        isPerformingAttack = false;

        StopAllCoroutines();
     

        if (bossAnimator != null)
        {
            bossAnimator.speed = 1f;
            bossAnimator.SetBool("IsMoving", false);
            bossAnimator.SetTrigger("Death");
        }
        Collider[] bossColliders = GetComponentsInChildren<Collider>();

        for (int i = 0; i < bossColliders.Length; i++)
        {
            bossColliders[i].enabled = false;
        }

        if (deadSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySoundAtPosition(deadSound, gameObject);
        }

        gameManager.instance.updateGameGoal(-1);
        gameManager.instance.addXp(xpGive);
        RecticleBehaviour.OffHover();

        int currencyDrop = GetCurrencyDrop();
        gameManager.instance.addCurrency(currencyDrop);

        if (exitPortal != null)
        {
            exitPortal.SetActive(true);
        }

        
    }

    private void UpdateHealthBar()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }

        if (healthbar != null)
        {
            healthbar.value = (float)currentHealth / (float)maxHealth;
        }
    }

    private IEnumerator UpdateDamageText()
    {
        damageText.text = "DMG: " + gameManager.instance.playerDamageOut;

        if (onScreenDMG != null)
        {
            onScreenDMG.SetActive(true);
        }

        yield return new WaitForSeconds(0.3f);

        if (onScreenDMG != null)
        {
            onScreenDMG.SetActive(false);
        }
    }

    private IEnumerator FlashColor(Color color, float duration)
    {
        if (model == null)
        {
            yield break;
        }

        model.material.color = color;
        yield return new WaitForSeconds(duration);
        model.material.color = originalColor;
    }

    public void freeze(float duration)
    {
        if (isDead)
        {
            return;
        }

        if(Time.time < nextBossFreezeTime)
        {
            return;
        }

        if(UnityEngine.Random.value > bossFreezeChance)
        {
            return;
        }

        nextBossFreezeTime = Time.time + bossFreezeCoolDown;

        if(freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
        }

        freezeRoutine = StartCoroutine(BossFreezeRoutine());
    }

    private IEnumerator BossFreezeRoutine()
    {
        isFrozen = true;

        if (bossFreezeSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySoundAtPosition(
                bossFreezeSound,
                gameObject
            );
        }

        if (bossAnimator != null)
        {
            bossAnimator.SetBool("IsMoving", false);
            bossAnimator.speed = 0f;
        }

        if (freezeVisualController != null)
        {
            freezeVisualController.ShowFreezeEffect();
        }

        yield return new WaitForSeconds(bossFreezeDuration);

        if (isDead)
        {
            yield break;
        }

        if (freezeVisualController != null)
        {
            freezeVisualController.HideFreezeEffect();
        }

        if (bossAnimator != null)
        {
            bossAnimator.speed = 1f;
        }

        isFrozen = false;
        freezeRoutine = null;
    }

    private IEnumerator FreezeHandler(float duration)
    {
        if (model != null)
        {
            model.material.color = Color.blue;
        }

        yield return new WaitForSeconds(duration);

        if (model != null)
        {
            model.material.color = originalColor;
        }

        isFrozen = false;
    }

    private void FacePlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }

    private void MoveTowardPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= stopDistance)
        {
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        direction.Normalize();

        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void StartBossMusic()
    {
        if (bossMusic != null && BackgroundMusic.Instance != null)
        {
            BackgroundMusic.Instance.PlayBossMusic(bossMusic);
        }
    }

    public void Interact()
    {
        // Boss does not need interaction behavior right now.
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
}