using UnityEngine;

public class ToxicProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float moveSpeed = 10f;

    [Tooltip("Damage dealt when the projectile hits the player.")]
    [SerializeField] private int damage = 15;

    [Tooltip("How long the projectile exists before destroying itself.")]
    [SerializeField] private float lifetime = 6f;

    [Header("Collision")]
    [Tooltip("Layers that stop the projectile, such as Player and Environment.")]
    [SerializeField] private LayerMask collisionLayers;

    private Vector3 moveDirection;
    private bool hasBeenLaunched;
    private bool hasHitSomething;
    private Transform owner;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!hasBeenLaunched || hasHitSomething)
        {
            return;
        }

        transform.position +=
            moveDirection * moveSpeed * Time.deltaTime;
    }

    public void Launch(Vector3 targetPosition, Transform projectileOwner)
    {
        owner = projectileOwner;

        moveDirection =
            (targetPosition - transform.position).normalized;

        if (moveDirection.sqrMagnitude <= 0.001f)
        {
            Destroy(gameObject);
            return;
        }

        transform.rotation = Quaternion.LookRotation(moveDirection);
        hasBeenLaunched = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (owner != null && other.transform.root == owner.root)
        {
            return;
        }

        Debug.Log(
            $"Toxic projectile touched: {other.name} | " +
            $"Layer: {LayerMask.LayerToName(other.gameObject.layer)}",
            other.gameObject
        );

        if (hasHitSomething)
        {
            return;
        }

        bool isValidLayer =
            (collisionLayers.value & (1 << other.gameObject.layer)) != 0;

        if (!isValidLayer)
        {
            Debug.LogWarning(
                $"Projectile ignored {other.name} because its layer is not selected.",
                other.gameObject
            );

            return;
        }

        IDamage damageable = other.transform.root.GetComponentInChildren<IDamage>();

        if (damageable == null)
        {
            Debug.LogWarning(
                $"Projectile hit {other.name}, but no IDamage component was found in its parents.",
                other.gameObject
            );

            return;
        }

        hasHitSomething = true;

        Debug.Log(
            $"Projectile is dealing {damage} damage to {other.name}.",
            other.gameObject
        );

        damageable.takeDamage(damage);

        Destroy(gameObject);
    }
}