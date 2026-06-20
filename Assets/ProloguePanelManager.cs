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
        if (nextButton != null) nextButton.interactable = false;
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // === SINKRONISASI AUDIO AWAL (Mengikuti Key MainMenu Kamu euy!) ===
        float savedBGM = PlayerPrefs.GetFloat("SavedBGM", 1f); 
        float savedSFX = PlayerPrefs.GetFloat("SavedSFX", 1f);

        if (bgmSource != null) bgmSource.volume = savedBGM;
        if (sfxSource != null) sfxSource.volume = savedSFX;
        if (fireAmbientSource != null) fireAmbientSource.volume = savedBGM;

        // Terapkan limit FPS yang disimpan dari MainMenu saat prologue dimulai
        int savedFPSIndex = PlayerPrefs.GetInt("SavedFPS", 1); // Default index 1 (120 FPS di MainMenu kamu)
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

        if (nextButton != null) nextButton.onClick.AddListener(ShowNextSlide);

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

        // Cek status aktif panel saat ini
        bool isCurrentlyActive = settingsPanel.activeSelf;
        
        // Balikkan kondisinya (jika nyala jadi mati, jika mati jadi nyala)
        settingsPanel.SetActive(!isCurrentlyActive);

        // Integrasikan dengan PauseManager global kamu
        PauseManager.isPaused = !isCurrentlyActive; 

        // Atur jalannya waktu game
        if (!isCurrentlyActive)
        {
            Time.timeScale = 0f; // Hentikan waktu dunia game
            if (nextButton != null) nextButton.interactable = false; // Kunci tombol next selama pause
            
            // === LINK LOGIKA SLIDER PROLOGUE SAAT DI-PAUSE ===
            InitPrologueSliders();
            Debug.Log("Prologue Di-pause, Menampilkan Settings.");
        }
        else
        {
            Time.timeScale = 1f; // Jalankan waktu normal kembali
            if (nextButton != null) nextButton.interactable = true; // Buka kembali tombol next
            Debug.Log("Settings Ditutup, Melanjutkan Prologue.");
        }
    }

    // FUNGSI KHUSUS: Pasang fungsi ini pada Event OnClick() milik Tombol "Back / Close / Resume" di dalam panel UI Settings kamu
    public void CloseSettingsViaButton()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        PauseManager.isPaused = false;
        Time.timeScale = 1f;
        if (nextButton != null) nextButton.interactable = true;
    }

    // ================= LOGIKA SINKRONISASI PENGATURAN DI PROLOGUE =================

    private void InitPrologueSliders()
    {
        // Cari komponen UI Slider & Dropdown secara otomatis di dalam SettingsPanel
        Slider[] sliders = settingsPanel.GetComponentsInChildren<Slider>(true);
        TMP_Dropdown dropdown = settingsPanel.GetComponentInChildren<TMP_Dropdown>(true);

        // Sinkronisasi nilai slider BGM & SFX 
        foreach (Slider slider in sliders)
        {
            string sliderName = slider.gameObject.name.ToLower();

            // === BLOK BGM (Sekarang Musik + Suara Api Masuk Sini!) ===
            if (sliderName.Contains("bgm") || sliderName.Contains("music") || sliderName.Contains("bcm"))
            {
                float savedBGM = PlayerPrefs.GetFloat("SavedBGM", 1f);
                slider.value = savedBGM;
                
                // Set volume awal saat panel dibuka
                if (bgmSource != null) bgmSource.volume = savedBGM;
                if (fireAmbientSource != null) fireAmbientSource.volume = savedBGM; // Api ikut volume BGM

                slider.onValueChanged.RemoveAllListeners();
                slider.onValueChanged.AddListener((val) => {
                    PlayerPrefs.SetFloat("SavedBGM", val);
                    if (bgmSource != null) bgmSource.volume = val; 
                    if (fireAmbientSource != null) fireAmbientSource.volume = val; // Api dikontrol real-time oleh Slider BGM
                    PlayerPrefs.Save();
                });
            }
            // === BLOK SFX (Hanya Efek Suara Saja) ===
            else if (sliderName.Contains("sfx") || sliderName.Contains("sound") || sliderName.Contains("effect"))
            {
                float savedSFX = PlayerPrefs.GetFloat("SavedSFX", 1f);
                slider.value = savedSFX;

                // Set volume awal untuk SFX saja
                if (sfxSource != null) sfxSource.volume = savedSFX;

                slider.onValueChanged.RemoveAllListeners();
                slider.onValueChanged.AddListener((val) => {
                    PlayerPrefs.SetFloat("SavedSFX", val);
                    if (sfxSource != null) sfxSource.volume = val; // Hanya mengubah volume SFX real-time
                    PlayerPrefs.Save();
                });
            }
        }

        // Sinkronisasi Dropdown FPS
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
        // Menyelaraskan dengan target FPS yang ada di MainMenuManager kamu euy!
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

        // Memainkan SFX judul di awal menggunakan PlayOneShot aman karena posisinya di intro awal scene
        if (sfxSource != null && prologueTitleSFX != null)
        {
            sfxSource.PlayOneShot(prologueTitleSFX);
        }

        if (fireAmbientSource != null && !fireAmbientSource.isPlaying)
        {
            fireAmbientSource.Play();
        }

        // FADE IN: Judul Prologue muncul
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

        // FADE OUT: Judul Prologue menghilang
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

        // === SEKUENS INTRO SELESAI (MASUK SLIDE 1) ===
        if (prologueSlides.Count > 0 && prologueSlides[0].panelObject != null)
        {
            prologueSlides[0].panelObject.SetActive(true);
            PrepareTypewriterForCurrentPanel();
        }

        // Jalankan SFX Slide Pertama
        PlayCurrentSlideSFX();

        // FADE IN LAYAR: Mengubah layar fader dari hitam pekat ke transparan
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
            
            // === FIX UTAMA: Hentikan SFX panel saat ini begitu tombol Next ditekan ===
            if (sfxSource != null)
            {
                sfxSource.Stop();
            }

            StartCoroutine(FadeBetweenPanels());
        }
    }

    IEnumerator FadeBetweenPanels()
    {
        // 1. FADE OUT: Layar menggelap ke hitam pekat
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

        // === PROSES DI BALIK LAYAR HITAM PEKAT ===
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
                
                // Mainkan SFX panel baru (menggunakan sistem override .clip, bukan PlayOneShot)
                PlayCurrentSlideSFX();
                PrepareTypewriterForCurrentPanel(); 

                yield return new WaitForSeconds(0.2f);

                // 2. FADE IN: Layar membuka kembali perlahan
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
            
            // === FIX UTAMA: Ubah sistem menjadi AudioSource.clip agar bisa di-Stop() sewaktu-waktu ===
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