using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private int baseZombieCount = 3;
    [SerializeField] private int increasePerTrigger = 2;

    private int currentZombieCount;

    private void Start()
    {
        currentZombieCount = baseZombieCount;
    }

    public void TriggerSpawn()
    {
        SpawnZombies(currentZombieCount);
        currentZombieCount += increasePerTrigger;
    }

    private void SpawnZombies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(zombiePrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}