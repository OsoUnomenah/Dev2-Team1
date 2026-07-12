using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Mixer Routing")]
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup musicGroup;

    [SerializeField] public AudioSource amSource;

    public bool UISound;

    private void Awake()
    {
        instance = this;
        //DontDestroyOnLoad(this);
    }

    public void PlaySound(BaseSoundSO sound)
    {
        if (sound == null)
            return;

        GameObject soundObject = new GameObject("Temp Audio");
        AudioSource audioSource = soundObject.GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = soundObject.AddComponent<AudioSource>();
        }

        // Route to correct mixer group based on sound type
        audioSource.outputAudioMixerGroup =
            sound.soundType == BaseSoundSO.SoundTypes.Music ? musicGroup : sfxGroup;

        AudioClip currSound = sound.clips[Random.Range(0, sound.clips.Length)];
 
        audioSource.clip = currSound;
        audioSource.volume = sound.volume;

        if (sound.randomizePitch == true)
        {
            audioSource.pitch = Random.Range(0.9f, 1.3f);
        }
        else
        {
            audioSource.pitch = sound.pitch;
        }

        audioSource.loop = sound.loop;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        if (!sound.loop)
        {
            Destroy(soundObject, currSound.length);
        }
    }

    public void PlaySoundFromSource(BaseSoundSO sound, GameObject noiseMaker)
    {
        if(sound == null || noiseMaker == null)
            return;

        AudioSource audioSource = noiseMaker.GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = noiseMaker.AddComponent<AudioSource>();
        }

        AudioClip currSound = sound.clips[Random.Range(0, sound.clips.Length)];

        audioSource.outputAudioMixerGroup = sfxGroup;
   
        audioSource.clip = currSound;
        audioSource.volume = sound.volume;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = sound.fallOffDistMin;
        audioSource.maxDistance = sound.fallOffDistMax;

        if (sound.randomizePitch == true)
        {
            audioSource.pitch = Random.Range(0.9f, 1.3f);
        }
        else
        {
            audioSource.pitch = sound.pitch;
        }

        audioSource.loop = sound.loop;

        audioSource.PlayOneShot(currSound, sound.volume);
    }

    public void PlaySoundAtPosition(BaseSoundSO sound, GameObject noiseMaker)
    {
        if (sound == null || noiseMaker == null)
            return;

        GameObject soundObject = new GameObject("Temp Audio");
        AudioSource audioSource = soundObject.GetComponent<AudioSource>();

        soundObject.transform.position = noiseMaker.transform.position;

        if (audioSource == null)
        {
            audioSource = soundObject.AddComponent<AudioSource>();
        }

        audioSource.outputAudioMixerGroup =
            sound.soundType == BaseSoundSO.SoundTypes.Music ? musicGroup : sfxGroup;

        AudioClip currSound = sound.clips[Random.Range(0, sound.clips.Length)];

        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = sound.fallOffDistMin;
        audioSource.maxDistance = sound.fallOffDistMax;
        audioSource.loop = sound.loop;
        audioSource.clip = currSound;
        audioSource.volume = sound.volume;

        if (sound.randomizePitch == true)
        {
            audioSource.pitch = Random.Range(0.9f, 1.3f);
        }
        else
        {
            audioSource.pitch = sound.pitch;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        Destroy(soundObject, currSound.length);
    }

    public void PlaySoundFollowPosition(BaseSoundSO sound, GameObject noiseMaker, float duration = 0.1f)
    {
        if (sound == null || noiseMaker == null)
            return;

        GameObject soundObject = new GameObject("Temp Audio");
        AudioSource audioSource = soundObject.GetComponent<AudioSource>();

        soundObject.transform.SetParent(noiseMaker.transform, false);

        if (audioSource == null)
        {
            audioSource = soundObject.AddComponent<AudioSource>();
        }

        audioSource.outputAudioMixerGroup =
            sound.soundType == BaseSoundSO.SoundTypes.Music ? musicGroup : sfxGroup;

        AudioClip currSound = sound.clips[Random.Range(0, sound.clips.Length)];

        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = sound.fallOffDistMin;
        audioSource.maxDistance = sound.fallOffDistMax;
        audioSource.loop = sound.loop;

        audioSource.clip = currSound;
        audioSource.volume = sound.volume;

        if (sound.randomizePitch == true)
        {
            audioSource.pitch = Random.Range(0.9f, 1.3f);
        }
        else
        {
            audioSource.pitch = sound.pitch;
        }

        if (duration == 0.1f)
        {
            duration = currSound.length;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        Destroy(soundObject, duration);
    }

    public void PlayUISound(BaseSoundSO sound, PointerEventData data)
    {
        if (sound == null || data == null)
            return;

        // Route to correct mixer group based on sound type
        amSource.outputAudioMixerGroup =
            sound.soundType == BaseSoundSO.SoundTypes.Music ? musicGroup : sfxGroup;

        AudioClip clip = sound.clips[0];
        float clipLength = clip.length;

        amSource.volume = sound.volume;
        
        amSource.PlayOneShot(clip, clipLength);

        UISound = false;
    }
}