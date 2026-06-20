using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections; 

public class MainMenuManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject startPanel;
    public GameObject menuPanel;       
    public GameObject chaptersMenuPanel; 

    [Header("Transition Settings")]
    public CanvasGroup screenFader;    
    public float fadeSpeed = 1.5f;     

    void Start()
    {
        if (chaptersMenuPanel != null) chaptersMenuPanel.SetActive(false);
        
        if (screenFader != null)
        {
            screenFader.alpha = 0;
            screenFader.blocksRaycasts = false;
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (chaptersMenuPanel != null && chaptersMenuPanel.activeSelf)
            {
                CloseChaptersMenu();
            }
            else if (menuPanel != null && menuPanel.activeSelf)
            {
                CloseMenu();
            }
        }
    }

    // Fungsi helper yang lebih bersih, hanya mengatur panel dan mereset memori klik (EventSystem)
    private void SwitchPanel(GameObject panelToHide, GameObject panelToShow)
    {
        if (panelToHide != null) panelToHide.SetActive(false);
        if (panelToShow != null) panelToShow.SetActive(true);
        
        // Lepaskan fokus dari EventSystem agar tidak ada tombol yang nyangkut di state "Selected"
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OpenChaptersMenu()
    {
        SwitchPanel(menuPanel, chaptersMenuPanel);
    }

    public void CloseChaptersMenu()
    {
        SwitchPanel(chaptersMenuPanel, menuPanel);
    }

    public void CloseMenu()
    {
        // Sembunyikan menuPanel
        SwitchPanel(menuPanel, null); 

        // Bangunkan kembali AnyButtonStarter
        AnyButtonStarter starter = Object.FindFirstObjectByType<AnyButtonStarter>();
        if (starter != null)
        {
            starter.ResetToStart();
        }
    }

    public void LoadChapter1()
    {
        StartCoroutine(FadeAndLoadScene("Chapter 1 - Hutan"));
    }

    IEnumerator FadeAndLoadScene(string sceneName)
    {
        if (screenFader != null)
        {
            screenFader.blocksRaycasts = true;
            float alpha = 0;
            while (alpha < 1)
            {
                alpha += Time.deltaTime * fadeSpeed;
                screenFader.alpha = alpha;
                yield return null;
            }
        }
        SceneManager.LoadScene(sceneName);
    }

    public void PlayGame()
    {
        StartCoroutine(FadeAndLoadScene("Chapter 1 - Hutan"));
    }

    public void QuitGame()
    {
        Debug.Log("Game Keluar!"); 
        Application.Quit();
    }
}