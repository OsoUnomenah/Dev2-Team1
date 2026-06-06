using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private BaseSoundSO _jump;

    private void OnEnable()
    {
         
    }

    private void OnDisable()
    {
        
    }

    public void PlaySound(BaseSoundSO sound)
    {
        if (sound != null)
        {
            GameObject soundObject = new GameObject("Temp Audio Source");
            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.clip = sound.clip;
            audioSource.Play();
        }
    }
}
