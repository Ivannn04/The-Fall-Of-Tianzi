using UnityEngine;
using System.Collections;
using TMPro;

public class ChapterIntroManager : MonoBehaviour
{
    public static bool IsIntroActive { get; private set; }

    [System.Serializable]
    public struct SlideData
    {
        public GameObject panelObject;
        public AudioClip sfxClip;
    }

    [Header("Audio Components")]
    public AudioSource sfxSource;
    public AudioClip introFadeSfx;

    [Header("UI Main References")]
    public GameObject storyPanel;
    public UnityEngine.UI.Button nextButton;

    [Header("Transition Settings")]
    public CanvasGroup screenFader;
    public float textFadeSpeed = 1.0f;       // Sesuai parameter gambar
    public float textDisplayDuration = 2.0f; // Sesuai parameter gambar
    public float fadeSpeed = 1.5f;          // Sesuai parameter gambar
    public float slideFadeSpeed = 2.0f;
    public float typeSpeed = 0.05f;
    public GameObject prologueTitleText;

    [Header("Slides Setup")]
    public SlideData[] prologueSlides;
    public SlideData[] epilogueSlides;

    [Header("Gameplay Objects")]
    public GameObject playerObject;
    public MonoBehaviour[] scriptsToDisable;

    [Header("Ending")]
    public GameOverManager gameOverManager;

    private SlideData[] currentSlides;
    private int currentSlideIndex = 0;
    private bool isOutro = false;
    private bool isTextTyping = false;

    private string completeText = "";
    private TextMeshProUGUI activeTextComponent;
    private Coroutine typingCoroutine;

    void Start()
    {
        IsIntroActive = true;

        if (screenFader != null)
        {
            screenFader.alpha = 1f;
            screenFader.blocksRaycasts = true;
        }

        if (storyPanel != null)
        {
            if (storyPanel.GetComponent<CanvasGroup>() == null)
                storyPanel.AddComponent<CanvasGroup>();

            storyPanel.SetActive(true);
            storyPanel.GetComponent<CanvasGroup>().alpha = 0f;
        }

        ToggleGameplay(false);

        DisableAllSlides(prologueSlides);
        DisableAllSlides(epilogueSlides);

        if (nextButton != null)
        {
            nextButton.interactable = false; // Dikunci dulu pas awal sekuens persis ProloguePanelManager
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        // Ambil teks komponen dari judul fader dan buat dia transparan di awal (Alpha = 0)
        if (prologueTitleText != null)
        {
            TextMeshProUGUI titleComp = prologueTitleText.GetComponent<TextMeshProUGUI>();
            if (titleComp == null) titleComp = prologueTitleText.GetComponentInChildren<TextMeshProUGUI>();
            
            if (titleComp != null)
            {
                titleComp.color = new Color(titleComp.color.r, titleComp.color.g, titleComp.color.b, 0f);
            }
            prologueTitleText.SetActive(true);
        }

        StartCoroutine(StartIntroSequence());
    }

    IEnumerator StartIntroSequence()
    {
        yield return new WaitForSeconds(0.5f); // Jeda awal sebelum SFX mulai

        if (sfxSource != null && introFadeSfx != null)
        {
            sfxSource.PlayOneShot(introFadeSfx);
        }

        // Ambil referensi komponen teks untuk manipulasi warna vertex alpha
        TextMeshProUGUI titleTextComp = prologueTitleText != null ? prologueTitleText.GetComponent<TextMeshProUGUI>() : null;
        if (titleTextComp == null && prologueTitleText != null) titleTextComp = prologueTitleText.GetComponentInChildren<TextMeshProUGUI>();

        // ====================================================================
        // 1. FADE IN: Judul Prologue Muncul Perlahan (Layar Tetap Hitam Pekat)
        // ====================================================================
        if (titleTextComp != null)
        {
            float alpha = 0f;
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * textFadeSpeed;
                titleTextComp.color = new Color(titleTextComp.color.r, titleTextComp.color.g, titleTextComp.color.b, alpha);
                yield return null;
            }
            titleTextComp.color = new Color(titleTextComp.color.r, titleTextComp.color.g, titleTextComp.color.b, 1f);
        }

        // ====================================================================
        // 2. TIMING TAMPIL: Diam Selama Durasi Tertentu
        // ====================================================================
        yield return new WaitForSeconds(textDisplayDuration);

        // ====================================================================
        // 3. FADE OUT: Judul Prologue Menghilang (Layar Masih Hitam Pekat)
        // ====================================================================
        if (titleTextComp != null)
        {
            float alpha = 1f;
            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * textFadeSpeed;
                titleTextComp.color = new Color(titleTextComp.color.r, titleTextComp.color.g, titleTextComp.color.b, alpha);
                yield return null;
            }
            titleTextComp.color = new Color(titleTextComp.color.r, titleTextComp.color.g, titleTextComp.color.b, 0f);
        }

