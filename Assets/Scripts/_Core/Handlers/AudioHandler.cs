using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;


    private void Awake()
    {
        instance = this;
    }

    public void PlaySound(BaseSoundSO sound)
    {
        GameObject soundObject = new GameObject("Temp Audio");
        AudioSource audioSource = soundObject.GetComponent<AudioSource>();

        if (soundObject.GetComponent<AudioSource>() == null)
        {
            audioSource = soundObject.AddComponent<AudioSource>();
        }

        audioSource.clip = sound.clip;
        audioSource.volume = sound.volume;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        Destroy(soundObject, sound.clip.length);
    }


    public void PlayFootsteps(BaseSoundSO _footsteps, GameObject noiseMaker)
    {
        AudioSource audioSource = noiseMaker.GetComponent<AudioSource>(); 

        if (noiseMaker.GetComponent<AudioSource>() == null) // stops a bunch of audio sources from being made on the same object
        {
           audioSource = noiseMaker.AddComponent<AudioSource>();
        }

        audioSource.clip = _footsteps.clip;
        audioSource.volume = _footsteps.volume;

        audioSource.Play();
    }

    public void PlaySoundAtPosition(BaseSoundSO sound, GameObject noiseMaker)
    {
        
        GameObject soundObject = new GameObject("Temp Audio");
        AudioSource audioSource = soundObject.GetComponent<AudioSource>();

        soundObject.transform.position = noiseMaker.transform.position;

        if (soundObject.GetComponent<AudioSource>() == null)
        {
            audioSource = soundObject.AddComponent<AudioSource>();
        }

        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = sound.fallOffDistMin;
        audioSource.maxDistance = sound.fallOffDistMax;

        audioSource.clip = sound.clip;
        audioSource.volume = sound.volume;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        Destroy(soundObject, sound.clip.length);
    }
}
