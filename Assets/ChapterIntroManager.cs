using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem; // <-- WAJIB ADA INI

public class ChapterIntroManager : MonoBehaviour
{
    // === TAMBAHKAN INI: Flag statis yang bisa dibaca oleh script manapun ===
    public static bool IsIntroActive { get; private set; }

    [Header("UI References")]
    public CanvasGroup screenFader;       
    public TextMeshProUGUI chapterText;  

    [Header("Audio Components")]
    public AudioSource sfxSource; 
    public LevelAudioManager levelAudioManager; 

    [Header("Transition Settings")]
    public float textFadeSpeed = 1.0f;   
    public float screenFadeSpeed = 1.0f; 
    public float textDisplayDuration = 2.0f; 

    [Header("BGM Timing Hack")]
    [Range(0.01f, 0.99f)] public float bgmTriggerThreshold = 0.3f; 

    void Update()
    {
        // Ganti Input lama menjadi sistem baru agar tidak crash euy
        if (Mouse.current.leftButton.wasPressedThisFrame || Keyboard.current.anyKey.wasPressedThisFrame)
        {
            // ... isi kode skip intro kamu yang sebelumnya (jika ada) ...
        }
    }

    void Awake()
    {
        // Set intro aktif sejak detik pertama scene dimuat
        IsIntroActive = true; 
    }

    void Start()
    {
        if (screenFader != null) screenFader.alpha = 1f;
        if (chapterText != null) chapterText.color = new Color(chapterText.color.r, chapterText.color.g, chapterText.color.b, 0f);

        StartCoroutine(PlayChapterIntro());
    }

    IEnumerator PlayChapterIntro()
    {
        if (screenFader != null)
        {
            screenFader.alpha = 1f;
            screenFader.blocksRaycasts = true; 
        }

        yield return new WaitForSeconds(0.5f);
        if (sfxSource != null) sfxSource.Play();

        // 1. Fade In Teks
        if (chapterText != null)
        {
            float alpha = 0f;
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * textFadeSpeed;
                chapterText.color = new Color(chapterText.color.r, chapterText.color.g, chapterText.color.b, alpha);
                yield return null;
            }
        }

        yield return new WaitForSeconds(textDisplayDuration);

        // 2. Fade Out Teks
        if (chapterText != null)
        {
            float alpha = 1f;
            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * textFadeSpeed;
                chapterText.color = new Color(chapterText.color.r, chapterText.color.g, chapterText.color.b, alpha);
                yield return null;
            }
        }

        // 3. Tirai Hitam Membuka
        if (screenFader != null)
        {
            float alpha = 1f;
            bool bgmPlayed = false;

            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * screenFadeSpeed;
                screenFader.alpha = alpha;

                if (alpha <= bgmTriggerThreshold && !bgmPlayed)
                {
                    if (levelAudioManager != null) levelAudioManager.PlayBGM();
                    bgmPlayed = true;
                }

                yield return null;
            }
            
            screenFader.alpha = 0f;
            screenFader.blocksRaycasts = false; 
        }

        // === KUNCI UTAMA DISINI ===
        // Intro resmi selesai, sekarang player baru boleh bergerak/menembak/klik kiri!
        IsIntroActive = false; 

        Debug.Log("Intro Selesai! Chapter 1 Dimulai.");
    }
}