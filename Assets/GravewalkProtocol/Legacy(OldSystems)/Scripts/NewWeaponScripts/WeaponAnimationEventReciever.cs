using UnityEngine;

public class WeaponAnimationEventReceiver : MonoBehaviour
{
    [SerializeField] private Animator weaponAnimator;

    [Header("Optional Animation Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip retrieveSound;

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
}