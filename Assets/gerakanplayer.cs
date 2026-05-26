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

    // --- LOGIKA SIKLUS SERANGAN ---
    private int nextAttackType = 1; 

    // --- TAMBAHAN VARIABEL BUFFER INPUT AGAR RESPONSIF ---
    private bool attackBuffered = false; // Menyimpan status apakah ada antrean pencetan serang

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
        bool isAttacking = stateInfo.IsName("Player_atk") || stateInfo.IsName("Player_atk2");

        if (!isAttacking)
        {
            anim.SetFloat("Speed", Mathf.Abs(move));
        }
        else
        {
            anim.SetFloat("Speed", 0);
        }

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

        // --- SOLUSI UTAMA: LOGIKA INPUT SERANG DENGAN BUFFER ---
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!isAttacking)
            {
                // Jika sedang tidak menyerang, langsung eksekusi serangan
                ExecuteAttack();
            }
            else
            {
                // Jika sedang sibuk menyerang, masukkan klik ini ke dalam antrean (buffer)
                attackBuffered = true;
            }
        }

        // Jika animasi serang sudah selesai (isAttacking jadi false) DAN ada antrean input
        if (!isAttacking && attackBuffered)
        {
            ExecuteAttack();
            attackBuffered = false; // Kosongkan kembali antrean setelah digunakan
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    // Fungsi khusus untuk mengeksekusi serangan agar kode tidak ditulis berulang
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

    public void ResetComboParameter()
    {
        anim.SetInteger("ComboCount", 0);
        // Tambahan aman: pastikan antrean di-clear saat reset parameter dijalankan lewat Animation Event
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