        // ====================================================================
        // 4. PREPARE DATA SLIDE PERTAMA (Sebelum Layar Hitam Dibuka)
        // ====================================================================
        currentSlides = prologueSlides;
        currentSlideIndex = 0;

        if (storyPanel != null)
            storyPanel.GetComponent<CanvasGroup>().alpha = 1f;

        if (currentSlides != null && currentSlides.Length > 0)
        {
            for (int i = 0; i < currentSlides.Length; i++)
            {
                if (currentSlides[i].panelObject != null)
                    currentSlides[i].panelObject.SetActive(i == 0);
            }

            activeTextComponent = currentSlides[0].panelObject.GetComponentInChildren<TextMeshProUGUI>();

            if (sfxSource != null && currentSlides[0].sfxClip != null)
            {
                sfxSource.Stop();
                sfxSource.clip = currentSlides[0].sfxClip;
                sfxSource.Play();
            }

            if (activeTextComponent != null)
            {
                completeText = activeTextComponent.text;
                activeTextComponent.text = ""; // Kosongkan teks sebelum diketik
            }
        }

        // ====================================================================
        // 5. FADE IN LAYAR: ScreenFader Membuka Dari Hitam Ke Transparan
        // ====================================================================
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
        }

        // Mulai mesin ketik untuk slide pertama setelah layar bersih terbuka
        if (activeTextComponent != null)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeTextAnimate());
        }

        if (nextButton != null) nextButton.interactable = true; // Tombol diaktifkan kembali
        if (prologueTitleText != null) prologueTitleText.SetActive(false);
    }

    void SpawnCurrentSlide()
    {
        if (currentSlides == null) return;

        if (currentSlideIndex >= currentSlides.Length)
        {
            EndStorySequence();
            return;
        }

        if (sfxSource != null) sfxSource.Stop(); // Matikan suara panel lama sebelum pindah
        StartCoroutine(TransitionWithFade());
    }

    IEnumerator TransitionWithFade()
    {
        // 1. FADE OUT LAYAR: Menggelap ke hitam pekat
        if (screenFader != null)
        {
            screenFader.blocksRaycasts = true;
            float alpha = 0f;
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * fadeSpeed * 2f; // Mengikuti perkalian kecepatan di logika lamamu
                screenFader.alpha = alpha;
                yield return null;
            }
            screenFader.alpha = 1f;
        }

        // --- PROSES DI BALIK LAYAR HITAM ---
        for (int i = 0; i < currentSlides.Length; i++)
        {
            if (currentSlides[i].panelObject == null) continue;

            currentSlides[i].panelObject.SetActive(i == currentSlideIndex);

            if (i == currentSlideIndex)
            {
                CanvasGroup cg = currentSlides[i].panelObject.GetComponent<CanvasGroup>();
                if (cg == null) cg = currentSlides[i].panelObject.AddComponent<CanvasGroup>();
                cg.alpha = 1f;

                if (sfxSource != null && currentSlides[i].sfxClip != null)
                {
                    sfxSource.clip = currentSlides[i].sfxClip;
                    sfxSource.Play();
                }

                activeTextComponent = currentSlides[i].panelObject.GetComponentInChildren<TextMeshProUGUI>();
                if (activeTextComponent != null)
                {
                    completeText = activeTextComponent.text;
                    activeTextComponent.text = ""; // Kosongkan sebelum render
                }
            }
        }

        yield return new WaitForSeconds(0.2f);

        // 2. FADE IN LAYAR: Layar membuka kembali perlahan menuju slide baru
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

        if (activeTextComponent != null)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeTextAnimate());
        }

        if (nextButton != null) nextButton.interactable = true;
    }

    IEnumerator TypeTextAnimate()
    {
        isTextTyping = true;
        foreach (char letter in completeText.ToCharArray())
        {
            activeTextComponent.text += letter;
            yield return new WaitForSecondsRealtime(typeSpeed); // Menggunakan Realtime seperti skrip ProloguePanelManager
        }
        isTextTyping = false;
    }

    void OnNextButtonClicked()
    {
        if (isTextTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            if (activeTextComponent != null) activeTextComponent.text = completeText;
            isTextTyping = false;
            return;
        }

        if (nextButton != null) nextButton.interactable = false;
        currentSlideIndex++;

        if (currentSlideIndex < currentSlides.Length)
            SpawnCurrentSlide();
        else
            EndStorySequence();
    }

