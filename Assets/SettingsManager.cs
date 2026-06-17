using UnityEngine;
using UnityEngine.UI; // Untuk Slider
using UnityEngine.Audio; // Untuk Audio Mixer
using TMPro; // Untuk TMP Dropdown/Text

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioMixer mainMixer; // Tarik MainMixer kamu ke sini
    public Slider bgmSlider;     // Tarik Slider BGM ke sini
    public Slider sfxSlider;     // Tarik Slider SFX ke sini

    [Header("FPS Settings")]
    public TMP_Dropdown fpsDropdown; // Tarik Dropdown TMP untuk FPS ke sini

    void Start()
    {
        // 1. Setup Slider Audio Bawaan (Nilai Mixer berkisar -80dB sampai 20dB)
        if (bgmSlider != null)
        {
            bgmSlider.minValue = -40f; // Minimal suara (bisa di-set -80f untuk mute total)
            bgmSlider.maxValue = 0f;   // Maksimal suara normal
            bgmSlider.value = playerSettingsHasKey("BGM") ? PlayerPrefs.GetFloat("BGM") : 0f;
            SetBGMVolume(bgmSlider.value);
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = -40f;
            sfxSlider.maxValue = 0f;
            sfxSlider.value = playerSettingsHasKey("SFX") ? PlayerPrefs.GetFloat("SFX") : 0f;
            SetSFXVolume(sfxSlider.value);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        // 2. Setup Dropdown FPS Bawaan
        if (fpsDropdown != null)
        {
            int savedFPSIndex = PlayerPrefs.GetInt("FPSIndex", 1); // Default 60 FPS (index 1)
            fpsDropdown.value = savedFPSIndex;
            ApplyFPS(savedFPSIndex);
            fpsDropdown.onValueChanged.AddListener(ApplyFPS);
        }
    }

    public void SetBGMVolume(float volume)
    {
        // Jika slider di paling kiri mentok, matikan suara total (-80 desibel)
        if (volume <= -40f) mainMixer.SetFloat("BGMVolume", -80f);
        else mainMixer.SetFloat("BGMVolume", volume);

        PlayerPrefs.SetFloat("BGM", volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (volume <= -40f) mainMixer.SetFloat("SFXVolume", -80f);
        else mainMixer.SetFloat("SFXVolume", volume);

        PlayerPrefs.SetFloat("SFX", volume);
    }

    public void ApplyFPS(int index)
    {
        // Logika pilihan Dropdown (Urutan index: 0, 1, 2)
        switch (index)
        {
            case 0:
                Application.targetFrameRate = 60; // Pilihan 30 FPS
                break;
            case 1:
                Application.targetFrameRate = 120; // Pilihan 60 FPS
                break;
            case 2:
                Application.targetFrameRate = -1; // Pilihan 60 FPS
                break;
        }
        PlayerPrefs.SetInt("FPSIndex", index);
    }

    private bool playerSettingsHasKey(string key) => PlayerPrefs.HasKey(key);
}