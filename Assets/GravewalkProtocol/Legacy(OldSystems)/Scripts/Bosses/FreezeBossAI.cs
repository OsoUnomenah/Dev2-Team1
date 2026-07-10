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

    [Header("Boss Stats")]
    [SerializeField] private int maxHealth = 300;
    [SerializeField] private int xpGive = 100;
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

    [Header("Pattern Settings")]
    [SerializeField] private float timeBetweenAttacks = 2f;
    [SerializeField] private float warningTime = 1.5f;
    [SerializeField] private bool preventSameAttackTwice = true;

    [Header("Wall Smash Attack")]
    [SerializeField] private Transform[] spikeWalls;
    [SerializeField] private float wallMoveDistance = 8f;
    [SerializeField] private float wallMoveTime = 0.75f;
    [SerializeField] private float wallHoldTime = 1f;

    [Header("Freeze Zone Attack")]
    [SerializeField] private GameObject[] freezeZones;
    [SerializeField] private Transform[] freezeZonePoints;
    [SerializeField] private int freezeZonesToUse = 2;
    [SerializeField] private float freezeZoneActiveTime = 4f;

    [Header("Floor Spike Attack")]
    [SerializeField] private Transform[] floorSpikes;
    [SerializeField] private float spikeRiseHeight = 4f;
    [SerializeField] private float spikeMoveTime = 0.5f;
    [SerializeField] private float spikeHoldTime = 1.5f;

    private int currentHealth;
    private bool playerInTrigger;
    private bool isDead;
    private bool isFrozen;
    private Coroutine bossLoopRoutine;
    private BossAttack lastAttack;
    private Transform player;

    private Color originalColor;

    private void Start()
    {
        currentHealth = maxHealth;

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

        HideFreezeZones();
        UpdateHealthBar();

        gameManager.instance.updateGameGoal(1);
    }

    public void TriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInTrigger = true;

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
        if (spikeWalls == null || spikeWalls.Length == 0 || player == null)
        {
            yield break;
        }

        Transform wall = spikeWalls[0];
        float closestDistance = Vector3.Distance(wall.position, player.position);

        for (int i = 1; i < spikeWalls.Length; i++)
        {
            float distance = Vector3.Distance(spikeWalls[i].position, player.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                wall = spikeWalls[i];
            }
        }

        Vector3 startPos = wall.position;

        Vector3 moveDirection = player.position - wall.position;
        moveDirection.y = 0f;
        moveDirection.Normalize();

        Vector3 endPos = startPos + moveDirection * wallMoveDistance;

        yield return StartCoroutine(MoveTransform(wall, startPos, endPos, wallMoveTime));

        yield return new WaitForSeconds(wallHoldTime);

        yield return StartCoroutine(MoveTransform(wall, endPos, startPos, wallMoveTime));
    }

    private IEnumerator FreezeZoneAttack()
    {
        if (freezeZones == null || freezeZones.Length == 0 || player == null)
        {
            yield break;
        }

        HideFreezeZones();

        int amount = Mathf.Clamp(freezeZonesToUse, 1, freezeZones.Length);
        Vector3 playerPos = player.position;

        for (int i = 0; i < amount; i++)
        {
            GameObject zone = freezeZones[i];

            if (zone == null)
            {
                continue;
            }

            Vector3 offset = Vector3.zero;

            if (i == 1)
            {
                offset = player.right * 4f;
            }
            else if (i == 2)
            {
                offset = -player.right * 4f;
            }

            zone.transform.position = new Vector3(
                playerPos.x + offset.x,
                zone.transform.position.y,
                playerPos.z + offset.z
            );

            zone.SetActive(true);
        }

        yield return new WaitForSeconds(freezeZoneActiveTime);

        HideFreezeZones();
    }

    private IEnumerator FloorSpikeAttack()
    {
        if (floorSpikes == null || floorSpikes.Length == 0 || player == null)
        {
            yield break;
        }

        Transform spike = floorSpikes[Random.Range(0, floorSpikes.Length)];

        Vector3 playerPos = player.position;

        Vector3 startPos = new Vector3(
            playerPos.x,
            spike.position.y,
            playerPos.z
        );

        spike.position = startPos;

        Vector3 endPos = startPos + Vector3.up * spikeRiseHeight;

        yield return StartCoroutine(MoveTransform(spike, startPos, endPos, spikeMoveTime));

        yield return new WaitForSeconds(spikeHoldTime);

        yield return StartCoroutine(MoveTransform(spike, endPos, startPos, spikeMoveTime));
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

    private void HideFreezeZones()
    {
        if (freezeZones == null)
        {
            return;
        }

        for (int i = 0; i < freezeZones.Length; i++)
        {
            if (freezeZones[i] != null)
            {
                freezeZones[i].SetActive(false);
            }
        }
    }

    public void takeDamage(int amount)
    {
        if (isDead)
        {
            return;
        }

        gameManager.instance.playerDamageOut = amount;

        if (damageText != null)
        {
            StartCoroutine(UpdateDamageText());
        }

        currentHealth -= amount;
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (AudioManager.instance != null && hitSound != null)
            {
                AudioManager.instance.PlaySoundAtPosition(hitSound, gameObject);
            }

            StartCoroutine(FlashColor(Color.red, 0.1f));
        }
    }

    private void Die()
    {
        isDead = true;

        if (deadSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySoundAtPosition(deadSound, gameObject);
        }

        gameManager.instance.updateGameGoal(-1);
        gameManager.instance.addXp(xpGive);
        RecticleBehaviour.OffHover();

        if (exitPortal != null)
        {
            exitPortal.SetActive(true);
        }

        HideFreezeZones();

        Destroy(gameObject);
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

        isFrozen = true;
        StartCoroutine(FreezeHandler(duration));
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
}