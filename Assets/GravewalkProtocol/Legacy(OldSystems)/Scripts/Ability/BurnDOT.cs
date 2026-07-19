using System.Collections;
using UnityEngine;

public class BurnDOT : MonoBehaviour
{
    [Header("Optional Visuals")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color burnTint = new Color(1f, 0.45f, 0.1f);
    [SerializeField] private bool tintWhileBurning = false;
    [SerializeField] private Transform effectAnchor;
    [SerializeField] private Vector3 effectLocalOffset = new Vector3(0f, 1f, 0f);

    [Header("Immunity")]
    [SerializeField] private bool fireImmune = false;

    private Coroutine burnRoutine;
    private IDamage damageTarget;
    private ParticleSystem activeBurnEffect;
    private Color originalColor;
    private bool hasOriginalColor;

    private void Awake()
    {
        damageTarget = GetComponent<IDamage>();

        if (damageTarget == null)
        {
            damageTarget = GetComponentInParent<IDamage>();
        }

        if (damageTarget == null)
        {
            damageTarget = GetComponentInChildren<IDamage>();
        }

        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (effectAnchor == null)
        {
            effectAnchor = transform;
        }

        if (targetRenderer != null)
        {
            originalColor = targetRenderer.material.color;
            hasOriginalColor = true;
        }
    }

    public bool IsFireImmune()
    {
        return fireImmune;
    }

    public void SetFireImmune(bool value)
    {
        fireImmune = value;
    }

    public void ApplyBurn(
        int ticks = 3,
        float tickInterval = 1f,
        int minDamage = 1,
        int maxDamage = 5,
        ParticleSystem burnEffectPrefab = null)
    {
        if (fireImmune)
        {
            Debug.Log("BURN: " + gameObject.name + " is fire immune");
            return;
        }

        if (damageTarget == null)
        {
            Debug.LogWarning("BURN: no IDamage found on " + gameObject.name);
            return;
        }

        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
        }

        StartBurnVisual(burnEffectPrefab);
        burnRoutine = StartCoroutine(BurnRoutine(ticks, tickInterval, minDamage, maxDamage));
    }

    private IEnumerator BurnRoutine(int ticks, float tickInterval, int minDamage, int maxDamage)
    {
        for (int i = 0; i < ticks; i++)
        {
            int burnDamage = Random.Range(minDamage, maxDamage + 1);

            if (damageTarget != null)
            {
                damageTarget.takeDamage(burnDamage);
            }

            yield return new WaitForSeconds(tickInterval);
        }

        StopBurnVisual();
        burnRoutine = null;
    }

    private void StartBurnVisual(ParticleSystem burnEffectPrefab)
    {
        if (tintWhileBurning && targetRenderer != null)
        {
            targetRenderer.material.color = burnTint;
        }

        if (burnEffectPrefab == null)
        {
            Debug.LogWarning("BURN VFX: burnEffectPrefab is NULL on " + gameObject.name);
            return;
        }

        if (activeBurnEffect != null)
        {
            Destroy(activeBurnEffect.gameObject);
        }

        Transform anchor = effectAnchor != null ? effectAnchor : transform;

        activeBurnEffect = Instantiate(
            burnEffectPrefab,
            anchor.position,
            Quaternion.identity,
            anchor
        );

        activeBurnEffect.transform.localPosition = effectLocalOffset;
        activeBurnEffect.transform.localRotation = Quaternion.identity;
        activeBurnEffect.transform.localScale = Vector3.one;

        Debug.Log("BURN VFX: spawned " + activeBurnEffect.name + " on " + gameObject.name);

        activeBurnEffect.Play(true);
    }

    private void StopBurnVisual()
    {
        if (tintWhileBurning && targetRenderer != null && hasOriginalColor)
        {
            targetRenderer.material.color = originalColor;
        }

        if (activeBurnEffect != null)
        {
            activeBurnEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Destroy(activeBurnEffect.gameObject, 1f);
            activeBurnEffect = null;
        }
    }

    private void OnDisable()
    {
        StopBurnVisual();
    }

    private void OnDestroy()
    {
        StopBurnVisual();
    }
}