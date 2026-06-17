using UnityEngine;
using TMPro; // Wajib untuk mengakses TextMeshPro

public class FPSCounter : MonoBehaviour
{
    [Header("UI Text Component")]
    public TextMeshProUGUI fpsText; // Tarik objek teks FPS ke sini di Inspector

    [Header("Settings")]
    public float updateInterval = 0.5f; // Berapa detik sekali teksnya diupdate

    private float accum = 0f; // Jumlah FPS yang dikumpulkan selama interval
    private int frames = 0;   // Jumlah frame yang lewat
    private float timeleft;   // Sisa waktu untuk interval saat ini

    void Start()
    {
        if (fpsText == null)
        {
            fpsText = GetComponent<TextMeshProUGUI>();
        }
        timeleft = updateInterval;
    }

    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        // Jika interval waktu sudah habis, hitung rata-rata FPS dan update UI
        if (timeleft <= 0.0f)
        {
            float fps = accum / frames;
            
            if (fpsText != null)
            {
                // Format teks menjadi angka bulat (0 desimal)
                fpsText.text = "FPS: " + Mathf.RoundToInt(fps).ToString();
            }

            // Reset hitungan untuk interval berikutnya
            timeleft = updateInterval;
            accum = 0.0f;
            frames = 0;
        }
    }
}