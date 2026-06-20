using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class AnyButtonStarter : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject menuPanel;
    private CanvasGroup startCG, menuCG;

    public Transform logoTransform;
    public float logoSpeed = 1f;

    public Transform textTransform;
    public float textSpeed = 2f;
    
    // Flag untuk melacak status
    private bool isMenuOpen = false;

    void Start()
    {
        startCG = startPanel.GetComponent<CanvasGroup>();
        menuCG = menuPanel.GetComponent<CanvasGroup>();

        startCG.alpha = 1;
        menuCG.alpha = 0;
        menuPanel.SetActive(false);
        StartCoroutine(FloatEffect());
    }

    IEnumerator FloatEffect()
    {
        // Tambahkan CanvasGroup untuk teks agar bisa kedip
        CanvasGroup textCG = textTransform.GetComponent<CanvasGroup>();
        if (textCG == null) textCG = textTransform.gameObject.AddComponent<CanvasGroup>();

        // Simpan posisi local asli Teks dan Logo saat start
        Vector3 originalTextPos = textTransform.localPosition;
        Vector3 originalLogoPos = (logoTransform != null) ? logoTransform.localPosition : Vector3.zero;

        while (!isMenuOpen)
        {
            // 1. Gerakan Teks (Naik Turun) berdasarkan posisi aslinya
            float yText = Mathf.Sin(Time.time * 2f) * 8f;
            textTransform.localPosition = new Vector3(originalTextPos.x, originalTextPos.y + yText, originalTextPos.z);

            // 2. Efek Kedip (Blinking) pada Teks
            textCG.alpha = Mathf.PingPong(Time.time * 2f, 3f);

            // 3. Gerakan Logo (Naik Turun) berdasarkan posisi aslinya
            if (logoTransform != null)
            {
                float yLogo = Mathf.Sin(Time.time * logoSpeed) * 5f;
                logoTransform.localPosition = new Vector3(originalLogoPos.x, originalLogoPos.y + yLogo, originalLogoPos.z);
            }
            
            yield return null;
        }
    }

    void Update()
    {
        // Jika menu sudah terbuka, fungsi Update akan langsung berhenti di sini
        if (isMenuOpen) return;

        if ((Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
        {
            isMenuOpen = true; // Tandai bahwa menu sudah terbuka
            StartCoroutine(Transition());
        }
    }


    IEnumerator Transition()
    {
        // Fade Out StartPanel
        for (float f = 1f; f >= 0; f -= 0.05f) {
            startCG.alpha = f;
            yield return new WaitForSeconds(0.02f);
        }
        startPanel.SetActive(false);

        // Fade In MenuPanel
        menuPanel.SetActive(true);
        for (float f = 0f; f <= 1; f += 0.05f) {
            menuCG.alpha = f;
            yield return new WaitForSeconds(0.02f);
        }
    }

    // Fungsi ini dipanggil oleh MainMenuManager saat kembali ke Start Panel
    public void ResetToStart()
    {
        isMenuOpen = false;           // Buka gerbang input 'press any button' kembali
        startPanel.SetActive(true);   // Munculkan panelnya
        
        if (startCG != null) 
        {
            startCG.alpha = 1f;       // Kembalikan visibilitas (hilangkan transparan 0)
        }
        
        StartCoroutine(FloatEffect()); // Jalankan animasi melayang dan kedip lagi
    }
}