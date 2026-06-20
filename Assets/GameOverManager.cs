using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject gameOverPanel; 
    public GameObject winningPanel;  

    [Header("Audio Settings")]
    public AudioSource audioSource;  
    public AudioClip gameplayBGM;    // BARU: Tempat narik lagu tema stage/level
    public AudioClip victorySound;
    public AudioClip loseSound;      

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winningPanel != null) winningPanel.SetActive(false);

        // BARU: Otomatis putar musik gameplay di awal stage
        if (audioSource != null && gameplayBGM != null)
        {
            audioSource.clip = gameplayBGM;
            audioSource.loop = true; // Paksa BGM agar terus mengulang (loop) saat habis
            audioSource.volume = 0.5f; // Atur volume BGM (50%) agar tidak menutupi SFX
            audioSource.Play();
        }
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

            // BARU: Matikan BGM gameplay terlebih dahulu agar suaranya tidak tabrakan
            if (audioSource != null)
            {
                audioSource.Stop(); 
                // Putar musik kekalahan sekali saja (tidak loop)
                if (loseSound != null) audioSource.PlayOneShot(loseSound, 0.7f);
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

            // BARU: Matikan BGM gameplay terlebih dahulu
            if (audioSource != null)
            {
                audioSource.Stop(); 
                // Putar musik kemenangan sekali saja (tidak loop)
                if (victorySound != null) audioSource.PlayOneShot(victorySound, 0.7f);
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