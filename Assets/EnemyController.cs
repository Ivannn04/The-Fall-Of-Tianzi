using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private Animator anim; 

    [Header("Hit Feedback")]
    public float flashDuration = 0.15f;    
    private SpriteRenderer spriteRenderer; 
    private Color originalColor;           
    private bool isFlashing = false;
    public AudioSource audioSource;       // Tempat narik AudioSource musuh
    public AudioClip hitSound;            // Tempat memasukkan audio kena hit

    [Header("Jump Settings")]
    public float jumpForce = 6.5f;         
    public float obstacleCheckDistance = 0.7f; 
    private bool isGrounded;

    [Header("Combat Settings")]
    public int maxHealth = 3;             
    private int currentHealth;
    public int damageValue = 1;

    // Tambahan Duplikasi "Hit Feedback" kemarin sudah dihapus bersih di sini

    [Header("Attack Cooldown")]
    public float attackCooldown = 1f;
    private float nextAttackTime;

    [Header("Drop Item Settings")] 
    public GameObject hpItemPrefab; // Tarik Prefab Potion/Heart ke sini di Inspector
    [Range(0, 100)] public float dropChance = 25f; // Peluang drop 25%

    private WaveEnemySpawner spawnerReference;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); 
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color; 
        }

        currentHealth = maxHealth;

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            Debug.LogError("Prefab musuhmu belum dipasang komponen 'Rigidbody 2D'!");
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

        Vector2 direction = (playerTransform.position - transform.position).normalized;

        bool isAttacking = false;
        if (anim != null)
        {
            isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsName("Enem_Atk") || 
                          anim.GetAnimatorTransitionInfo(0).anyState; 
        }

        if (!isAttacking)
        {
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
            
            if (anim != null)
            {
                anim.SetFloat("Speed", moveSpeed);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f);
            }
        }

        if (isGrounded && !isAttacking)
        {
            bool playerIsAbove = (playerTransform.position.y - transform.position.y) > 0.5f;
            bool isCloseHorizontal = Mathf.Abs(playerTransform.position.x - transform.position.x) < 2.5f;
            bool jumpBecausePlayerAbove = playerIsAbove && isCloseHorizontal;

            Physics2D.queriesStartInColliders = false;

            Vector2 rayDirection = rb.linearVelocity.x > 0 ? Vector2.right : Vector2.left;
            
            Vector2 rayOriginLow = (Vector2)transform.position - new Vector2(0, 0.3f) + (rayDirection * 0.2f);
            Vector2 rayOriginHigh = (Vector2)transform.position + (rayDirection * 0.2f);

            RaycastHit2D hitLow = Physics2D.Raycast(rayOriginLow, rayDirection, obstacleCheckDistance);
            RaycastHit2D hitHigh = Physics2D.Raycast(rayOriginHigh, rayDirection, obstacleCheckDistance);

            bool hitObstacleGround = (hitLow.collider != null && hitLow.collider.CompareTag("Ground")) || 
                                     (hitHigh.collider != null && hitHigh.collider.CompareTag("Ground"));

            if (hitObstacleGround || jumpBecausePlayerAbove)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                isGrounded = false; 
            }
        }

        if (!isAttacking && spriteRenderer != null)
        {
            if (direction.x > 0)
            {
                spriteRenderer.flipX = true; 
            }
            else if (direction.x < 0)
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    public void RegisterSpawner(WaveEnemySpawner spawner)
    {
        spawnerReference = spawner;
    }

    public void TakeDamageFromPlayer(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " terkena serangan! Sisa HP: " + currentHealth);

        // Putar suara musuh terkena hit jika klip suaranya ada
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        StartCoroutine(FlashRedEffect());

        if (currentHealth <= 0)
        {
            DieWithDelay();
        }
    }

    IEnumerator FlashRedEffect()
    {
        if (spriteRenderer != null && !isFlashing)
        {
            isFlashing = true;
            spriteRenderer.color = Color.red; 
            yield return new WaitForSeconds(flashDuration); 
            spriteRenderer.color = originalColor; 
            isFlashing = false;
        }
    }

    void DieWithDelay()
    {
        // 1. Amankan drop item paling pertama sebelum komponen dihancurkan!
        CheckDropItem();

        // FIX BARU: Paksa sprite musuh langsung berubah jadi MERAH instan saat mati!
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red; 
        }

        // 2. Putar suara mati secara mandiri di posisi musuh agar tidak kepotong Destroy
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, 1f); 
        }

        // 3. Beritahu spawner kalau musuh kalah
        if (spawnerReference != null)
        {
            spawnerReference.OnEnemyDefeated();
        }

        // 4. Matikan collider dan fisik agar tidak mengganggu player lagi
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 5. Hancurkan game object musuh mengikuti durasi flashDuration
        // Pastikan variabel flashDuration di Inspector kamu bernilai minimal 0.15f atau 0.2f 
        // supaya mata kita sempat melihat kedipan merahnya sebelum musuh hilang!
        Destroy(gameObject, flashDuration);
    }

    void CheckDropItem()
    {
        if (hpItemPrefab == null) return;

        float randomRoll = Random.Range(0f, 100f);

        if (randomRoll <= dropChance)
        {
            Instantiate(hpItemPrefab, transform.position, Quaternion.identity);
            Debug.Log(gameObject.name + " menjatuhkan item HP!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
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
                if (anim != null)
                {
                    anim.SetTrigger("Attack");
                }

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