using UnityEngine;

public class BossAnimationRelay : MonoBehaviour
{
    [SerializeField] private FireBossAI bossAI;

    private void Awake()
    {
        if (bossAI == null)
            bossAI = GetComponentInParent<FireBossAI>();
    }

    public void SpawnFlameSweepBurst()
    {
        if (bossAI != null)
            bossAI.SpawnFlameSweepBurst();
    }

    public void SpawnMagmaPodVolley()
    {
        if (bossAI != null)
            bossAI.SpawnMagmaPodVolley();
    }

    public void SpawnBacklashShockwave()
    {
        if (bossAI != null)
            bossAI.SpawnBacklashShockwave();
    }

    public void OnDeathAnimationFinished()
    {
        if (bossAI != null)
            bossAI.OnDeathAnimationFinished();
    }
}