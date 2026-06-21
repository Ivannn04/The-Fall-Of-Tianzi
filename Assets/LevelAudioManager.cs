using UnityEngine;

public class LevelAudioManager : MonoBehaviour
{
    public static LevelAudioManager Instance { get; private set; }

    [Header("Audio Source")]
    public AudioSource bgmSource; // Cukup satu source untuk mengontrol musik latar

    [Header("Game Over & Win Music (Played as BGM)")]
    public AudioClip victorySound;
    public AudioClip loseSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
    }

    public void PlayVictorySound()
    {
        StopBGM(); 
        if (bgmSource != null && victorySound != null)
        {
            bgmSource.clip = victorySound;
            bgmSource.Play();
        }
    }

    public void PlayLoseSound()
    {
        StopBGM(); 
        if (bgmSource != null && loseSound != null)
        {
            bgmSource.clip = loseSound;
            bgmSource.Play();
        }
    }
}