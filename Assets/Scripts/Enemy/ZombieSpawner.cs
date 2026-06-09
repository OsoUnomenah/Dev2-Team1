using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;
    public Transform[] spawnPoints;

    private bool hasSpawned;

    public void SpawnZombies()
    {
        Debug.Log("SpawnZombies CALLED");
        if (hasSpawned) return;

        hasSpawned = true;

        // Use BOTH level + zone for scaling
        int difficulty = (int)gameManager.instance.level;
        int zone = gameManager.instance.runZone;

        int zombieCount = 2 + zone + Mathf.FloorToInt(difficulty / 5);

        Debug.Log("Spawning Zombies: " + zombieCount);

        for (int i = 0; i < zombieCount; i++)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(zombiePrefab, point.position, point.rotation);
        }
    }
}