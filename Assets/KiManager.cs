using UnityEngine;
using UnityEngine.UI;

public class KiManager : MonoBehaviour
{
    public static KiManager Instance;

    [Header("Ki Settings")]
    public float maxKi = 3f;      // Maksimal 3 tong
    public float currentKi = 3f;  // Mulai game dengan 3 tong penuh

    [Header("UI Elements (3 Objek Tong)")]
    // Masukkan 3 UI Image dari tong kamu ke sini (KL1, KL1 (1), KL1 (2))
    public Image[] tongImages; 

    [Header("Sprite Aset Tong")]
    public Sprite spriteTongIsi;   // Tarik aset gambar tong yang ADA ISINYA ke sini
    public Sprite spriteTongKosong; // Tarik aset gambar tong yang KOSONG ke sini

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateKiUI();
    }

    // Fungsi mengurangi energi saat klik kanan (berkurang 1 tong)
    public bool TryUseKi()
    {
        if (currentKi >= 1f)
        {
            currentKi -= 1f;
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

            // --- KUNCI UTAMA: ATUR FILL AMOUNT DAN GANTI SPRITE ---
            if (currentKi >= i + 1)
            {
                // Tong Full
                tongImages[i].sprite = spriteTongIsi;
                tongImages[i].fillAmount = 1f;
            }
            else if (currentKi > i)
            {
                // Tong Terisi Sebagian (misal saat nambah 0.5 dari melee)
                tongImages[i].sprite = spriteTongIsi;
                tongImages[i].fillAmount = currentKi - i; 
            }
            else
            {
                // Tong Kosong Total -> Ganti ke Sprite Tong Kosong, set fillAmount ke 1 agar gambarnya muncul utuh
                tongImages[i].sprite = spriteTongKosong;
                tongImages[i].fillAmount = 1f; 
            }
        }
    }
}