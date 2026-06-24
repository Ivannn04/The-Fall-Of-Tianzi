using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // WAJIB ADA UNTUK MENANGANI BUTTON VIA CODE

public class PauseManager : MonoBehaviour
{
    public static bool isPaused = false;

    [Header("UI Reference")]
    public GameObject pausePanel; 
    public GameObject settingsPanel;
    public Button pauseButton; // BARU: Tarik UI Button Pause kamu ke sini di Inspector (Opsional)

    private float startDelayTimer = 0f;
    private float canToggleTime = 0.2f; 

    void Start()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false); 
        startDelayTimer = 0f;

        // BARU: Daftarkan fungsi klik jika kamu memasang button-nya langsung di Inspector
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePauseViaButton);
        }
    }

    void Update()
    {
        startDelayTimer += Time.unscaledDeltaTime;
        if (startDelayTimer < canToggleTime) return; 

        // Input khusus Keyboard ESC
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleEscInput();
        }
    }

    // Mengelompokkan logika ESC agar rapi
    private void HandleEscInput()
    {
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            CloseSettings();
        }
        else if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    // BARU: Fungsi khusus untuk Button UI di dalam Gameplay (Tombol Pause di pojok layar)
    public void TogglePauseViaButton()
    {
        // Jeda aman agar tidak sengaja terpencet di awal scene
        if (startDelayTimer < canToggleTime) return;

        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            // Jika setting lagi kebuka, tombol pause di pojok layar akan menutup setting dan kembali ke pause menu
            CloseSettings();
        }
        else if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
        if (pauseButton != null) pauseButton.interactable = false; // BARU: Matikan tombol pause gameplay saat menu pause aktif
        Debug.Log("Game Paused via Manager");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButton != null) pauseButton.interactable = true; // BARU: Aktifkan kembali tombol pause gameplay
        Debug.Log("Game Resumed via Manager");
    }

    public void OpenSettings()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("Main Menu"); 
    }
}