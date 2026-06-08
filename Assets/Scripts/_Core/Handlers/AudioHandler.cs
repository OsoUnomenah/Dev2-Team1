using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] private BaseSoundSO _jump;

    private void Awake()
    {
        instance = this;
    }


    public void PlaySound(AudioClip sound)
    {
        AudioSource.PlayClipAtPoint(sound, Camera.main.transform.position);
    }
}
