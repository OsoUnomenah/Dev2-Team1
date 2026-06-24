using UnityEngine;

public class AbilityPickup : MonoBehaviour
{

    [Header("Audio")]
    [SerializeField] private BaseSoundSO abilityPickupSound;

    [SerializeField] AbilityStats stats;



    private void OnTriggerEnter(Collider other)
    {
        IPickupAbilities pic = other.GetComponent<IPickupAbilities>();

        if (pic != null)
        {
            pic.getStats(stats);
            PlayAbilityPickupSound();
            Destroy(gameObject);
        }
    }

    private void PlayAbilityPickupSound()
    {
        if (AudioManager.instance != null && abilityPickupSound != null)
        {
            AudioManager.instance.PlaySound(abilityPickupSound);
        }
    }
}