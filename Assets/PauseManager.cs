using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject pausePanel; // Tarik PausePanel ke sini

    private bool isPaused = false;
    private float startDelayTimer = 0f;
    private float canToggleTime = 0.2f; // Jeda aman 0.2 detik di awal game

    void Start()
    {
        // Paksa matikan panel di frame pertama
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        startDelayTimer = 0f;
    }

    void Update()
    {
        // Hitung waktu sejak game dimulai
        startDelayTimer += Time.unscaledDeltaTime;
        if (startDelayTimer < canToggleTime) return; // Jika belum 0.2 detik, abaikan input apa pun

        // Cek input tombol ESC
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true); 
        Time.timeScale = 0f; 
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false); 
        Time.timeScale = 1f; 
    }

    public void OpenSettings()
    {
        Debug.Log("Membuka Menu Setting...");
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenuScene"); 
    }
}