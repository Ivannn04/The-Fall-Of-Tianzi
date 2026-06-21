using UnityEngine;

public class LevelAudioManager : MonoBehaviour
{
    [Header("Audio Component")]
    public AudioSource audioSource;  

    [Header("BGM Settings")]
    public AudioClip gameplayBGM;    
    
    [Range(0f, 1f)] 
    public float bgmVolume = 0.5f; // Nilai volume yang bisa kamu geser-geser di Inspector

    void Start()
    {
        // Setup spesifikasi audio di awal, tapi tidak langsung di-play
        if (audioSource != null && gameplayBGM != null)
        {
            audioSource.clip = gameplayBGM;
            audioSource.loop = true; 
            audioSource.volume = bgmVolume; // Set volume awal
        }
    }

    void Update()
    {
        // === PERBAIKAN UTAMA ===
        // Terus pantau dan samakan volume AudioSource dengan slider bgmVolume di Inspector
        if (audioSource != null && audioSource.volume != bgmVolume)
        {
            audioSource.volume = bgmVolume;
            
            // Catatan: Cara ini juga mempermudah UI Slider Settings kamu nanti 
            // untuk langsung mengubah variabel 'bgmVolume' ini.
        }
    }

    // Fungsi pembantu yang dipanggil oleh ChapterIntroManager
    public void PlayBGM()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.volume = bgmVolume; // Pastikan volume sinkron saat mulai
            audioSource.Play();
        }
    }
}