using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject gameOverPanel; 
    public GameObject winningPanel;  

    [Header("Audio Settings")]
    public AudioSource levelBgmSource; // Tarik AudioSource BGM utama ke sini untuk dimatikan saat kalah/menang
    public AudioClip victorySound;
    public AudioClip loseSound;       

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winningPanel != null) winningPanel.SetActive(false);
        // Pemutaran BGM otomatis di awal sudah DIHAPUS dari sini euy!
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

            // Matikan BGM gameplay utama yang sedang berputar di scene
            if (levelBgmSource != null)
            {
                levelBgmSource.Stop(); 
                
                // Putar SFX kekalahan sekali saja lewat source tersebut
                if (loseSound != null) levelBgmSource.PlayOneShot(loseSound, 0.7f);
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

            // Matikan BGM gameplay utama yang sedang berputar di scene
            if (levelBgmSource != null)
            {
                levelBgmSource.Stop(); 
                
                // Putar SFX kemenangan sekali saja lewat source tersebut
                if (victorySound != null) levelBgmSource.PlayOneShot(victorySound, 0.7f);
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
        SceneManager.LoadScene("Main Menu"); 
    }
}