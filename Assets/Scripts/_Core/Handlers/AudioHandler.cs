using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Mixer Routing")]
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup musicGroup;

    private void Awake()
    {
        instance = this;
    }

    public void PlaySound(BaseSoundSO sound)
    {
        GameObject soundObject = new GameObject("Temp Audio");
        AudioSource audioSource = soundObject.GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = soundObject.AddComponent<AudioSource>();
        }

        // Route to correct mixer group based on sound type
        audioSource.outputAudioMixerGroup =
            sound.soundType == BaseSoundSO.SoundTypes.Music ? musicGroup : sfxGroup;

        audioSource.clip = sound.clip;
        audioSource.volume = sound.volume;
        audioSource.pitch = sound.pitch;
        audioSource.loop = sound.loop;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        if (!sound.loop)
        {
            Destroy(soundObject, sound.clip.length);
        }
    }

    public void PlayFootsteps(BaseSoundSO _footsteps, GameObject noiseMaker)
    {
        AudioSource audioSource = noiseMaker.GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = noiseMaker.AddComponent<AudioSource>();
        }

        audioSource.outputAudioMixerGroup = sfxGroup;

        audioSource.clip = _footsteps.clip;
        audioSource.volume = _footsteps.volume;
        audioSource.pitch = _footsteps.pitch;
        audioSource.loop = _footsteps.loop;

        audioSource.Play();
    }

    public void PlaySoundAtPosition(BaseSoundSO sound, GameObject noiseMaker)
    {
        GameObject soundObject = new GameObject("Temp Audio");
        AudioSource audioSource = soundObject.GetComponent<AudioSource>();

        soundObject.transform.position = noiseMaker.transform.position;

        if (audioSource == null)
        {
            audioSource = soundObject.AddComponent<AudioSource>();
        }

        audioSource.outputAudioMixerGroup =
            sound.soundType == BaseSoundSO.SoundTypes.Music ? musicGroup : sfxGroup;

        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = sound.fallOffDistMin;
        audioSource.maxDistance = sound.fallOffDistMax;

        audioSource.clip = sound.clip;
        audioSource.volume = sound.volume;
        audioSource.pitch = sound.pitch;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        Destroy(soundObject, sound.clip.length);
    }
}