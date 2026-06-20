using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioMixer mainMixer;
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("FPS Settings")]
    public TMP_Dropdown fpsDropdown;

    void Start()
    {
        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        if (fpsDropdown != null) fpsDropdown.onValueChanged.AddListener(SetFPS);

        // Ambil data yang disimpan dari Main Menu
        LoadSettings();
    }

    public void SetBGMVolume(float sliderValue)
    {
        if (mainMixer != null)
        {
            if (sliderValue <= 0) mainMixer.SetFloat("BGMVolume", -80f);
            else mainMixer.SetFloat("BGMVolume", Mathf.Log10(sliderValue) * 20);
        }
        PlayerPrefs.SetFloat("SavedBGM", sliderValue);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float sliderValue)
    {
        if (mainMixer != null)
        {
            if (sliderValue <= 0) mainMixer.SetFloat("SFXVolume", -80f);
            else mainMixer.SetFloat("SFXVolume", Mathf.Log10(sliderValue) * 20);
        }
        PlayerPrefs.SetFloat("SavedSFX", sliderValue);
        PlayerPrefs.Save();
    }

    public void SetFPS(int index)
    {
        switch (index)
        {
            case 0: Application.targetFrameRate = 60; break;
            case 1: Application.targetFrameRate = 120; break;
            case 2: Application.targetFrameRate = -1; break;
        }
        PlayerPrefs.SetInt("SavedFPS", index);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        float loadedBGM = PlayerPrefs.GetFloat("SavedBGM", 1f);
        if (bgmSlider != null) bgmSlider.value = loadedBGM;
        SetBGMVolume(loadedBGM);

        float loadedSFX = PlayerPrefs.GetFloat("SavedSFX", 1f);
        if (sfxSlider != null) sfxSlider.value = loadedSFX;
        SetSFXVolume(loadedSFX);

        int loadedFPS = PlayerPrefs.GetInt("SavedFPS", 1);
        if (fpsDropdown != null) fpsDropdown.value = loadedFPS;
        SetFPS(loadedFPS);
    }
}