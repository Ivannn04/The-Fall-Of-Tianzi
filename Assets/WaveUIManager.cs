using UnityEngine;
using TMPro; // Pastikan ini ada jika menggunakan TextMeshPro

public class WaveUIManager : MonoBehaviour
{
    // KUNCI UTAMA: Baris ini yang dicari oleh WaveEnemySpawner agar tidak error
    public static WaveUIManager Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI waveNoticeText; 
    public TextMeshProUGUI waveTimerText;  

    void Awake()
    {
        // Setup Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (waveNoticeText != null) waveNoticeText.text = "";
        if (waveTimerText != null) waveTimerText.text = "";
    }

public void ShowWaveNotice(int waveNumber)
{
    if (waveNoticeText != null)
    {
        waveNoticeText.gameObject.SetActive(true);
        waveNoticeText.text = "WAVE " + waveNumber;
        
        // JANGAN panggil StartCoroutine(HideWaveNoticeAfterDelay) di sini 
        // agar teks Wave tetap nangkring di atas layar sepanjang game!
    }
}

    public void UpdateTimerText(float timeRemaining)
    {
        if (waveTimerText != null)
        {
            if (timeRemaining > 0)
            {
                waveTimerText.gameObject.SetActive(true);
                waveTimerText.text = "Next Wave in: " + Mathf.CeilToInt(timeRemaining).ToString() + "s";
            }
            else
            {
                waveTimerText.gameObject.SetActive(false);
            }
        }
    }

    System.Collections.IEnumerator HideWaveNoticeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (waveNoticeText != null) waveNoticeText.gameObject.SetActive(false);
    }
}