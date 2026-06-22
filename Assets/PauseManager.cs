using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static bool isPaused = false;

    [Header("UI Reference")]
    public GameObject pausePanel; // Tarik PausePanel ke sini
    public GameObject settingsPanel;

    private float startDelayTimer = 0f;
    private float canToggleTime = 0.2f; // Jeda aman 0.2 detik di awal game

    void Start()
    {
        // Paksa matikan panel di frame pertama
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false); // Pastikan settings juga mati di awal
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
            // 1. Prioritas Pertama: Cek apakah menu Settings sedang terbuka
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            // 2. Jika Settings tidak terbuka, cek apakah game sedang di-pause
            else if (isPaused)
            {
                ResumeGame();
            }
            // 3. Jika game sedang berjalan normal, maka Pause gamenya
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void OpenSettings()
    {
        // Matikan menu pause
        if (pausePanel != null) pausePanel.SetActive(false);

        // Nyalakan menu settings
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        // Nyalakan kembali menu pause
        if (pausePanel != null) pausePanel.SetActive(true);

        // Matikan menu settings
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("Main Menu"); 
    }
}