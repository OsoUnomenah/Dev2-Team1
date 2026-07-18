using System.Collections;
using UnityEngine;

public class MagmaRock : MonoBehaviour
{
    [SerializeField] private GameObject lavaPoolPrefab;
    [SerializeField] private Transform lavaSpawnPoint;
    [SerializeField] private float lavaDuration = 5f;

    private GameObject activeLavaPool;

    private void Start()
    {
        SpawnLavaPool();
    }

    private void SpawnLavaPool()
    {
        if (lavaPoolPrefab == null)
            return;

        Vector3 spawnPos = lavaSpawnPoint != null ? lavaSpawnPoint.position : transform.position;

        activeLavaPool = Instantiate(lavaPoolPrefab, spawnPos, Quaternion.identity, transform);
        StartCoroutine(RemoveLavaPoolAfterDelay());
    }

    private IEnumerator RemoveLavaPoolAfterDelay()
    {
        yield return new WaitForSeconds(lavaDuration);

        if (activeLavaPool != null)
        {
            Destroy(activeLavaPool);
        }
    }

    public void DestroyByShockwave()
    {
        Destroy(gameObject);
    }
}