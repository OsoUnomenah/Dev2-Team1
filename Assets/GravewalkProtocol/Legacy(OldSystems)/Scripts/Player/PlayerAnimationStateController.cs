using UnityEngine;

public class PlayerAnimationStateController : MonoBehaviour
{
    [SerializeField] Animator animator;

    private PlayerWeaponManager weaponManager;

    // Hashes
    private readonly int recoil = Animator.StringToHash("applyRecoil");
    private readonly int pistolUp = Animator.StringToHash("pistolPickedUp");
    private readonly int rifleUp = Animator.StringToHash("riflePickedUp");
    private readonly int shotgunUp = Animator.StringToHash("shotgunPickedUp");
    private readonly int sniperUp = Animator.StringToHash("sniperPickedUp");
    private readonly int hammerUp = Animator.StringToHash("hammerPickedUp");
    private readonly int katanaUp = Animator.StringToHash("katanaPickedUp");

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
    }

    private void OnMeleeAttackBegin()
    {
        gameManager.instance.animIsMeleeing = true;
    }

    private void OnMeleeAttackEnd()
    {
        gameManager.instance.animIsMeleeing = false;
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
}
