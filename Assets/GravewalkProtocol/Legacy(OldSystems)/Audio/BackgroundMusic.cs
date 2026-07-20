using System.Collections;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float fadeDuration = 0.5f;

    private AudioClip previousMusic;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlayBossMusic(AudioClip bossMusic)
    {
        if (audioSource == null || bossMusic == null)
        {
            return;
        }

        if (audioSource.clip == bossMusic)
        {
            return;
        }

        previousMusic = audioSource.clip;
        SwitchMusic(bossMusic);
    }

    public void RestorePreviousMusic()
    {
        if (previousMusic == null)
        {
            return;
        }

        SwitchMusic(previousMusic);
        previousMusic = null;
    }

    private void SwitchMusic(AudioClip newMusic)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeMusicRoutine(newMusic));
    }

    private IEnumerator FadeMusicRoutine(AudioClip newMusic)
    {
        float originalVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            audioSource.volume = Mathf.Lerp(
                originalVolume,
                0f,
                timer / fadeDuration
            );

            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newMusic;
        audioSource.loop = true;
        audioSource.Play();

        timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            audioSource.volume = Mathf.Lerp(
                0f,
                originalVolume,
                timer / fadeDuration
            );

            yield return null;
        }

        audioSource.volume = originalVolume;
        fadeRoutine = null;
    }
}