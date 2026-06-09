using UnityEngine;
using UnityEngine.InputSystem;

public class playermovement1 : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 8f;

    private Rigidbody2D rb;
    private Animator anim; 
    private bool isGrounded;
    private bool isFacingRight = true; 

    // --- LOGIKA SIKLUS SERANGAN MELEE (KLIK KIRI) ---
    private int nextAttackType = 1; 
    private bool attackBuffered = false; 

    [Header("Melee Attack Settings")]
    public Transform attackPoint;       
    public float attackRange = 0.5f;    
    public LayerMask enemyLayers;       

    // --- LOGIKA RANGED ATTACK (KLIK KANAN) ---
    [Header("Ranged Attack Settings")]
    public GameObject kiWavePrefab; 
    public Transform firePoint;     

    // --- VARIABEL TIMER IDLE 5 DETIK ---
    private float idleTimer = 0f;
    public float idleDelay = 5f; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); 
    }

    void Update()
    {
        float move = 0;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            move = -1;
        }
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            move = 1;
        }

        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        
        bool isAttacking = stateInfo.IsName("Player_atk") || 
                           stateInfo.IsName("Player_atk2") || 
                           stateInfo.IsName("Ranged_atk"); 

        if (!isAttacking)
        {
            anim.SetFloat("Speed", Mathf.Abs(move));
        }
        else
        {
            anim.SetFloat("Speed", 0);
        }

        anim.SetBool("IsGrounded", isGrounded);

        if (move == 0 && isGrounded && !isAttacking)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDelay)
            {
                anim.SetTrigger("PlayIdle"); 
                idleTimer = 0f;             
            }
        }
        else
        {
            idleTimer = 0f;
        }

        if (move > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (move < 0 && isFacingRight)
        {
            Flip();
        }

        // --- DETEKSI MELEE ATTACK (KLIK KIRI) ---
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!isAttacking)
            {
                ExecuteAttack();
            }
            else
            {
                attackBuffered = true;
            }
        }

        if (!isAttacking && attackBuffered)
        {
            ExecuteAttack();
            attackBuffered = false; 
        }

        // --- DETEKSI RANGED ATTACK (KLIK KANAN dengan Pengecekan Ki) ---
        if (Mouse.current.rightButton.wasPressedThisFrame && !isAttacking)
        {
            // Cek dulu ke KiManager apakah isi tong masih ada minimal 1 buah
            if (KiManager.Instance != null && KiManager.Instance.TryUseKi())
            {
                ShootKiWave();
            }
            else
            {
                Debug.Log("Ki kamu habis! Tebas musuh dulu untuk mengisi tong!");
            }
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (anim != null) anim.SetTrigger("Jump");
        }
    }

    private void ExecuteAttack()
    {
        anim.SetInteger("ComboCount", nextAttackType);

        if (attackPoint != null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

            // Boolean untuk menandai apakah tebasan kita sukses memukul musuh
            bool hitSuccess = false;

            foreach (Collider2D enemy in hitEnemies)
            {
                EnemyController enemyScript = enemy.GetComponent<EnemyController>();
                if (enemyScript != null)
                {
                    enemyScript.TakeDamageFromPlayer(1); 
                    hitSuccess = true; // Set true karena ada musuh yang terkena damage
                }
            }

            // KUNCI UTAMA MELEE FILL: Jika tebasan berhasil mengenai musuh, isi tong bertambah setengah (0.5)
            if (hitSuccess && KiManager.Instance != null)
            {
                KiManager.Instance.AddKiFromMelee();
            }
        }

        if (nextAttackType == 1)
        {
            nextAttackType = 2; 
        }
        else
        {
            nextAttackType = 1; 
        }
    }

    private void ShootKiWave()
    {
        anim.SetTrigger("RangedAttack");

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;

        GameObject waveObj = Instantiate(kiWavePrefab, spawnPosition, Quaternion.identity);
        KiWave waveScript = waveObj.GetComponent<KiWave>();

        if (waveScript != null)
        {
            Vector2 shootDirection = isFacingRight ? Vector2.right : Vector2.left;
            waveScript.Launch(shootDirection);
        }
    }

    public void ResetComboParameter()
    {
        anim.SetInteger("ComboCount", 0);
        attackBuffered = false; 
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
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

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}