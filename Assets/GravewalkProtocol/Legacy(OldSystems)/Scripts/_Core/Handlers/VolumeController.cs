using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Sliders")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;

    private const float MinVolume = 0.0001f;
    private const string SfxParam = "SFXVolume";
    private const string MusicParam = "MusicVolume";

    private void Awake()
    {
        // SFX
        float sfxSaved = PlayerPrefs.GetFloat(SfxParam, 1f);
        sfxSlider.value = sfxSaved;
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        SetSFXVolume(sfxSaved);

        // Music
        float musicSaved = PlayerPrefs.GetFloat(MusicParam, 1f);
        musicSlider.value = musicSaved;
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        SetMusicVolume(musicSaved);
    }

    public void SetSFXVolume(float volume)
    {
        SetVolume(SfxParam, volume);
    }

    public void SetMusicVolume(float volume)
    {
        SetVolume(MusicParam, volume);
    }

    private void SetVolume(string paramName, float volume)
    {
        float v = Mathf.Clamp(volume, MinVolume, 1f);
        float dB = Mathf.Log10(v) * 20f;
        audioMixer.SetFloat(paramName, dB);
        PlayerPrefs.SetFloat(paramName, volume);
    }
}