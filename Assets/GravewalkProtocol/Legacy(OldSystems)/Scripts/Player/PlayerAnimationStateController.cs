using UnityEngine;

public class PlayerAnimationStateController : MonoBehaviour
{
    [SerializeField] public Animator animator;

    [Header("Animation Control")]
    

    private PlayerWeaponManager weaponManager;

    private int currentHits;
    private float meleeTimer;
    private bool isRestPos;

    // Hashes
    private readonly int recoil = Animator.StringToHash("applyRecoil");
    private readonly int pistolUp = Animator.StringToHash("pistolPickedUp");
    private readonly int rifleUp = Animator.StringToHash("riflePickedUp");
    private readonly int shotgunUp = Animator.StringToHash("shotgunPickedUp");
    private readonly int sniperUp = Animator.StringToHash("sniperPickedUp");
    private readonly int hammerUp = Animator.StringToHash("hammerPickedUp");
    private readonly int katanaUp = Animator.StringToHash("katanaPickedUp");
    private readonly int lightAttackK = Animator.StringToHash("isHittingK");
    private readonly int lightAttackHandler = Animator.StringToHash("katanaLA");
    private readonly int heavyAttackK = Animator.StringToHash("heavyHitK");

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


        HandleRecoil();
        HandleWeaponPickups();
        HandleLightAttackChaining();
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

    private void HandleRecoil()
    {
        if (gameManager.instance.isShooting)
        {
            animator.SetBool(recoil, true);
        }
        else if(gameManager.instance.canShoot)
        {
            animator.SetBool(recoil, false);
        }
    }

    private void HandleWeaponPickups()
    {
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

    public void PlayMeleeLightAttack()
    {
        if (animator == null)
            return;

        if (currentHits > 0)
            animator.SetFloat(lightAttackHandler, 1);

        animator.SetBool(lightAttackK, true);
    }

    public void PlayMeleeHeavyAttack()
    {
        if (animator == null)
            return;

     //  animator.SetBool(heavyAttack, true);
    }

    public void PlayMeleeBlock()
    {
        if (animator == null)
            return;

     //   animator.SetBool(meleeBlock, true);
    }

    public void ResetMeleeAnimationTriggers()
    {
        if (animator == null)
            return;

        animator.SetBool(lightAttackK, false);
        //animator.SetBool(heavyAttack, false);
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
}
