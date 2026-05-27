using UnityEngine;

public class KiWave : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Launch(Vector2 direction)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed;

        // --- PERBAIKAN UTAMA DI SINI ---
        // 1. Ambil dulu ukuran skala asli yang sudah kamu set di Inspector Prefab
        Vector3 currentScale = transform.localScale;

        // 2. Kita gunakan Mathf.Abs agar nilai X aslinya selalu positif dulu
        float originalXScale = Mathf.Abs(currentScale.x);

        // 3. Jika ke kiri, kalikan skala asli dengan -1. Jika ke kanan, tetap positif.
        if (direction.x < 0)
        {
            currentScale.x = -originalXScale; // Membalik ke kiri tanpa merubah ukuran asli
        }
        else
        {
            currentScale.x = originalXScale;  // Tetap menghadap kanan sesuai ukuran asli
        }

        // 4. Masukkan kembali nilainya ke localScale objek
        transform.localScale = currentScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}