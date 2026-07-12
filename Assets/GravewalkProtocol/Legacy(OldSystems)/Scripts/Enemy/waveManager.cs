using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    [Header("Wave Settings")]
    public GameObject[] enemyPrefabs;
    public float timeBetweenWaves = 5f;

    [Header("Scaling")]
    public int baseEnemies = 5;
    public int enemiesPerWave = 2;
    public int enemiesPerPlayerLevel = 1;

    [Header("UI")]
    public TMP_Text waveText;

    private gameManager gm;

    [Header("Drops")]
    [SerializeField] private List<GameObject> drops;
    [Range(1, 100)][SerializeField] int dropChance;
   

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
        waveText = gameManager.instance.waveText;
        StartCoroutine(BeginGame());
    }

    IEnumerator BeginGame()
    {
        yield return new WaitForSeconds(2f); // small intro delay

        StartCoroutine(StartNextWave());
    }

    void CacheSpawnPoints()
    {
        List<Transform> points = new List<Transform>();

        foreach (Transform child in transform)
        {
            points.Add(child);
        }

        spawnPoints = points.ToArray();
    }

    private void Update()
    {
        if (spawningWave)
            return;

        if (enemiesAlive <= 0 && waveSystemStarted)
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
           // Debug.Log("Wave system activated.");
        }

        currentWave++;

        ShowWaveUI();

        yield return new WaitForSeconds(timeBetweenWaves);

        int enemyCount = CalculateEnemyCount();

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.25f);
        }

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
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points found!");
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        Instantiate(enemy, spawnPoint.position, spawnPoint.rotation);

        enemiesAlive++;
    }

    public void OnEnemyKilled()
    {
        enemiesAlive--;

        if (enemiesAlive < 0)
            enemiesAlive = 0;
    }

    void ShowWaveUI()
    {
        if (waveText == null) return;

        waveText.gameObject.SetActive(true);
        waveText.text = "WAVE " + currentWave + " INCOMING";

        StartCoroutine(HideWaveUI());
    }

    IEnumerator HideWaveUI()
    {
        yield return new WaitForSeconds(2f);

        if (waveText != null)
            waveText.gameObject.SetActive(false);
    }

    public void Drop(Transform enemyPos)
    {
        int roll = Random.Range(1, 100);
        Debug.Log("Roll " + roll);
        if (roll <= dropChance)
        {
            GameObject item = drops[Random.Range(0, drops.Count - 1)];
            Debug.Log("Dropping " + item);
            enemyPos.position -= new Vector3(0, 0.6f, 0);
            Instantiate(item, enemyPos.position, Quaternion.Euler(0, Random.Range(0, 360), 0));
        }
    }
}