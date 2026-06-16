using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    [Header("Audio Source Reference")]
    private AudioSource audioSource;

    [Header("Movement Sounds")]
    public AudioClip footstepSound;
    public AudioClip jumpSound;

    [Header("Combat Sounds")]
    public AudioClip meleeAttackSound;
    public AudioClip rangedAttackSound;

    void Awake()
    {
        // Otomatis mengambil komponen AudioSource yang ada di tubuh MC
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Pengaman agar suara tidak otomatis berbunyi saat game mulai
        audioSource.playOnAwake = false;
    }

    // 1. Fungsi Suara Langkah Kaki
    public void PlayFootstep()
    {
        // Gunakan check IsPlaying agar suara langkah kaki tidak tumpang tindih (kresek-kresek)
        if (footstepSound != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(footstepSound, 0.6f); // Volume 0.6 agar tidak terlalu pekak
        }
    }

    // 2. Fungsi Suara Lompat
    public void PlayJump()
    {
        if (jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }

    // 3. Fungsi Suara Tebasan Pedang / Melee
    public void PlayMeleeAttack()
    {
        if (meleeAttackSound != null)
        {
            audioSource.PlayOneShot(meleeAttackSound);
        }
    }

    // 4. Fungsi Suara Tembakan / Ranged
    public void PlayRangedAttack()
    {
        if (rangedAttackSound != null)
        {
            audioSource.PlayOneShot(rangedAttackSound);
        }
    }
}