using UnityEngine;
using UnityEngine.UI; // Wajib untuk UI Image

public class SegmentedHealth : MonoBehaviour
{
    [Header("UI Reference")]
    // Masukkan ke-5 objek Image bar di Hierarchy ke sini
    public Image[] healthBars; 

    [Header("Sprite Settings")]
    public Sprite fullDragonSprite;  // Masukkan aset kepala naga utuh/normal
    public Sprite brokenDragonSprite;// Masukkan aset kepala naga hancur/retak

    private int currentHealth;
    private int maxHealth;

    void Start()
    {
        maxHealth = healthBars.Length; 
        currentHealth = maxHealth;
        
        UpdateHealthUI();
    }

    // Fungsi dipanggil saat terkena 1 hit
    public void TakeDamage()
    {
        if (currentHealth > 0)
        {
            currentHealth--; // Berkurang 1 bar
            UpdateHealthUI();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Fungsi untuk menambah nyawa (jika ada item heal)
    public void Heal()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth++; // Bertambah 1 bar
            UpdateHealthUI();
        }
    }

    // Fungsi utama untuk mengganti sprite naga
    void UpdateHealthUI()
    {
        for (int i = 0; i < healthBars.Length; i++)
        {
            // Pastikan komponen Image-nya aktif/terlihat terlebih dahulu
            healthBars[i].enabled = true;

            // Jika indeks di bawah nyawa sekarang, pakai kepala utuh
            if (i < currentHealth)
            {
                healthBars[i].sprite = fullDragonSprite;
            }
            // Jika sudah terkena hit, ganti ke kepala ancur
            else
            {
                healthBars[i].sprite = brokenDragonSprite;
            }
        }
    }

    void Die()
    {
        Debug.Log("Karakter Kalah!");
    }
}