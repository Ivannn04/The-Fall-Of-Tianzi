using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SegmentedHealth : MonoBehaviour
{
    [Header("UI Reference")]
    public Image[] healthBars; 
    public Sprite fullDragonSprite;
    public Sprite brokenDragonSprite;

    [Header("Audio Settings")]
    public AudioSource audioSource; // Tarik komponen AudioSource MC ke sini
    public AudioClip hurtSound;     // Klip suara saat terkena hit
    public AudioClip deathSound;    // Klip suara saat mati

    [Header("Player Control References")]
    public Animator anim; 
    public MonoBehaviour movementScript; 

    [Header("Flash Effect Settings")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f;

    [Header("Game Over Settings")]
    public GameOverManager gameOverManager;

    private SpriteRenderer sr;
    private Rigidbody2D rb; 
    private int currentHealth;
    private int maxHealth;
    public bool isDead = false; 

    void Start()
    {
        isDead = false; 
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>(); 
        
        if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;
        
        if (movementScript != null) movementScript.enabled = true; 
        
        maxHealth = healthBars.Length;
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage()
    {
        if (isDead) return;

        currentHealth--;
        UpdateHealthUI();

        // BARU: Putar suara terluka jika klipnya ada
        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
    }

    StartCoroutine(FlashEffect());

    if (currentHealth <= 0) Die();
    else if (anim != null) anim.SetTrigger("Hurt");
    }

    IEnumerator FlashEffect()
    {
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            sr.color = originalColor;
        }
    }

    void UpdateHealthUI()
    {
        for (int i = 0; i < healthBars.Length; i++)
        {
            healthBars[i].enabled = true;
            // MEMPERBAIKI LOGIKA: Gunakan '<= i' agar perhitungannya pas dengan jumlah index array UI kamu
            healthBars[i].sprite = (i < currentHealth) ? fullDragonSprite : brokenDragonSprite;
        }
    }

    void Die()
    {
        isDead = true;

        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // 1. Matikan skrip pergerakan agar input player lumpuh total
        if (movementScript != null)
            movementScript.enabled = false;

        // 2. Kunci Fisika secara mutlak (Anti-Geser, Anti-Dorong)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; 
            rb.angularVelocity = 0f;          
            rb.gravityScale = 0;              
            rb.bodyType = RigidbodyType2D.Kinematic; 
        }

        // 3. Matikan Collider (Biar musuh bisa lewat menembus jasad player)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false; 
        }

        // Jalankan Coroutine penunda dengan angka pasti (3 atau 5 detik sesukamu)
        StartCoroutine(WaitForDeathAnimation());
    }

    // PASTIKAN FUNGSI INI ADA DI BAWAH DIE() DAN DI DALAM KELAS UTAMA
    IEnumerator WaitForDeathAnimation()
    {
        // Tunggu 3 detik penuh secara Real-time sebelum menu game over keluar
        yield return new WaitForSecondsRealtime(3f);

        // Setelah delay, barulah panel Game Over dinyalakan!
        if (gameOverManager != null)
        {
            gameOverManager.SetupGameOver();
        }
        else
        {
            Debug.LogError("GameOverManager belum ditarik ke dalam slot di Inspector SegmentedHealth!");
        }
    }
    // --- PERBAIKAN UTAMA DI SINI ---
    public void Heal(int amount)
    {
        // Pengaman: Jika player sudah mati, jangan beri dia heal
        if (isDead) return;

        // Tambah darah saat ini
        currentHealth += amount;

        // Batasi agar tidak melebihi darah maksimal
        if (currentHealth > maxHealth) 
        {
            currentHealth = maxHealth;
        }
        
        // FIX: Wajib panggil fungsi ini supaya gambar naga di UI langsung berubah nambah secara realtime!
        UpdateHealthUI();

        Debug.Log("MC berhasil memulihkan darah! Sisa HP sekarang: " + currentHealth);
    }
}