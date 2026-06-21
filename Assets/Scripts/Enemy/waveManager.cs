using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    [Header("Wave Settings")]
    public GameObject[] enemyPrefabs; // multiple enemy types
    public float timeBetweenWaves = 5f;

    [Header("Scaling")]
    public int baseEnemies = 5;
    public int enemiesPerWave = 2;
    public int enemiesPerPlayerLevel = 1;

    private gameManager gm;

    private bool waveSystemStarted = false;
    private bool spawningWave = false;

    private int currentWave = 0;
    private int enemiesAlive = 0;

    private Transform[] spawnPoints;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        gm = gameManager.instance;

        CacheSpawnPoints();
    }

    // Collect spawn points from children
    void CacheSpawnPoints()
    {
        List<Transform> points = new List<Transform>();

        foreach (Transform child in transform)
        {
            if (child.CompareTag("SpawnPoint"))
            {
                points.Add(child);
            }
        }
        spawnPoints = points.ToArray();
    }

    private void Update()
    {
        if (enemiesAlive <= 0 && !spawningWave)
        {
            StartCoroutine(StartNextWave());
        }
    }

    IEnumerator StartNextWave()
    {
        spawningWave = true;

        if (!waveSystemStarted)
        {
            waveSystemStarted = true;
            Debug.Log("Wave system activated.");
        }

        currentWave++;

        Debug.Log($"Wave {currentWave} starting in {timeBetweenWaves} seconds");

        yield return new WaitForSeconds(timeBetweenWaves);

        int enemyCount = CalculateEnemyCount();

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.25f);
        }

        Debug.Log($"Spawned {enemyCount} enemies");

        spawningWave = false;
    }

    int CalculateEnemyCount()
    {
        return baseEnemies +
               (currentWave * enemiesPerWave) +
               ((int)gm.level * enemiesPerPlayerLevel);
    }

    void SpawnEnemy()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject enemyToSpawn =
            enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        Instantiate(enemyToSpawn, spawnPoint.position, spawnPoint.rotation);

        enemiesAlive++;
    }

    public void OnEnemyKilled()
    {
        enemiesAlive--;

        if (enemiesAlive < 0)
            enemiesAlive = 0;
    }
}