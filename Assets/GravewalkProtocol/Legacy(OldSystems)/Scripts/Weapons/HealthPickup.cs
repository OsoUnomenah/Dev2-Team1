using UnityEngine;

public class HealthPickup : MonoBehaviour, ISpin
{
    //GameObject player;
    [SerializeField] private GameObject healthPickup;
    [Range(0f, 1f)][SerializeField] public float healPercent = 0.25f;
    [SerializeField] private float spinSpeed = 50f;

    private void Update()
    {
        Spin(spinSpeed);
    }

    public void Spin(float _spinSpeed)
    {
         healthPickup.transform.Rotate(Vector3.up * _spinSpeed * Time.deltaTime);
    }

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