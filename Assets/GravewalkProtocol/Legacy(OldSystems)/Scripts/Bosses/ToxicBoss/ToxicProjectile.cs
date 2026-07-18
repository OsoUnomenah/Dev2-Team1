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
        Destroy(transform.root.gameObject, lifetime);
    }

    private void Update()
    {
        if (!hasBeenLaunched || hasHitSomething)
        {
            return;
        }

        transform.root.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    public void Launch(
     Vector3 targetPosition,
     Transform projectileOwner)
    {
        owner = projectileOwner;

        Transform projectileRoot = transform.root;

        moveDirection = (targetPosition - projectileRoot.position).normalized;

        if (moveDirection.sqrMagnitude <= 0.001f)
        {
            Destroy(projectileRoot.gameObject);
            return;
        }

        projectileRoot.rotation = Quaternion.LookRotation(moveDirection);

        hasBeenLaunched = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (owner != null && other.transform.root == owner.root)
        {
            return;
        }

        if (hasHitSomething)
        {
            return;
        }

        bool isValidLayer = (collisionLayers.value & (1 << other.gameObject.layer)) != 0;

        if (!isValidLayer)
        {
            return;
        }

        IDamage damageable = other.transform.root.GetComponentInChildren<IDamage>();

        if (damageable == null)
        {
            return;
        }

        hasHitSomething = true;

        damageable.takeDamage(damage);

        Destroy(transform.root.gameObject);
    }
}