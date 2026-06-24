using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem; // WAJIB ADA UNTUK SISTEM INPUT BARU UNITY

public class ProloguePanelManager : MonoBehaviour
{
    private bool isTyping = false;
    private string currentFullText = "";
    private TextMeshProUGUI currentTextComponent;

    [System.Serializable]
    public struct SlideData
    {
        public GameObject panelObject; 
        public AudioClip sfxClip;      
    }

    [Header("UI References")]
    public Button nextButton;           
    public Button openSettingsButton; // BARU: Drag & Drop tombol settings kamu ke sini di Inspector
    public CanvasGroup screenFader;     
    public TextMeshProUGUI prologueTitleText; 

    [Header("Settings Panel Setup")]
    public GameObject settingsPanel; // Drag & Drop objek panel settings kamu ke sini di Inspector

    [Header("Audio Components")]
    public AudioSource bgmSource;       
    public AudioSource sfxSource;       
    public AudioSource fireAmbientSource; 

    [Header("Audio Settings")]
    public AudioClip prologueTitleSFX;  

    [Header("Slides Setup")]
    public List<SlideData> prologueSlides; 
    private int currentSlideIndex = 0;

    [Header("Typewriter Settings")]
    public float typingSpeed = 0.05f;    // Jeda waktu antar huruf (makin kecil makin cepat)
    private Coroutine typewriterCoroutine;

    [Header("Transition Settings")]
    public float textFadeSpeed = 1.0f;   
    public float textDisplayDuration = 2.0f; 
    public float fadeSpeed = 1.5f;
    public string nextSceneName = "Chapter 1 - Hutan";

    void Start()
    {
        if (nextButton != null) nextButton.onClick.AddListener(ShowNextSlide);
        
        // BARU: Daftarkan fungsi klik untuk tombol open settings
        if (openSettingsButton != null) openSettingsButton.onClick.AddListener(ToggleSettingsPanel);

        if (nextButton != null) nextButton.interactable = false;
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // === SINKRONISASI AUDIO AWAL ===
        float savedBGM = PlayerPrefs.GetFloat("SavedBGM", 1f); 
        float savedSFX = PlayerPrefs.GetFloat("SavedSFX", 1f);

        if (bgmSource != null) bgmSource.volume = savedBGM;
        if (sfxSource != null) sfxSource.volume = savedSFX;
        if (fireAmbientSource != null) fireAmbientSource.volume = savedBGM;

        int savedFPSIndex = PlayerPrefs.GetInt("SavedFPS", 1); 
        ApplySavedFPS(savedFPSIndex);

        if (screenFader != null)
        {
            screenFader.alpha = 1f;
            screenFader.blocksRaycasts = true;
        }

        if (prologueTitleText != null)
        {
            prologueTitleText.color = new Color(prologueTitleText.color.r, prologueTitleText.color.g, prologueTitleText.color.b, 0f);
        }

        InitSlides();
        StartCoroutine(PlayPrologueIntro());
    }

