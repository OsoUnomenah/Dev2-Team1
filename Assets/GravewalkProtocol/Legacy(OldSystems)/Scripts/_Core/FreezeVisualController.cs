using UnityEngine;

public class FreezeVisualController : MonoBehaviour
{
    [Header("Freeze Visuals")]
    [SerializeField] private GameObject normalIcePrisonPrefab;
    [SerializeField] private GameObject bossIceEffectPrefab;
    [SerializeField] private Transform effectAnchor;
    [SerializeField] private bool useBossEffect;

    [Header("Shatter Effect")]
    [SerializeField] private ParticleSystem shatterEffectPrefab;

    [Header("Placement")]
    [SerializeField] private Vector3 localPositionOffset;
    [SerializeField] private Vector3 localRotationOffset;
    [SerializeField] private Vector3 localScale = Vector3.one;

    private GameObject activeFreezeVisual;

    public void ShowFreezeEffect()
    {
        HideFreezeEffect();

        GameObject selectedPrefab = useBossEffect
            ? bossIceEffectPrefab
            : normalIcePrisonPrefab;

        if (selectedPrefab == null)
        {
            return;
        }

        Transform parent = effectAnchor != null
            ? effectAnchor
            : transform;

        activeFreezeVisual = Instantiate(
            selectedPrefab,
            parent
        );

        activeFreezeVisual.transform.localPosition =
            localPositionOffset;

        activeFreezeVisual.transform.localRotation =
            Quaternion.Euler(localRotationOffset);

        activeFreezeVisual.transform.localScale =
            localScale;
    }

    public void HideFreezeEffect()
    {
        if (activeFreezeVisual == null)
        {
            return;
        }

        Destroy(activeFreezeVisual);
        activeFreezeVisual = null;
    }

    public void PlayShatterEffect()
    {
        if (shatterEffectPrefab != null)
        {
            Transform spawnPoint = effectAnchor != null
                ? effectAnchor
                : transform;

            ParticleSystem shatterEffect = Instantiate(
                shatterEffectPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            shatterEffect.Play();
        }

        HideFreezeEffect();
    }

    private void OnDisable()
    {
        HideFreezeEffect();
    }
}
