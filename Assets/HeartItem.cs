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

                // BARU: Putar suara tepat di posisi item sebelum objeknya dihancurkan
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                Destroy(gameObject);
            }
        }
    }
}