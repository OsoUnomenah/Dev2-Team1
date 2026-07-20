using UnityEngine;

public interface IToxic
{
    bool IsToxic { get; }
    float ToxicDamageMultiplier { get; }

    bool TryApplyToxic();
}
