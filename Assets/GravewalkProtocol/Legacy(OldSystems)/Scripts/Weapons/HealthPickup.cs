using UnityEngine;

public class HealthPickup : MonoBehaviour, ISpin
{
    //GameObject player;
    [SerializeField] private GameObject healthPickup;
    [Range(0f, 1f)][SerializeField] public float healPercent = 0.25f;
    [SerializeField] private float spinSpeed = 50f;
    [SerializeField] private BaseSoundSO _healthPickup;

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
            AudioManager.instance.PlaySoundFromSource(_healthPickup, gameManager.instance.player);

            float healAmount = stats.maxHealth * healPercent;
            stats.Heal(healAmount);

            Destroy(gameObject);
        }
    }
}