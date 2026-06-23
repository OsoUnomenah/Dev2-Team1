using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    GameObject player;

    [Range(0f, 1f)]
    public float healPercent = 0.25f;

    private void OnTriggerEnter(Collider other)
    {
        StatHandler stats = other.GetComponentInChildren<StatHandler>();
        Debug.Log("Found StatHandler? " + (stats != null));
        if (stats != null)
        {
            float healAmount = stats.maxHealth * healPercent;
            stats.Heal(healAmount);

            Destroy(gameObject);
        }
    }
}