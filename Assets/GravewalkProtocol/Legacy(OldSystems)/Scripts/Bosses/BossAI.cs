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

public class BossAI : MonoBehaviour, IDamage, IInteract, IFreeze, IBossTrigger
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

    [Header("Weapon")]
    // may need multiple types of bullets so make another one if need be
    [SerializeField] GameObject bullet;
    
    //only needed if you are using the same type of gun pivot as the enemies
    [SerializeField] Transform gunPivot;
    [SerializeField] Transform shootPos;
    [Range(1, 10)][SerializeField] int gunRotateSpeed;

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

        int pathPicker = Random.Range(1, 5);
        UnityEngine.Vector3 end;

        end = movementPos[pathPicker].transform.position;
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
        yield return new WaitForSeconds(16f);
        canAttack2 = true;
    }
    bool canAttack3 = true;
    IEnumerator attack3Cooldown()
    {
        allowedMovement = true;
        yield return new WaitForSeconds(18f);
        canAttack3 = true;
        
    }
    bool isAttack1 = false;
    private void Attack1()
    {
        Debug.Log("Attack 1");
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
        //attacks go here

        if (timer < 0)
        {            
            gun.SetActive(false);
            currentState = BossState.Rest;
            timer = -100;
            isAttack1 = false;
            StartCoroutine(attack1Cooldown());
        }

        
        timer--;

    }
    bool isAttack2 = false;
    private void Attack2()
    {
        Debug.Log("Attack 2");
        if (!canAttack2)
        {
            currentState = BossState.Decide;
            return;
        }
        if (!isAttack2)
        {
            timer = Random.Range(500, 1000);
            isAttack2 = true;
        }


        if (timer < 0)
        {
            gun.SetActive(false);
            currentState = BossState.Rest;
            timer = -100;
            isAttack2 = false;
            StartCoroutine(attack2Cooldown());
        }


        timer--;
    }
    bool isAttack3 = false;
    private void Attack3()
    {
        Debug.Log("Attack 3");
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


        if (timer < 0)
        {
            gun.SetActive(false);
            currentState = BossState.Rest;
            timer = -100;
            isAttack3 = false;
            StartCoroutine(attack3Cooldown());
        }


        timer--;
    }
    public void TriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
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
        else if (timer < -100)
        {
            timer = Random.Range(1, 3000); //1200
        }

        timer -= 1;

        if (timer < 0)
        {
            phasePicker = Random.Range(1, 4);// picks from a range of 1 2 or 3
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
