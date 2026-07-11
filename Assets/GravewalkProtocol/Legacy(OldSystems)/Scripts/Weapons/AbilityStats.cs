using UnityEngine;

[CreateAssetMenu]
public class AbilityStats : ScriptableObject
{
    public string abilityName;

    public GameObject model;
    
    [Header("Audio")]
    public BaseSoundSO throwSound;

    public enum ability { fire, freeze, magent, crystal, toxic, lightning }
    [SerializeField] public ability abilityType;


    [Range(1, 4)] public int shootDistance;
    [Range(0.1f, 50)] public float shootCooldown;
    [Range(1, 2000)] public float effectTimer;

    public ParticleSystem loopedEffect;

    [Range(1, 5)] public int level; //amount of upgrades this adds to the ability 
                                    //gives lower cool down/higher shoot dist/ etc 
}
