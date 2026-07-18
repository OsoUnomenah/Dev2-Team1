using System.Collections;
using UnityEngine;

public class MagmaPods : MonoBehaviour, IDamage
{
    [SerializeField] private int health = 20;
    [SerializeField] private float armTime = 0.4f;
    [SerializeField] private float fuseTime = 4f;
    [SerializeField] private GameObject earlyExplosionPrefab;
    [SerializeField] private GameObject magmaRockPrefab;

    private bool stuck;
    private bool detonated;

    private void Start()
    {
        StartCoroutine(FuseRoutine());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (stuck) return;

        stuck = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        transform.SetParent(collision.transform, true);
    }

    public void takeDamage(int amount)
    {
        if (detonated) return;

        health -= amount;
        if (health <= 0)
        {
            DetonateEarly();
        }
    }

    private IEnumerator FuseRoutine()
    {
        yield return new WaitForSeconds(armTime);
        yield return new WaitForSeconds(fuseTime);

        if (!detonated)
        {
            SpawnMagmaRock();
        }
    }

    private void DetonateEarly()
    {
        detonated = true;

        if (earlyExplosionPrefab != null)
            Instantiate(earlyExplosionPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void SpawnMagmaRock()
    {
        detonated = true;

        if (magmaRockPrefab != null)
            Instantiate(magmaRockPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}