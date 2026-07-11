using System.Collections;
using UnityEngine;

public class FreezeZoneHazard : MonoBehaviour
{
    [Header("Freeze")]
    [SerializeField] private float freezeDuration = 2f;
    [SerializeField] private bool freezeOnlyOncePerActivation = true;

    [Header("Damage Over Time")]
    [SerializeField] private int damagePerTick = 3;
    [SerializeField] private float tickRate = 0.5f;

    private bool hasFrozen;
    private Coroutine damageRoutine;

    private void OnEnable()
    {
        hasFrozen = false;
        damageRoutine = null;
    }

    private void OnDisable()
    {
        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
            damageRoutine = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryFreezePlayer(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryFreezePlayer(other);
    }

    private void TryFreezePlayer(Collider other)
    {
        if (freezeOnlyOncePerActivation && hasFrozen)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (gameManager.instance == null || gameManager.instance.playerInputHandler == null)
        {
            return;
        }

        gameManager.instance.playerInputHandler.FreezePlayer(freezeDuration);

        if (damageRoutine == null)
        {
            damageRoutine = StartCoroutine(FreezeDamageRoutine());
        }

        hasFrozen = true;
    }

    private IEnumerator FreezeDamageRoutine()
    {
        float timer = 0f;

        while (timer < freezeDuration)
        {
            if (gameManager.instance != null && gameManager.instance.playerStatHandler != null)
            {
                gameManager.instance.playerStatHandler.takeDamage(damagePerTick);
            }

            timer += tickRate;
            yield return new WaitForSeconds(tickRate);
        }

        damageRoutine = null;
    }
}