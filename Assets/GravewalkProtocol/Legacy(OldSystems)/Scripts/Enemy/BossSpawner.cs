using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    public GameObject bossZombiePrefab;
    public Transform spawnPoint;

    private bool spawned;

    public void SpawnBoss()
    {
        if (spawned)
            return;

        spawned = true;

        Instantiate(
            bossZombiePrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );
    }
}