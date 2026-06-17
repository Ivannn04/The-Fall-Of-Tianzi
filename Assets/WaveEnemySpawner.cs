using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem; // WAJIB ada ini untuk sistem input baru Unity
using TMPro; // WAJIB ada ini untuk mengatur teks TextMeshPro Objective

public class WaveEnemySpawner : MonoBehaviour
{
    [Header("Import UI Manager")]
    public GameOverManager uiManager; // Tarik objek _GameOverManager ke sini di Inspector

    [Header("Pool Musuh & Lokasi")]
    public GameObject[] enemyPrefabs; 
    public Transform[] spawnPoints;     

    [Header("Pengaturan Target Game")]
    public int totalTargetKills = 15;   
    
    [Header("Pengaturan Jeda UI Wave")]
    public float timeBetweenWaves = 5f; 

    [Header("UI Awal Game (Start Prompt)")]
    public GameObject startPromptText; // Drag & Drop objek 'StartPromptText' ke sini

    [Header("UI Objective Settings")]
    public TextMeshProUGUI objectiveText; 
    public GameObject fpsTextObject; // Drag & Drop objek teks 'ObjectiveText' ke sini

    // Status Tracker
    private int totalEnemiesKilled = 0; 
    private int currentWaveNumber = 1;
    private int enemiesRemainingInWave = 0;
    private bool isWaitingForNextWave = false; 
    private bool gameHasStarted = false; 
    private bool gameIsOver = false; // Pengaman agar wave berhenti total saat menang/kalah

    void Start()
    {
        if (fpsTextObject != null) fpsTextObject.SetActive(false);

        // Pastikan UI Start menyala di awal game
        if (startPromptText != null) startPromptText.SetActive(true);

        // TAMBAHKAN INI: Matikan objek teks obyektif di awal game agar tidak kelihatan
        if (objectiveText != null) objectiveText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (gameIsOver) return; // Jika game sudah selesai, matikan semua fungsi update spawner

        // JURUS ANTI-GAGAL INPUT SYSTEM BARU:
        if (!gameHasStarted && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("Klik mouse kiri terdeteksi secara global! Memulai game...");
            PlayerClickedToStart();
        }
    }

    public void PlayerClickedToStart()
    {
        if (gameHasStarted || gameIsOver) return; 

        gameHasStarted = true;

        // Matikan UI Start dari layar
        if (startPromptText != null) startPromptText.SetActive(false);

        if (objectiveText != null) objectiveText.gameObject.SetActive(true);

        if (fpsTextObject != null) fpsTextObject.SetActive(true);

        // PERBAIKAN: Tampilkan inisialisasi awal objective (0/15) serempak tepat saat game baru dimulai!
        UpdateObjectiveUI();
        

        // Mulai hitung mundur Wave 1
        StartCoroutine(StartNextWaveWithTimer(true));
    }

    IEnumerator StartNextWaveWithTimer(bool isFirstWave)
    {
        isWaitingForNextWave = true;

        while (WaveUIManager.Instance == null)
        {
            yield return null; 
        }

        // Cek sebelum menampilkan notifikasi wave ke layar UI
        if (gameIsOver || totalEnemiesKilled >= totalTargetKills)
        {
            isWaitingForNextWave = false;
            yield break; // Hentikan coroutine detik ini juga
        }

        WaveUIManager.Instance.ShowWaveNotice(currentWaveNumber);

        float countdown = timeBetweenWaves;
        while (countdown > 0)
        {
            // Jika di tengah hitung mundur tiba-tiba pemain menang, langsung potong kompas keluar
            if (gameIsOver) { WaveUIManager.Instance.UpdateTimerText(0); yield break; }

            WaveUIManager.Instance.UpdateTimerText(countdown);
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        WaveUIManager.Instance.UpdateTimerText(0);
        isWaitingForNextWave = false;

        ActualSpawnLogic();
    }

    void ActualSpawnLogic()
    {
        // Cek kemenangan utama
        if (totalEnemiesKilled >= totalTargetKills || gameIsOver)
        {
            WinGame();
            return;
        }

        int enemiesToSpawn = currentWaveNumber;
        int remainingToTarget = totalTargetKills - totalEnemiesKilled;
        if (enemiesToSpawn > remainingToTarget)
        {
            enemiesToSpawn = remainingToTarget;
        }

        Debug.Log("Memulai Wave " + currentWaveNumber + " | Memunculkan " + enemiesToSpawn + " Musuh");
        enemiesRemainingInWave = enemiesToSpawn;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnRandomEnemy();
        }
    }

    void SpawnRandomEnemy()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0 || gameIsOver) return;

        int randomEnemyIndex = Random.Range(0, enemyPrefabs.Length);
        int randomPointIndex = Random.Range(0, spawnPoints.Length);

        GameObject selectedEnemyPrefab = enemyPrefabs[randomEnemyIndex];
        Transform selectedSpawnPoint = spawnPoints[randomPointIndex];

        GameObject spawnedEnemy = Instantiate(selectedEnemyPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
        
        EnemyController enemyScript = spawnedEnemy.GetComponent<EnemyController>();
        if (enemyScript != null)
        {
            enemyScript.RegisterSpawner(this);
        }
    }

    public void OnEnemyDefeated()
    {
        if (gameIsOver) return;

        totalEnemiesKilled++;
        enemiesRemainingInWave--;

        // Setiap ada musuh mati, perbarui teks hitungan secara realtime
        UpdateObjectiveUI();

        // Cek langsung: Apakah kematian musuh ini menyentuh target kemenangan?
        if (totalEnemiesKilled >= totalTargetKills)
        {
            WinGame();
            return; // Keluar langsung
        }

        // Jika musuh di wave ini habis, lanjut ke wave berikutnya
        if (enemiesRemainingInWave <= 0 && !isWaitingForNextWave)
        {
            currentWaveNumber++;
            StartCoroutine(StartNextWaveWithTimer(false)); 
        }
    }

    void UpdateObjectiveUI()
    {
        if (objectiveText != null)
        {
            objectiveText.text = "Kill The Enemies! (" + totalEnemiesKilled + "/" + totalTargetKills + ")";
        }
    }

    void WinGame()
    {
        if (gameIsOver) return; // Mencegah pemanggilan ganda

        gameIsOver = true; // Kunci status game selesai!
        StopAllCoroutines(); // Paksa matikan seluruh hitung mundur wave

        if (objectiveText != null)
        {
            objectiveText.text = "STAGE CLEAR!";
        }
        
        // FIX BARU: Panggil panel kemenangan secara resmi di sini!
        if (uiManager != null)
        {
            uiManager.SetupWinning();
        }
        else
        {
            Debug.LogError("uiManager (GameOverManager) belum di-drag ke slot Spawner di Inspector!");
        }

        Debug.Log("Selamat! Kamu Menang!");
    }
}