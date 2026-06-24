using UnityEngine;

public class AbilityPickup : MonoBehaviour
{

    [SerializeField] AbilityStats stats;



    private void OnTriggerEnter(Collider other)
    {
        IPickupAbilities pic = other.GetComponent<IPickupAbilities>();

        if (pic != null)
        {
            pic.getStats(stats);
            Destroy(gameObject);
        }
    }
}