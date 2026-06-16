using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject gameOverPanel; 
    public GameObject winningPanel;  

    [Header("Audio Settings")]
    public AudioSource audioSource;  
    public AudioClip victorySound;
    public AudioClip loseSound;      // BARU: Tempat memasukkan audio kekalahan

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winningPanel != null) winningPanel.SetActive(false);
    }

    // Fungsi Game Over
    public void SetupGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f; 
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // BARU: Putar musik/efek suara kekalahan jika ada
            if (audioSource != null && loseSound != null)
            {
                audioSource.PlayOneShot(loseSound);
            }
        }
    }

    // Fungsi Layar Kemenangan
    public void SetupWinning()
    {
        if (winningPanel != null)
        {
            winningPanel.SetActive(true);
            Time.timeScale = 0f; 
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (audioSource != null && victorySound != null)
            {
                audioSource.PlayOneShot(victorySound);
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextStage(string nextSceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenuScene"); 
    }
}