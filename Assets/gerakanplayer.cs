using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // <--- WAJIB TAMBAHKAN BARIS INI!

public class playermovement1 : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 8f;

    private Rigidbody2D rb;
    private Animator anim; 
    private bool isGrounded;
    private bool isFacingRight = true; 

    private int nextAttackType = 1; 
    private bool attackBuffered = false; 

    [Header("BARU: Referensi UI Tombol Kanan Bawah")]
    public Image atkButtonImage;     
    public Image jumpButtonImage;    
    public Image specialButtonImage; 

    [Header("BARU: Pengaturan Warna Efek Tekan")]
    public Color pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f); 
    
    // PERBAIKAN: Buat penampung alpha asli untuk masing-masing tombol
    private float atkAlphaOriginal = 1f;
    private float jumpAlphaOriginal = 1f;
    private float specialAlphaOriginal = 1f;

    [Header("Melee Attack Settings")]
    public Transform attackPoint;        
    public float attackRange = 0.5f;     
    public LayerMask enemyLayers;        

    [Header("Ranged Attack Settings")]
    public GameObject kiWavePrefab; 
    public Transform firePoint;     

    private float idleTimer = 0f;
    public float idleDelay = 5f; 

    // Referensi ke script Health & Audio
    private SegmentedHealth healthScript;
    private PlayerAudioManager audioManager; // BARU: Referensi audio manager

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); 
        healthScript = GetComponent<SegmentedHealth>();
        audioManager = GetComponent<PlayerAudioManager>(); 

        // PERBAIKAN: Mengambil nilai transparan asli yang sudah kamu set di Inspector
        if (atkButtonImage != null) atkAlphaOriginal = atkButtonImage.color.a;
        if (jumpButtonImage != null) jumpAlphaOriginal = jumpButtonImage.color.a;
        if (specialButtonImage != null) specialAlphaOriginal = specialButtonImage.color.a;
    }

    void Update()
    {
        // 1. Cek Kematian (Jika mati, fungsi Update berhenti di sini)
        if (healthScript != null && healthScript.enabled && GetComponent<SegmentedHealth>().isDead) 
        {
            return; 
        }

        ResetButtonColors();

        // 2. Logika Pergerakan
        float move = 0;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) move = -1;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) move = 1;

        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        // 3. Logika Animasi
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        bool isAttacking = stateInfo.IsName("Player_atk") || stateInfo.IsName("Player_atk2") || stateInfo.IsName("Ranged_atk"); 

        anim.SetFloat("Speed", isAttacking ? 0 : Mathf.Abs(move));
        anim.SetBool("IsGrounded", isGrounded);

        // Idle Timer
        if (move == 0 && isGrounded && !isAttacking)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDelay) { anim.SetTrigger("PlayIdle"); idleTimer = 0f; }
        }
        else idleTimer = 0f;

        // Flip Karakter
        if ((move > 0 && !isFacingRight) || (move < 0 && isFacingRight)) Flip();

        // 4. Input Serangan
        if (Mouse.current.leftButton.isPressed && atkButtonImage != null) atkButtonImage.color = pressedColor;
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!isAttacking) ExecuteAttack();
            else attackBuffered = true;
        }

        if (!isAttacking && attackBuffered) { ExecuteAttack(); attackBuffered = false; }

        if (Mouse.current.rightButton.isPressed && specialButtonImage != null) specialButtonImage.color = pressedColor;
        if (Mouse.current.rightButton.wasPressedThisFrame && !isAttacking)
        {
            if (KiManager.Instance != null && KiManager.Instance.GetCurrentKi() >= 0.95f)
            {
                KiManager.Instance.TryUseKi();
                ShootKiWave();
            }
        }

        // 5. Jump
        if (Keyboard.current.spaceKey.isPressed && jumpButtonImage != null) jumpButtonImage.color = pressedColor;
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (anim != null) anim.SetTrigger("Jump");
            
            // BARU: Putar suara lompat
            if (audioManager != null) audioManager.PlayJump();
        }
    } 

    // --- FUNGSI PENDUKUNG ---

    private void ExecuteAttack()
    {
        anim.SetInteger("ComboCount", nextAttackType);
        
        // BARU: Putar suara tebasan melee
        if (audioManager != null) audioManager.PlayMeleeAttack();

        if (attackPoint != null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
            bool hitSuccess = false;
            foreach (Collider2D enemy in hitEnemies)
            {
                EnemyController enemyScript = enemy.GetComponent<EnemyController>();
                if (enemyScript != null) { enemyScript.TakeDamageFromPlayer(1); hitSuccess = true; }
            }
            if (hitSuccess && KiManager.Instance != null) KiManager.Instance.AddKiFromMelee();
        }
        nextAttackType = (nextAttackType == 1) ? 2 : 1;
    }

    private void ShootKiWave()
    {
        anim.SetTrigger("RangedAttack");

        // BARU: Putar suara tembakan ranged
        if (audioManager != null) audioManager.PlayRangedAttack();

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        GameObject waveObj = Instantiate(kiWavePrefab, spawnPosition, Quaternion.identity);
        KiWave waveScript = waveObj.GetComponent<KiWave>();
        if (waveScript != null) waveScript.Launch(isFacingRight ? Vector2.right : Vector2.left);
    }

    public void ResetComboParameter() { anim.SetInteger("ComboCount", 0); attackBuffered = false; }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnCollisionEnter2D(Collision2D collision) { if (collision.gameObject.CompareTag("Ground")) isGrounded = true; }
    private void OnCollisionExit2D(Collision2D collision) { if (collision.gameObject.CompareTag("Ground")) isGrounded = false; }
    
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null) { Gizmos.color = Color.red; Gizmos.DrawWireSphere(attackPoint.position, attackRange); }
    }

    private void ResetButtonColors()
    {
        // Kembalikan ke warna putih (normal) tapi Alphanya dikunci pakai Alpha transparan asli bawaanmu
        if (atkButtonImage != null && !Mouse.current.leftButton.isPressed) 
            atkButtonImage.color = new Color(1f, 1f, 1f, atkAlphaOriginal);

        if (specialButtonImage != null && !Mouse.current.rightButton.isPressed) 
            specialButtonImage.color = new Color(1f, 1f, 1f, specialAlphaOriginal);

        if (jumpButtonImage != null && !Keyboard.current.spaceKey.isPressed) 
            jumpButtonImage.color = new Color(1f, 1f, 1f, jumpAlphaOriginal);
    }
}