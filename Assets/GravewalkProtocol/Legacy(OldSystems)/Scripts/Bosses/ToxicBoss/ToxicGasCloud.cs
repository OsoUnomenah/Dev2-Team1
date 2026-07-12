using System.Collections.Generic;
using UnityEngine;

public class ToxicGasCloud : MonoBehaviour
{
    [Header("Gas Damage")]

    [Tooltip("Damage dealt each time the gas damages the player.")]
    [Range(1, 100)]
    [SerializeField] private int damagePerTick = 5;

    [Tooltip("Time between each damage tick.")]
    [Range(0.1f, 5f)]
    [SerializeField] private float damageInterval = 1f;

    [Tooltip("How long the gas cloud remains active.")]
    [Range(1f, 30f)]
    [SerializeField] private float cloudDuration = 6f;

    [Header("Collision")]

    [Tooltip("Layers that the gas cloud can damage.")]
    [SerializeField] private LayerMask targetLayers;

    [Header("Gas Movement")]

    [Tooltip("How close the gas must get before it stops moving.")]
    [SerializeField] private float stoppingDistance = 0.25f;

    private Vector3 movementTarget;
    private float movementSpeed;
    private bool shouldMove;

    private readonly Dictionary<IDamage, float> nextDamageTimes =
        new Dictionary<IDamage, float>();

    private void Start()
    {
        Destroy(gameObject, cloudDuration);
    }

    private void Update()
    {
        if (!shouldMove)
        {
            return;
        }

        Vector3 targetPosition = movementTarget;
        targetPosition.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            movementSpeed * Time.deltaTime
        );

        float distanceToTarget = Vector3.Distance(
            transform.position,
            targetPosition
        );

        if (distanceToTarget <= stoppingDistance)
        {
            shouldMove = false;
        }
    }

    public void InitializeMoving(
        Vector3 targetPosition,
        float moveSpeed)
    {
        movementTarget = targetPosition;
        movementTarget.y = transform.position.y;

        movementSpeed = moveSpeed;
        shouldMove = true;
    }

    public void InitializeStationary()
    {
        shouldMove = false;
    }

    private void OnTriggerStay(Collider other)
    {
        bool isTargetLayer =
            (targetLayers.value & (1 << other.gameObject.layer)) != 0;

        if (!isTargetLayer)
        {
            return;
        }

        IDamage damageable =
            other.transform.root.GetComponentInChildren<IDamage>();

        if (damageable == null)
        {
            return;
        }

        if (nextDamageTimes.TryGetValue(
            damageable,
            out float nextAllowedDamageTime))
        {
            if (Time.time < nextAllowedDamageTime)
            {
                return;
            }
        }

        damageable.takeDamage(damagePerTick);

        nextDamageTimes[damageable] =
            Time.time + damageInterval;
    }

    private void OnTriggerExit(Collider other)
    {
        IDamage damageable =
            other.transform.root.GetComponentInChildren<IDamage>();

        if (damageable != null)
        {
            nextDamageTimes.Remove(damageable);
        }
    }
}