    void Update()
    {
        // DETEKSI UTAMA: Tekan tombol Escape (ESC) untuk buka/tutup settings panel
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleSettingsPanel();
        }
    }

    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null) return;

        bool isCurrentlyActive = settingsPanel.activeSelf;
        
        settingsPanel.SetActive(!isCurrentlyActive);
        PauseManager.isPaused = !isCurrentlyActive; 

        if (!isCurrentlyActive)
        {
            Time.timeScale = 0f; 
            if (nextButton != null) nextButton.interactable = false; 
            if (openSettingsButton != null) openSettingsButton.interactable = false; // BARU: Kunci tombol setting saat panel terbuka
            
            InitPrologueSliders();
            Debug.Log("Prologue Di-pause, Menampilkan Settings.");
        }
        else
        {
            Time.timeScale = 1f; 
            if (nextButton != null) nextButton.interactable = true; 
            if (openSettingsButton != null) openSettingsButton.interactable = true; // BARU: Buka kembali tombol setting saat panel tutup
            Debug.Log("Settings Ditutup, Melanjutkan Prologue.");
        }
    }

    public void CloseSettingsViaButton()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        PauseManager.isPaused = false;
        Time.timeScale = 1f;
        if (nextButton != null) nextButton.interactable = true;
        if (openSettingsButton != null) openSettingsButton.interactable = true; // BARU: Kembalikan interaksi tombol setting
    }

    private void InitPrologueSliders()
    {
        Slider[] sliders = settingsPanel.GetComponentsInChildren<Slider>(true);
        TMP_Dropdown dropdown = settingsPanel.GetComponentInChildren<TMP_Dropdown>(true);

        foreach (Slider slider in sliders)
        {
            string sliderName = slider.gameObject.name.ToLower();

            if (sliderName.Contains("bgm") || sliderName.Contains("music") || sliderName.Contains("bcm"))
            {
                float savedBGM = PlayerPrefs.GetFloat("SavedBGM", 1f);
                slider.value = savedBGM;
                
                if (bgmSource != null) bgmSource.volume = savedBGM;
                if (fireAmbientSource != null) fireAmbientSource.volume = savedBGM; 

                slider.onValueChanged.RemoveAllListeners();
                slider.onValueChanged.AddListener((val) => {
                    PlayerPrefs.SetFloat("SavedBGM", val);
                    if (bgmSource != null) bgmSource.volume = val; 
                    if (fireAmbientSource != null) fireAmbientSource.volume = val; 
                    PlayerPrefs.Save();
                });
            }
            else if (sliderName.Contains("sfx") || sliderName.Contains("sound") || sliderName.Contains("effect"))
            {
                float savedSFX = PlayerPrefs.GetFloat("SavedSFX", 1f);
                slider.value = savedSFX;

                if (sfxSource != null) sfxSource.volume = savedSFX;

                slider.onValueChanged.RemoveAllListeners();
                slider.onValueChanged.AddListener((val) => {
                    PlayerPrefs.SetFloat("SavedSFX", val);
                    if (sfxSource != null) sfxSource.volume = val; 
                    PlayerPrefs.Save();
                });
            }
        }

        if (dropdown != null)
        {
            dropdown.value = PlayerPrefs.GetInt("SavedFPS", 1);
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener((index) => {
                PlayerPrefs.SetInt("SavedFPS", index);
                ApplySavedFPS(index);
                PlayerPrefs.Save();
            });
        }
    }

    private void ApplySavedFPS(int index)
    {
        switch (index)
        {
            case 0: Application.targetFrameRate = 60; break;
            case 1: Application.targetFrameRate = 120; break;
            case 2: Application.targetFrameRate = -1; break;
        }
    }

    void InitSlides()
    {
        if (prologueSlides == null || prologueSlides.Count == 0)
        {
            Debug.LogError("Daftar prologueSlides masih kosong euy!");
            return;
        }

        for (int i = 0; i < prologueSlides.Count; i++)
        {
            if (prologueSlides[i].panelObject != null)
            {
                prologueSlides[i].panelObject.SetActive(false);
            }
        }
    }

    IEnumerator PlayPrologueIntro()
    {
        yield return new WaitForSeconds(0.5f);

        if (sfxSource != null && prologueTitleSFX != null)
        {
            sfxSource.PlayOneShot(prologueTitleSFX);
        }

        if (fireAmbientSource != null && !fireAmbientSource.isPlaying)
        {
            fireAmbientSource.Play();
        }

        if (prologueTitleText != null)
        {
            float alpha = 0f;
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * textFadeSpeed;
                prologueTitleText.color = new Color(prologueTitleText.color.r, prologueTitleText.color.g, prologueTitleText.color.b, alpha);
                yield return null;
            }
            prologueTitleText.color = new Color(prologueTitleText.color.r, prologueTitleText.color.g, prologueTitleText.color.b, 1f);
        }

        yield return new WaitForSeconds(textDisplayDuration);

        if (prologueTitleText != null)
        {
            float alpha = 1f;
            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * textFadeSpeed;
                prologueTitleText.color = new Color(prologueTitleText.color.r, prologueTitleText.color.g, prologueTitleText.color.b, alpha);
                yield return null;
            }
            prologueTitleText.color = new Color(prologueTitleText.color.r, prologueTitleText.color.g, prologueTitleText.color.b, 0f);
        }

        if (prologueSlides.Count > 0 && prologueSlides[0].panelObject != null)
        {
            prologueSlides[0].panelObject.SetActive(true);
            PrepareTypewriterForCurrentPanel();
        }

        PlayCurrentSlideSFX();

        if (screenFader != null)
        {
            screenFader.blocksRaycasts = true; 
            float alpha = 1f;
            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * fadeSpeed;
                screenFader.alpha = alpha;
                yield return null;
            }
            
            screenFader.alpha = 0f;
            screenFader.blocksRaycasts = false; 

            if (bgmSource != null && !bgmSource.isPlaying) 
            {
                bgmSource.Play();
            }
        }
        else
        {
            if (bgmSource != null && !bgmSource.isPlaying) bgmSource.Play();
        }

        if (currentTextComponent != null)
        {
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(TypeTextAnimate(currentTextComponent));
        }

        if (nextButton != null) nextButton.interactable = true;
    }

    public void ShowNextSlide()
    {
        if (PauseManager.isPaused) return;

        if (isTyping)
        {
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            
            if (currentTextComponent != null)
            {
                currentTextComponent.text = currentFullText; 
            }
            isTyping = false; 
        }
        else
        {
            if (nextButton != null) nextButton.interactable = false; 
            
            if (sfxSource != null)
            {
                sfxSource.Stop();
            }

            StartCoroutine(FadeBetweenPanels());
        }
    }

    IEnumerator FadeBetweenPanels()
    {
        if (screenFader != null)
        {
            screenFader.blocksRaycasts = true;
            
            float alpha = 0f;
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * fadeSpeed * 2f;
                screenFader.alpha = alpha;
                yield return null;
            }
            screenFader.alpha = 1f;
        }

        if (currentSlideIndex < prologueSlides.Count)
        {
            if (prologueSlides[currentSlideIndex].panelObject != null)
            {
                prologueSlides[currentSlideIndex].panelObject.SetActive(false);
            }

            currentSlideIndex++;

            if (currentSlideIndex < prologueSlides.Count)
            {
                if (prologueSlides[currentSlideIndex].panelObject != null)
                {
                    prologueSlides[currentSlideIndex].panelObject.SetActive(true);
                }
                
                PlayCurrentSlideSFX();
                PrepareTypewriterForCurrentPanel(); 

                yield return new WaitForSeconds(0.2f);

                if (screenFader != null)
                {
                    float alpha = 1f;
                    while (alpha > 0f)
                    {
                        alpha -= Time.deltaTime * fadeSpeed * 2f;
                        screenFader.alpha = alpha;
                        yield return null;
                    }
                    
                    screenFader.alpha = 0f;
                    screenFader.blocksRaycasts = false; 
                }

                if (currentTextComponent != null)
                {
                    if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
                    typewriterCoroutine = StartCoroutine(TypeTextAnimate(currentTextComponent));
                }
                
                if (nextButton != null) nextButton.interactable = true;
            }
            else
            {
                StartCoroutine(EndPrologueTransition());
            }
        }
    }

    void PrepareTypewriterForCurrentPanel()
    {
        GameObject currentPanel = prologueSlides[currentSlideIndex].panelObject;
        if (currentPanel != null)
        {
            currentTextComponent = currentPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (currentTextComponent != null)
            {
                currentFullText = currentTextComponent.text; 
                currentTextComponent.text = ""; 
            }
        }
    }

    IEnumerator TypeTextAnimate(TextMeshProUGUI textComponent)
    {
        isTyping = true;

        foreach (char c in currentFullText.ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false; 
    }

    void PlayCurrentSlideSFX()
    {
        if (currentSlideIndex < prologueSlides.Count && sfxSource != null)
        {
            AudioClip currentSFX = prologueSlides[currentSlideIndex].sfxClip;
            
            if (currentSFX != null)
            {
                sfxSource.clip = currentSFX;
                sfxSource.Play();
            }
        }
    }

    IEnumerator EndPrologueTransition()
    {
        float startBGMVolume = bgmSource != null ? bgmSource.volume : 0;
        float startFireVolume = fireAmbientSource != null ? fireAmbientSource.volume : 0;

        if (screenFader != null)
        {
            screenFader.blocksRaycasts = true; 
            float alpha = 0;
            while (alpha < 1)
            {
                alpha += Time.deltaTime * fadeSpeed;
                screenFader.alpha = alpha;

                if (bgmSource != null) bgmSource.volume = Mathf.Lerp(startBGMVolume, 0f, alpha);
                if (fireAmbientSource != null) fireAmbientSource.volume = Mathf.Lerp(startFireVolume, 0f, alpha);

                yield return null;
            }
        }

        if (bgmSource != null) bgmSource.Stop();
        if (fireAmbientSource != null) fireAmbientSource.Stop();
        
        SceneManager.LoadScene(nextSceneName);
    }
}