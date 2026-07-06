using UnityEngine;

public class ZombieTrigger : MonoBehaviour
{
    public ZombieSpawner zombieSpawner;
    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            zombieSpawner.TriggerSpawn();
        }
    }
}
