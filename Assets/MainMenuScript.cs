using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Audio; 
using UnityEngine.UI;    
using System.Collections; 
using TMPro; 

public class MainMenuManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject startPanel;
    public GameObject menuPanel;       
    public GameObject chaptersMenuPanel; 
    public GameObject settingsPanel; 

    [Header("Transition Settings")]
    public CanvasGroup screenFader;    
    public float fadeSpeed = 1.5f;     

    [Header("Audio Settings")] 
    public AudioSource bgmSource;
    public AudioMixer mainMixer;     
    public Slider bgmSlider;         
    public Slider sfxSlider;         

    [Header("FPS Settings")]
    public TMP_Dropdown fpsDropdown; 

    void Start()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }

        // --- LOAD & SET BGM VOLUME ---
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
            float savedBGM = PlayerPrefs.GetFloat("SavedBGM", 1f);
            bgmSlider.value = savedBGM;
            SetBGMVolume(savedBGM);
        }

        // --- LOAD & SET SFX VOLUME ---
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            float savedSFX = PlayerPrefs.GetFloat("SavedSFX", 1f);
            sfxSlider.value = savedSFX;
            SetSFXVolume(savedSFX);
        }

        // --- LOAD & SET FPS SETTINGS ---
        if (fpsDropdown != null)
        {
            fpsDropdown.onValueChanged.AddListener(SetFPS);
            int savedFPS = PlayerPrefs.GetInt("SavedFPS", 1); 
            fpsDropdown.value = savedFPS;
            SetFPS(savedFPS);
        }

        if (chaptersMenuPanel != null) chaptersMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false); 
        
        if (screenFader != null)
        {
            screenFader.alpha = 0;
            screenFader.blocksRaycasts = false;
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettingsMenu();
                return;
            }
            if (chaptersMenuPanel != null && chaptersMenuPanel.activeSelf)
            {
                CloseChaptersMenu();
                return;
            }
            if (menuPanel != null && menuPanel.activeSelf)
            {
                CloseMenu();
                return;
            }
        }
    }

    private void SwitchPanel(GameObject panelToHide, GameObject panelToShow)
    {
        if (panelToHide != null) panelToHide.SetActive(false);
        if (panelToShow != null) panelToShow.SetActive(true);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    public void OpenSettingsMenu()
    {
        SwitchPanel(menuPanel, settingsPanel);
    }

    public void CloseSettingsMenu()
    {
        SwitchPanel(settingsPanel, menuPanel);
    }

    // ========== FUNGSI PENGATURAN GLOBAL ==========
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
    // =====================================================

    public void OpenChaptersMenu()
    {
        SwitchPanel(menuPanel, chaptersMenuPanel);
    }

    public void CloseChaptersMenu()
    {
        SwitchPanel(chaptersMenuPanel, menuPanel);
    }

    public void CloseMenu()
    {
        SwitchPanel(menuPanel, null); 
        AnyButtonStarter starter = Object.FindAnyObjectByType<AnyButtonStarter>();
        if (starter != null) starter.ResetToStart();
    }

    // ========== TAMBAHAN: FUNGSI UNTUK PROLOGUE ==========
    public void LoadPrologue()
    {
        StartCoroutine(FadeAndLoadScene("Prologue")); // Pastikan nama scene di Build Settings persis seperti ini
    }
    // =====================================================

    public void LoadChapter1()
    {
        StartCoroutine(FadeAndLoadScene("Chapter 1 - Hutan"));
    }

    public void PlayGame()
    {
        // Biasanya tombol "Play" utama langsung mengarah ke awal cerita (Prologue)
        StartCoroutine(FadeAndLoadScene("Prologue")); 
    }

    IEnumerator FadeAndLoadScene(string sceneName)
    {
        if (screenFader != null)
        {
            screenFader.blocksRaycasts = true;
            float alpha = 0;
            while (alpha < 1)
            {
                alpha += Time.deltaTime * fadeSpeed;
                screenFader.alpha = alpha;
                yield return null;
            }
        }
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Game Keluar!"); 
        Application.Quit();
    }
}