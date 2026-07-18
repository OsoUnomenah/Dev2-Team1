using System.Collections;
using UnityEngine;

public class PlayerAnimationStateController : MonoBehaviour
{
    [SerializeField] public Animator animator;

    [Header("Animation Control")]
    [Range(0f, 1f)][SerializeField] private float steveDisplaceTime = 0.1f;
    [Range(0f, 1f)][SerializeField] private float steveReturnTime = 0.15f;
    [Range(0f, 1f)][SerializeField] private float steveLeanDist = 0.01f;
    
    [SerializeField] private Transform steve;

    private PlayerWeaponManager weaponManager;

    private int currentHits;
    private bool isRestPos;
    private bool displacing;
    private WeaponData currWeapon;
    private bool isBlocking;



    // Hashes
    private readonly int pistolUp = Animator.StringToHash("pistolPickedUp");
    private readonly int rifleUp = Animator.StringToHash("riflePickedUp");
    private readonly int shotgunUp = Animator.StringToHash("shotgunPickedUp");
    private readonly int sniperUp = Animator.StringToHash("sniperPickedUp");
    private readonly int hammerUp = Animator.StringToHash("hammerPickedUp");
    private readonly int katanaUp = Animator.StringToHash("katanaPickedUp");
    private readonly int lightAttackK = Animator.StringToHash("isHittingK");
    private readonly int lightAttackHandler = Animator.StringToHash("katanaLA");
    private readonly int heavyAttackK = Animator.StringToHash("heavyHitK");
    private readonly int lightAttackH = Animator.StringToHash("isHittingH");
    private readonly int heavyAttackH = Animator.StringToHash("heavyHitH");
    private readonly int isMoving = Animator.StringToHash("isMoving");
    private readonly int jumped = Animator.StringToHash("jumped");
    private readonly int pickedUp = Animator.StringToHash("pickedUp");
    private readonly int meleeBlock = Animator.StringToHash("blocking");

    void Start()
    {
        weaponManager = gameManager.instance.playerWeaponManager;
    }


    void Update()
    {
        if (gameManager.instance == null || animator == null || weaponManager == null)
        {
            return;
        }


        HandleWeaponPickups();
        HandleLightAttackChaining();
        HandleCharacterMovement();
    }

    private void OnMeleeAttackBegin()
    {
        isRestPos = false;
        gameManager.instance.animIsMeleeing = true;
    }

    private void OnMeleeAttackEnd()
    {
        gameManager.instance.animIsMeleeing = false;
        currentHits += 1;
    }

    private void OnReturnToRestPos()
    {
        isRestPos = true;
    }

    private void OnBlock()
    {

    }

    private void HandleWeaponPickups()
    {
        if (currWeapon == null && weaponManager.CurrentWeaponData || currWeapon != weaponManager.CurrentWeaponData)
        {
            animator.SetBool(pickedUp, true);
            currWeapon = weaponManager.CurrentWeaponData;

            if (weaponManager.CurrentWeaponName == "Pistol")
            {
                animator.SetBool(pistolUp, true);
            }
            else
            {
                animator.SetBool(pistolUp, false);
            }

            if (weaponManager.CurrentWeaponName == "Rifle")
            {
                animator.SetBool(rifleUp, true);
            }
            else
            {
                animator.SetBool(rifleUp, false);
            }

            if (weaponManager.CurrentWeaponName == "Shotgun")
            {
                animator.SetBool(shotgunUp, true);
            }
            else
            {
                animator.SetBool(shotgunUp, false);
            }

            if (weaponManager.CurrentWeaponName == "Sniper")
            {
                animator.SetBool(sniperUp, true);
            }
            else
            {
                animator.SetBool(sniperUp, false);
            }

            if (weaponManager.CurrentWeaponName == "Katana")
            {
                animator.SetBool(katanaUp, true);
            }
            else
            {
                animator.SetBool(katanaUp, false);
            }

            if (weaponManager.CurrentWeaponName == "Hammer")
            {
                animator.SetBool(hammerUp, true);
            }
            else
            {
                animator.SetBool(hammerUp, false);
            }
        }
    }

    public void PlayMeleeLightAttack()
    {
        if (animator == null)
            return;

        if (currentHits > 0)
            animator.SetFloat(lightAttackHandler, 1);

        if (weaponManager.CurrentWeaponName == "Katana")
            animator.SetBool(lightAttackK, true);

        if (weaponManager.CurrentWeaponName == "Hammer")
            animator.SetBool(lightAttackH, true);
    }

    public void PlayMeleeHeavyAttack()
    {
        if (animator == null)
            return;

        if (weaponManager.CurrentWeaponName == "Katana")
            animator.SetBool(heavyAttackK, true);

        if (weaponManager.CurrentWeaponName == "Hammer")
            animator.SetBool(heavyAttackH, true);
    }

    public void PlayMeleeBlock()
    {
        if (animator == null)
            return;

        gameManager.instance.animIsBlocking = true;
        animator.SetBool(meleeBlock, true);
    }

    public void ResetMeleeAnimationTriggers()
    {
        if (animator == null)
            return;

        if (weaponManager.CurrentWeaponName == "Katana")
        {
            animator.SetBool(lightAttackK, false);
            animator.SetBool(heavyAttackK, false);
        }

        if (weaponManager.CurrentWeaponName == "Hammer")
        {
            animator.SetBool(lightAttackH, false);
            animator.SetBool(heavyAttackH, false);
        }

        gameManager.instance.isMeleeing = false;
    }

    private void HandleLightAttackChaining()
    {
        if (!weaponManager.Type)
            return; 

        if (isRestPos && currentHits > 1)
        {
            currentHits = 0;
            animator.SetFloat(lightAttackHandler, 0);
        }
    }

    private void HandleCharacterMovement()
    {

        if (gameManager.instance.playerInputHandler.currentMovement.x != 0)
        {
            animator.SetBool(isMoving, true);
        }
        else if(gameManager.instance.playerInputHandler.currentMovement.x == 0)
        {
            animator.SetBool(isMoving, false);
        }

        if (gameManager.instance.playerInputHandler.currentMovement.y > 0.2f)
        {
            animator.SetBool(jumped, true);
        }
        else if(gameManager.instance.playerInputHandler.currentMovement.y < 0.2f)
        {
            animator.SetBool(jumped, false);
        }

        if (gameManager.instance.dashTriggered && !displacing)
        {
            displacing = true;
            StartCoroutine(DashDisplacement());
        }
    }

    IEnumerator DashDisplacement()
    {
        Vector3 hipOrigPos = steve.localPosition;
        Vector3 dashDir = transform.InverseTransformDirection(gameManager.instance.playerInputHandler.currentMovement);
        Vector3 targetPos = hipOrigPos - dashDir * steveLeanDist;

        float timer = 0f;
        while (timer < steveDisplaceTime)
        {
            timer += Time.deltaTime;
            steve.localPosition = Vector3.Lerp(hipOrigPos, targetPos, timer / steveDisplaceTime);
            yield return null;
        }

        timer = 0f;
        while (timer < steveReturnTime)
        {
            timer += Time.deltaTime;
            steve.localPosition = Vector3.Lerp(targetPos, hipOrigPos, timer / steveReturnTime);
            yield return null;
        }
        
        steve.localPosition = hipOrigPos;
        displacing = false;
    }
}
