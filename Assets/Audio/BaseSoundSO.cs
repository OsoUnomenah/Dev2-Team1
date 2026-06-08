using UnityEngine;

[CreateAssetMenu()]
public class BaseSoundSO : ScriptableObject
{
    public enum SoundTypes
    {
        SFX,
        Music
    }

    public SoundTypes soundType;
    public AudioClip clip;
    public bool loop = false;
    public bool randomizePitch = false;
    [Range(0.1f, 1f)] public float randomPitchRangeMod = .1f;
    [Range(0.1f, 2f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    [Range(1f, 100f)] public float fallOffDistMin = 2f;
    [Range(1f, 100f)] public float fallOffDistMax = 20f;
   
}
