using UnityEngine;

public class BossHazardDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damageAmount = 20;
    [SerializeField] private float damageCooldown = 1.5f;

    private float nextDamageTime;

    private void OnTriggerEnter(Collider other)
    {
        TryDamagePlayer(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamagePlayer(other);
    }

    private void TryDamagePlayer(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (Time.time < nextDamageTime)
        {
            return;
        }

        if (gameManager.instance == null || gameManager.instance.playerStatHandler == null)
        {
            return;
        }

        nextDamageTime = Time.time + damageCooldown;
        gameManager.instance.playerStatHandler.takeDamage(damageAmount);
    }
}