public void OnObjectiveComplete()
    {
        // Cegah double-trigger jika fungsi ini tidak sengaja dipanggil dua kali dari sistem kill gameplay
        if (isOutro) return; 
        
        Debug.Log("Objective Complete! Memulai Transisi Epilog.");
        StartCoroutine(TransitionToOutroSequence());
    }

    IEnumerator TransitionToOutroSequence()
    {
        if (nextButton != null) nextButton.interactable = false; // Kunci tombol saat transisi layar pekat

        // 1. FADE OUT GAMEPLAY: Layar menggelap perlahan ke hitam pekat
        if (screenFader != null)
        {
            screenFader.blocksRaycasts = true;
            float alpha = screenFader.alpha; // Mulai dari alpha saat ini (biasanya 0f)
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * fadeSpeed;
                screenFader.alpha = alpha;
                yield return null;
            }
            screenFader.alpha = 1f;
        }

        // ====================================================================
        // PROSES DI BALIK LAYAR HITAM PEKAT (Sangat Aman dari Kebocoran Visual)
        // ====================================================================
        ToggleGameplay(false); // Matikan objek player dan skrip kontrol gameplay

        currentSlides = epilogueSlides;
        currentSlideIndex = 0;
        isOutro = true;

        // Bersihkan dulu seluruh sisa slide prologue agar tidak menumpuk di UI
        DisableAllSlides(prologueSlides);
        DisableAllSlides(epilogueSlides);

        // Siapkan Panel Utama Cerita
        if (storyPanel != null)
        {
            storyPanel.SetActive(true);
            CanvasGroup cg = storyPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = storyPanel.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
        }

        // Siapkan Aset dan Komponen Teks Epilog Slide Pertama (Index 0)
        if (currentSlides != null && currentSlides.Length > 0)
        {
            if (currentSlides[0].panelObject != null)
            {
                currentSlides[0].panelObject.SetActive(true);
            }

            if (sfxSource != null)
            {
                sfxSource.Stop(); // Hentikan SFX / sisa audio pertarungan gameplay jika ada
                if (currentSlides[0].sfxClip != null)
                {
                    sfxSource.clip = currentSlides[0].sfxClip;
                    sfxSource.Play();
                }
            }

            activeTextComponent = currentSlides[0].panelObject.GetComponentInChildren<TextMeshProUGUI>();
            if (activeTextComponent != null)
            {
                completeText = activeTextComponent.text;
                activeTextComponent.text = ""; // Kosongkan teks agar tidak langsung 'menembak' terlihat penuh
            }
        }

        // Beri jeda sepersekian detik di hitam pekat agar transisi terasa natural
        yield return new WaitForSeconds(0.4f);

        // 2. FADE IN EPILOG: Membuka fader dari hitam pekat ke transparan
        if (screenFader != null)
        {
            float alpha = 1f;
            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * fadeSpeed;
                screenFader.alpha = alpha;
                yield return null;
            }
            screenFader.alpha = 0f;
            screenFader.blocksRaycasts = false;
        }

        // 3. MULAI ANIMASI MESIN KETIK EPILOG
        if (activeTextComponent != null)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeTextAnimate());
        }

        if (nextButton != null) nextButton.interactable = true; // Buka interaksi tombol Next untuk cerita epilog
    }

    void EndStorySequence()
    {
        if (!isOutro)
            StartCoroutine(FadeOutToGameplay());
        else
            StartCoroutine(FadeToWinningPanel());
    }

    IEnumerator FadeOutToGameplay()
    {
        if (screenFader != null)
        {
            screenFader.blocksRaycasts = true;
            float alpha = 0f;
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * fadeSpeed;
                screenFader.alpha = alpha;
                yield return null;
            }
            screenFader.alpha = 1f;
        }

        if (storyPanel != null) storyPanel.SetActive(false);
        IsIntroActive = false;

        if (sfxSource != null) sfxSource.Stop();

        ToggleGameplay(true);

        if (screenFader != null)
        {
            float alpha = 1f;
            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * fadeSpeed;
                screenFader.alpha = alpha;
                yield return null;
            }
            screenFader.alpha = 0f;
            screenFader.blocksRaycasts = false;
        }
    }

    void ToggleGameplay(bool state)
    {
        if (playerObject != null) playerObject.SetActive(state);
        foreach (var script in scriptsToDisable)
        {
            if (script != null) script.enabled = state;
        }
    }

    IEnumerator FadeToWinningPanel()
    {
        if (screenFader != null)
        {
            screenFader.blocksRaycasts = true;
            float alpha = 0f;
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * fadeSpeed;
                screenFader.alpha = alpha;
                yield return null;
            }
            screenFader.alpha = 1f;
        }

        if (storyPanel != null) storyPanel.SetActive(false);

        if (gameOverManager != null)
        {
            if (sfxSource != null) sfxSource.Stop();
            gameOverManager.SetupWinning();
        }
    }

    void DisableAllSlides(SlideData[] slides)
    {
        foreach (var s in slides)
        {
            if (s.panelObject != null) s.panelObject.SetActive(false);
        }
    }
}