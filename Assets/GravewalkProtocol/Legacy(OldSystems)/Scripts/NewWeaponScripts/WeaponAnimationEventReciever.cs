using UnityEngine;

public class WeaponAnimationEventReceiver : MonoBehaviour
{
    [SerializeField] private Animator weaponAnimator;

    [Header("Optional Animation Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip retrieveSound;


    [SerializeField] private PlayerInputHandler playerInputHandler;

    private void Awake()
    {
        if (weaponAnimator == null)
        {
            weaponAnimator = GetComponent<Animator>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (playerInputHandler == null)
        {
            playerInputHandler = FindAnyObjectByType<PlayerInputHandler>();
        }
    }

    public void DisableSelectAnim()
    {
        ReturnToIdle();
    }

    public void PlayRetrieveSound()
    {
        if (audioSource != null && retrieveSound != null)
        {
            audioSource.PlayOneShot(retrieveSound);
        }
    }

    public void AltFireToIdle()
    {
        ReturnToIdle();
    }

    public void DryFireToIdle()
    {
        ReturnToIdle();
    }

    // Ammo is already subtracted by PlayerInputHandler.
    public void SingleFireAmmoCounter()
    {
    }

    // We will connect the project's muzzle flash and effects later.
    public void AddSingleFireEffects()
    {
    }

    // PlayerInputHandler already plays WeaponData.ShootSound.
    public void PlayFireSound()
    {
    }

    private void ReturnToIdle()
    {
        if (weaponAnimator == null)
        {
            return;
        }

        weaponAnimator.CrossFadeInFixedTime(
            "Idle",
            0.05f,
            0
        );
    }

    // The project already plays the reload sound through WeaponData.
    public void PlayReloadPart1Sound()
    {
    }

    // Reserved for a second mechanical reload sound later.
    public void PlayReloadPart2Sound()
    {
    }

    // Ammo is currently refilled by PlayerInputHandler when the reload timer ends.
    public void AddAmmo()
    {
    }

    public void ReloadToIdle()
    {
        ReturnToIdle();
    }

    // Ammo is already consumed by PlayerInputHandler.
    public void AutoFireAmmoCounter()
    {
    }

    public void PlayReloadLoop()
    {
        if (weaponAnimator != null)
        {
            weaponAnimator.CrossFadeInFixedTime(
                "ReloadLoop",
                0.05f,
                0
            );
        }
    }

    // Ammo is currently completed by PlayerInputHandler's reload timer.
    // This event is kept empty to prevent duplicate ammo changes.
    public void AddShotgunAmmo()
    {
        if (playerInputHandler == null)
        {
            playerInputHandler = FindAnyObjectByType<PlayerInputHandler>();
        }

        if (playerInputHandler == null || weaponAnimator == null)
        {
            return;
        }

        bool needsAnotherShell =
            playerInputHandler.AddShotgunShellFromAnimation();

        if (!needsAnotherShell)
        {
            weaponAnimator.CrossFadeInFixedTime(
                "EndReload",
                0.05f,
                0,
                0f
            );
        }
    }

    // The project's reload audio already plays through WeaponData.
    // We can assign a separate cocking sound later.
    public void PlayCockSound()
    {
    }

    public void EndReloadToIdle()
    {
        ReturnToIdle();
    }

    // Scope visibility is controlled by the project's ADS system.
    public void HideScope()
    {
        if (ChargeSniperScopeUI.Instance != null)
        {
            ChargeSniperScopeUI.Instance.Hide();
        }
    }

    public void ShowScope()
    {
        if (ChargeSniperScopeUI.Instance != null)
        {
            ChargeSniperScopeUI.Instance.Show();
        }
    }
}