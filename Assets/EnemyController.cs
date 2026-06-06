using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    private Transform playerTransform;
    private Rigidbody2D rb;

    [Header("Jump Settings")]
    public float jumpForce = 6.5f;         // Kekuatan lompat disesuaikan
    public float obstacleCheckDistance = 0.7f; // Jarak deteksi dinding di depan
    private bool isGrounded;

    [Header("Combat Settings")]
    public int damageValue = 1;
    [Header("Attack Cooldown")]
    public float attackCooldown = 1f;
    private float nextAttackTime;

    private WaveEnemySpawner spawnerReference;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            Debug.LogError("Waduh! Prefab musuhmu belum dipasang komponen 'Rigidbody 2D' nih!");
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            MoveAndJumpLogic();
        }
    }

    void MoveAndJumpLogic()
    {
        if (rb == null) return;

        // Hitung arah horizontal ke player
        Vector2 direction = (playerTransform.position - transform.position).normalized;

        // Gerakan Horizontal Fisika
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        // --- LOGIKA LOMPAT OTOMATIS (ANTI-SELF COLLISION) ---
        if (isGrounded)
        {
            // Kondisi 1: MC berada di atas musuh dan jarak horizontalnya dekat
            bool playerIsAbove = (playerTransform.position.y - transform.position.y) > 0.5f;
            bool isCloseHorizontal = Mathf.Abs(playerTransform.position.x - transform.position.x) < 2.5f;
            bool jumpBecausePlayerAbove = playerIsAbove && isCloseHorizontal;

            // KUNCI UTAMA: Paksa Raycast mengabaikan collider dalam tubuh musuh sendiri
            Physics2D.queriesStartInColliders = false;

            // Tentukan arah raycast berdasarkan arah gerak aktual Rigidbody
            Vector2 rayDirection = rb.linearVelocity.x > 0 ? Vector2.right : Vector2.left;
            
            // Titik awal tembakan (sedikit dimajukan di depan badan musuh agar aman)
            Vector2 rayOriginLow = (Vector2)transform.position - new Vector2(0, 0.3f) + (rayDirection * 0.2f);
            Vector2 rayOriginHigh = (Vector2)transform.position + (rayDirection * 0.2f);

            // Tembak Raycast ke depan
            RaycastHit2D hitLow = Physics2D.Raycast(rayOriginLow, rayDirection, obstacleCheckDistance);
            RaycastHit2D hitHigh = Physics2D.Raycast(rayOriginHigh, rayDirection, obstacleCheckDistance);

            bool hitObstacleGround = (hitLow.collider != null && hitLow.collider.CompareTag("Ground")) || 
                                     (hitHigh.collider != null && hitHigh.collider.CompareTag("Ground"));

            // JIKA MENABRAK UNDAKAN DARI JAUH ATAU MC DI ATAS -> LANGSUNG LOMPAT
            if (hitObstacleGround || jumpBecausePlayerAbove)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                isGrounded = false; 
            }
        }

        // Flip Sprite (Logika Menghadap Kiri Bawaan Aset)
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    public void RegisterSpawner(WaveEnemySpawner spawner)
    {
        spawnerReference = spawner;
    }

    public void TakeDamageFromPlayer()
    {
        Die();
    }

    void Die()
    {
        if (spawnerReference != null)
        {
            spawnerReference.OnEnemyDefeated();
        }
        Destroy(gameObject);
    }

    // --- DETEKSI NYPIJAK TANAH LEWAT COLLISION TAG ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TryDamagePlayer(collision.gameObject);
        }
        
        // Jika bersentuhan dengan objek ber-tag Ground, berarti musuh menapak tanah
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TryDamagePlayer(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Jika kaki musuh keluar/lepas dari Tilemap Ground, status tanahnya hilang
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void TryDamagePlayer(GameObject player)
    {
        if (Time.time >= nextAttackTime)
        {
            SegmentedHealth playerHealth = player.GetComponent<SegmentedHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 rayDirection = transform.localScale.x < 0 ? Vector2.right : Vector2.left;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, rayDirection * obstacleCheckDistance);
        Gizmos.DrawRay((Vector2)transform.position - new Vector2(0, 0.3f), rayDirection * obstacleCheckDistance);
    }
}