using UnityEngine;

public class HeartItem : MonoBehaviour
{
    [Header("Settings")]
    public int healAmount = 1; 

    [Header("Audio Settings")]
    public AudioClip pickupSound; // Tarik klip suara mengambil potion di sini

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SegmentedHealth playerHealth = collision.GetComponent<SegmentedHealth>();

            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount); 

                // PERBAIKAN: Cari PlayerAudioManager dari player yang menabrak item
                PlayerAudioManager audioManager = collision.GetComponent<PlayerAudioManager>();
                
                if (audioManager != null)
                {
                    // Panggil fungsi putar suara pickup dari audio manager player
                    audioManager.PlayPickup(); 
                }
                else
                {
                    // Backup Jaga-jaga kalau player kelupaan pasang PlayerAudioManager,
                    // kita paksa bersuara di koordinat Z kamera agar tetap terdengar kencang!
                    if (pickupSound != null)
                    {
                        Vector3 safeSoundPos = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z + 1f);
                        AudioSource.PlayClipAtPoint(pickupSound, safeSoundPos);
                    }
                }

                Destroy(gameObject);
            }
        }
    }
}