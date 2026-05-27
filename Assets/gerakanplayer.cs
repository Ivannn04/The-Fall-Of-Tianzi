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

    // --- LOGIKA RANGED ATTACK (KLIK KANAN) ---
    [Header("Ranged Attack Settings")]
    public GameObject kiWavePrefab; // Tempat menaruh prefab proyektil di Inspector
    public Transform firePoint;     // Titik munculnya proyektil

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

        // Ambil info status animasi saat ini
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        
        // Pengecekan status: apakah sedang memutar animasi attack klik kiri ATAU animasi tebasan ki klik kanan
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

        // Kirim status tanah ke Animator
        anim.SetBool("IsGrounded", isGrounded);

        // --- LOGIKA HITUNG MUNDUR TIMER IDLE ---
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

        // --- DETEKSI RANGED ATTACK (KLIK KANAN) ---
        // Hanya mendeteksi klik kanan, dan pastikan tidak sedang sibuk menyerang yang lain
        if (Mouse.current.rightButton.wasPressedThisFrame && !isAttacking)
        {
            ShootKiWave();
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void ExecuteAttack()
    {
        anim.SetInteger("ComboCount", nextAttackType);

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
        // 1. Picu animasi tebasan jarak jauh menggunakan TRIGGER baru (bukan ComboCount)
        anim.SetTrigger("RangedAttack");

        // 2. Tentukan posisi kemunculan proyektil
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;

        // 3. Spawn objek proyektil KiWave
        GameObject waveObj = Instantiate(kiWavePrefab, spawnPosition, Quaternion.identity);
        KiWave waveScript = waveObj.GetComponent<KiWave>();

        // 4. Berikan arah tembakan berdasarkan arah hadap karakter
        Vector2 shootDirection = isFacingRight ? Vector2.right : Vector2.left;
        waveScript.Launch(shootDirection);
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
}