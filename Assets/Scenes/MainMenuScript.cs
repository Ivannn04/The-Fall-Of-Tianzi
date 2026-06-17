using UnityEngine;
using UnityEngine.SceneManagement; // Ini wajib untuk pindah scene

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        // Masukkan nama scene-mu di sini. Sesuaikan dengan persis
        SceneManager.LoadScene("Chapter 1 - Hutan"); 
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}