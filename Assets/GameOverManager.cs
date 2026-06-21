using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject gameOverPanel; 
    public GameObject winningPanel;  

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winningPanel != null) winningPanel.SetActive(false);
    }

    public void SetupGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f; 
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Menggunakan Instance dari LevelAudioManager
            if (LevelAudioManager.Instance != null)
            {
                LevelAudioManager.Instance.PlayLoseSound();
            }
        }
    }

    public void SetupWinning()
        {
            Debug.Log("SETUP WINNING MASUK");

            if (winningPanel != null)
            {
                Debug.Log("WINNING PANEL AKTIF");

                winningPanel.SetActive(true);

                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (LevelAudioManager.Instance != null)
                {
                    LevelAudioManager.Instance.PlayVictorySound();
                }
            }
            else
            {
                Debug.LogError("WINNING PANEL BELUM DIISI!");
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