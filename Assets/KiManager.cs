using UnityEngine;
using UnityEngine.UI;

public class KiManager : MonoBehaviour
{
    public static KiManager Instance;

    [Header("Ki Settings")]
    public float maxKi = 3f;      // Maksimal 3 tong
    public float currentKi = 3f;  // Mulai game dengan 3 tong penuh

    [Header("UI Elements (3 Objek Tong)")]
    public Image[] tongImages; 

    [Header("Sprite Aset Tong (3 Variasi)")]
    public Sprite spriteTongIsi;       // Gambar tong PENUH warna biru
    public Sprite spriteTongSetengah;   // BARU: Gambar tong setengah isi (0.5)
    public Sprite spriteTongKosong;     // Gambar tong KOSONG

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateKiUI();
    }

    // Fungsi mengambil data Ki saat ini untuk dicek oleh skrip Player
    public float GetCurrentKi()
    {
        return currentKi;
    }

    // Fungsi mengurangi energi saat klik kanan (berkurang 1 tong)
    public bool TryUseKi()
    {
        // Beri toleransi desimal kecil agar tidak tersangkut di angka 0.99f akibat pembulatan internal komputer
        if (currentKi >= 0.95f)
        {
            currentKi -= 1f;
            
            // Pengaman desimal gaib
            if (currentKi < 0.05f) currentKi = 0f;

            UpdateKiUI();
            return true; 
        }
        return false; 
    }

    // Fungsi menambah energi saat melee attack sukses (bertambah 0.5 tong)
    public void AddKiFromMelee()
    {
        currentKi = Mathf.Clamp(currentKi + 0.5f, 0f, maxKi);
        UpdateKiUI();
    }

    // Mengatur visual isi dan sprite tong secara realtime
    public void UpdateKiUI()
    {
        for (int i = 0; i < tongImages.Length; i++)
        {
            if (tongImages[i] == null) continue;

            // Selalu set fillAmount ke 1f agar sprite pixel art-mu muncul utuh tanpa terpotong kaku
            tongImages[i].fillAmount = 1f;

            // Hitung sisa Ki khusus untuk tong di indeks ke-i ini
            float kiForThisTong = currentKi - i;

            if (kiForThisTong >= 0.95f)
            {
                // Status 1: Tong Penuh Total
                tongImages[i].sprite = spriteTongIsi;
            }
            else if (kiForThisTong >= 0.45f && kiForThisTong < 0.95f)
            {
                // Status 2: Tong Terisi Setengah (Nilai mendekati 0.5)
                tongImages[i].sprite = spriteTongSetengah;
            }
            else
            {
                // Status 3: Tong Kosong Total
                tongImages[i].sprite = spriteTongKosong;
            }
        }
    }
}