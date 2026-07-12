using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class LightningBossAI : MonoBehaviour, IDamage, IInteract, IFreeze, IBossTrigger
{


    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] int xpGive = 100;
    [SerializeField] private int minCurrencyDrop = 10;
    [SerializeField] private int maxCurrencyDrop = 25;
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
    [SerializeField] BaseSoundSO _ball_lightning;

    [Header("Weapon")]
    // may need multiple types of bullets so make another one if need be
    [SerializeField] GameObject bullet;


    //only needed if you are using the same type of gun pivot as the enemies
    [SerializeField] Transform gunPivot;
    [SerializeField] Transform shootPos;
    [Range(1, 10)][SerializeField] int gunRotateSpeed;

    //might need multiple different shoot rates for a boss
    [Range(0.1f, 2f)][SerializeField] float shootRate;

    [Header("Lightning Strike Attack")]
    [SerializeField] GameObject lightningStrikes;
    [SerializeField] GameObject lightningStrikeIndicator;
    [Range(1, 100)][SerializeField] int totalStrikes;
    [Range(1, 50)][SerializeField] int strikesAttackArea;
    [SerializeField] float lightningChargeUpTime;
    [SerializeField] float lightningDestroyTime;

    private int numStrikes;
    private Vector3 posOrig;

    [Header("Dash Attack")]
    [SerializeField] int dashCount;
    [SerializeField] TrailRenderer trail;
    [Range(0.1f, 2f)][SerializeField] float dashAttackSpeed;
    [Range(0.01f, 1f)][SerializeField] float dashInterval;
    [Range(80, 500)][SerializeField] int LDRotSpeed;

    [Header("Ball Lightning")]
    [SerializeField] GameObject ballLightning;
    [SerializeField] ParticleSystem ballLightningEffect;
    [Range(1f, 20f)][SerializeField] float ballDestroyTime;


    [Header("Cooldowns")]
    [Range(1f, 20f)][SerializeField] float attack1Cd;
    [Range(1f, 20f)][SerializeField] public float attack2Cd;
    [Range(1f, 20f)][SerializeField] public float attack3Cd;

    private bool canAttack1;
    private bool canAttack2;
    private bool canAttack3;

    [Header("Don't touch unless debugging")]
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

        if (exitPortal != null)
        {
            exitPortal.SetActive(false);
        }


        currentHealth = maxHealth;

        originalColor = model.material.color;

        player = GameObject.FindGameObjectWithTag("Player").transform;

        currentState = BossState.Rest;

        gameManager.instance.updateGameGoal(1);

        timer = -100;
        phasePicker = 0;

        canAttack1 = true;
        canAttack2 = true;
        canAttack3 = true;
        ballLightningEffect.Stop();
    }

    private void Update()
    {
        if (gameManager.instance.isPaused)
        {
            return;
        }

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
            if (!isAttack2)
            {
                FacePlayer();
            }

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
        if (!isAttack2)
        {
            Movement();
        }
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

        int pathPicker = Random.Range(0, movementPos.Count - 1);
        UnityEngine.Vector3 end;

        if (movementPos[pathPicker].transform.position == transform.position)
        {
            pathPicker %= 1;
        }

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

    IEnumerator attack1Cooldown()
    {
        allowedMovement = true;
        numStrikes = 0;
        yield return new WaitForSeconds(attack1Cd);
        canAttack1 = true;
    }

    IEnumerator attack2Cooldown()
    {
        allowedMovement = true;
        yield return new WaitForSeconds(attack2Cd);
        canAttack2 = true;
    }

    IEnumerator attack3Cooldown()
    {
        allowedMovement = true;
        yield return new WaitForSeconds(attack3Cd);
        canAttack3 = true;
    }
    bool isAttack1 = false;
    private void Attack1()
    {
        if (!canAttack1)
        {
            currentState = BossState.Decide;
            return;
        }
        if (!isAttack1)
        {
            timer = Random.Range(500, 1000);
            isAttack1 = true;
        }


        Debug.Log("Attack 1");
        //attacks go here

        if (totalStrikes > numStrikes)
        {
            StartCoroutine(LightningStrikes());
        }


        if (timer < 0)
        {
            if (gun != null)
            {
                gun.SetActive(false);
            }

            currentState = BossState.Rest;
            timer = -100;
            isAttack1 = false;
            canAttack1 = false;
            StartCoroutine(attack1Cooldown());
        }


        timer--;

    }

    IEnumerator LightningStrikes()
    {
        numStrikes++;


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

    bool isAttack2 = false;
    private void Attack2()
    {
        if (!canAttack2)
        {
            currentState = BossState.Decide;
            return;
        }
        if (!isAttack2)
        {
            timer = 1500;
            isAttack2 = true;
        }
        Debug.Log("Attack 2");

        if (timer >= 1000)
        {
            StartCoroutine(LightningDash());
        }

        timer = -100;
        if (timer < 0)
        {
            if (gun != null)
            {
                gun.SetActive(false);
            }

            currentState = BossState.Rest;
            timer = -100;
            canAttack2 = false;
            StartCoroutine(attack2Cooldown());
        }


        timer--;
    }

    IEnumerator LightningDash()
    {
        trail.enabled = true;
        for (int i = 0; i < dashCount; i++)
        {
            GameObject lightningStrike = Instantiate(lightningStrikes, transform.position, Quaternion.Euler(0, Random.Range(0, 360), 0));
            AudioManager.instance.PlaySoundAtPosition(_lightning, gameObject);
            Destroy(lightningStrike, lightningDestroyTime);


            float rand = Random.Range(0, 1);
            Vector3 randomPos = new Vector3(rand, 0, rand) + player.position;
            randomPos.y = movementPos[0].transform.position.y;

            yield return StartCoroutine(MoveIndependent(randomPos, dashAttackSpeed));

            Collider[] playerHits = Physics.OverlapSphere(transform.position, 5, LayerMask.GetMask("Player"));

            if (playerHits.Length > 0)
            {
                foreach (Collider struck in playerHits)
                {
                    IDamage dmg = gameManager.instance.playerStatHandler.GetComponentInChildren<IDamage>();
                    dmg?.takeDamage(attackDamage);
                }
            }

            yield return new WaitForSeconds(dashInterval);
        }

        trail.enabled = false;

        int ranMove = Random.Range(0, movementPos.Count - 1);
        Vector3 pos = movementPos[ranMove].transform.position;

        yield return StartCoroutine(MoveIndependent(pos, 2f));

        isAttack2 = false;
    }

    IEnumerator MoveIndependent(Vector3 dest, float moveTime)
    {
        yield return StartCoroutine(TurnIndependent(dest));

        Vector3 startingPosition = transform.position;

        float time = 0f;

        while (time < moveTime)
        {
            time += Time.deltaTime;

            transform.position = Vector3.Lerp(startingPosition, dest, time / moveTime);

            yield return null;
        }

        if (transform.position != dest)
        {
            transform.position = dest;
        }
    }

    IEnumerator TurnIndependent(Vector3 end)
    {
        Vector3 dir = end - transform.position;
        Quaternion target;
        target = Quaternion.LookRotation(dir);

        while (Quaternion.Angle(transform.rotation, target) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                target,
                LDRotSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
    }

    bool isAttack3 = false;
    private void Attack3()
    {
        if (!canAttack3)
        {
            currentState = BossState.Decide;
            return;
        }
        if (!isAttack3)
        {
            timer = Random.Range(500, 1000);
            isAttack3 = true;
        }
        Debug.Log("Attack 3");

        StartCoroutine(BallLightning());

        timer = -1;

        if (timer < 0)
        {
            if (gun != null)
            {
                gun.SetActive(false);
            }

            currentState = BossState.Rest;
            timer = -100;
            isAttack3 = false;
            canAttack3 = false;
            StartCoroutine(attack3Cooldown());
        }


        timer--;
    }

    IEnumerator BallLightning()
    {
        ballLightningEffect.Play();
        yield return new WaitForSeconds(2f);
        ballLightningEffect.Stop();

        if (ballLightning != null)
        {
            Vector3 randPos = transform.position + Random.insideUnitSphere * 10;
            randPos.y = transform.position.y;

            GameObject ball = Instantiate(ballLightning, randPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
            AudioManager.instance.PlaySoundFollowPosition(_ball_lightning, ball, ballDestroyTime);
            Destroy(ball, ballDestroyTime);
        }
    }

    public void TriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInTrigger = true;
            // model.material.color = Color.orange;
        }
    }


    private void Decide()
    {
        if (!PlayerInTrigger)
        {
            Rest();
        }
        else if (timer < -100)
        {
            timer = Random.Range(1, 1500); //1200
        }

        timer -= 1;

        if (timer < -100 && allowedAttack)
        {
            phasePicker = Random.Range(1, 4);

            if (phasePicker == 1)
            {
                currentState = BossState.Attack1;
            }
            else if (phasePicker == 2)
            {
                currentState = BossState.Attack2;
            }
            else if (phasePicker == 3)
            {
                currentState = BossState.Attack3;
            }
        }

    }

    public void updateHealthBar()
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

    IEnumerator updateDamageText()
    {
        damageText.text = ("DMG: " + gameManager.instance.playerDamageOut.ToString());

        onScreenDMG.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        onScreenDMG.SetActive(false);
    }

    public void takeDamage(int amount)
    {
        Debug.Log("taking damage");
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