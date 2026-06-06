using UnityEngine;

public class WaveEnemySpawner : MonoBehaviour
{
    [Header("Pool Musuh & Lokasi")]
    public GameObject[] enemyPrefabs; 
    public Transform[] spawnPoints;     

    [Header("Pengaturan Target Game")]
    public int totalTargetKills = 15;   // Batas maksimum total kill untuk menang
    
    // Status Tracker
    private int totalEnemiesKilled = 0; 
    private int currentWaveNumber = 1;
    private int enemiesRemainingInWave = 0;

    void Start()
    {
        // Memulai Wave pertama saat game start
        StartNextWave();
    }

    void StartNextWave()
    {
        // Cek apakah target 15 kill sudah tercapai
        if (totalEnemiesKilled >= totalTargetKills)
        {
            WinGame();
            return;
        }

        // Menentukan jumlah musuh yang spawn di wave ini berdasarkan nomor wave
        // Misal: Wave 1 = 1 musuh, Wave 2 = 2 musuh, Wave 3 = 3 musuh, dst.
        int enemiesToSpawn = currentWaveNumber;

        // Jaga agar jumlah musuh tidak melebihi sisa target menuju 15 kill
        int remainingToTarget = totalTargetKills - totalEnemiesKilled;
        if (enemiesToSpawn > remainingToTarget)
        {
            enemiesToSpawn = remainingToTarget;
        }

        Debug.Log("Memulai Wave " + currentWaveNumber + " | Memunculkan " + enemiesToSpawn + " Musuh");

        // Set jumlah musuh aktif yang harus dihabisi di wave ini
        enemiesRemainingInWave = enemiesToSpawn;

        // Lakukan perulangan untuk men-spawn musuh sesuai jumlahnya
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnRandomEnemy();
        }
    }

    void SpawnRandomEnemy()
{
    if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;

    // 1. Acak jenis musuh dan lokasi spawn
    int randomEnemyIndex = Random.Range(0, enemyPrefabs.Length);
    int randomPointIndex = Random.Range(0, spawnPoints.Length);

    GameObject selectedEnemyPrefab = enemyPrefabs[randomEnemyIndex];
    Transform selectedSpawnPoint = spawnPoints[randomPointIndex];

    // 2. KUNCI UTAMA: Kita pakai Instantiate tapi posisinya di-set terpisah 
    // agar skrip terhubung dulu sebelum musuh melakukan kalkulasi fisik di game
    GameObject spawnedEnemy = Instantiate(selectedEnemyPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
    
    // Hubungkan spawner ke musuh yang baru lahir
    EnemyController enemyScript = spawnedEnemy.GetComponent<EnemyController>();
    if (enemyScript != null)
    {
        enemyScript.RegisterSpawner(this);
    }
    else
    {
        Debug.LogError("Waduh! Prefab musuhmu belum dipasang skrip 'EnemyController' nih!");
    }
}

    // Fungsi ini akan dipanggil oleh musuh sesaat sebelum mereka hancur/mati
    public void OnEnemyDefeated()
    {
        totalEnemiesKilled++;
        enemiesRemainingInWave--;

        Debug.Log("Musuh Mati! Total Kill: " + totalEnemiesKilled + "/" + totalTargetKills);

        // Jika semua musuh di wave ini sudah mati, lanjut ke wave berikutnya
        if (enemiesRemainingInWave <= 0)
        {
            currentWaveNumber++;
            // Beri sedikit jeda waktu sebelum wave berikutnya muncul (misal 1.5 detik)
            Invoke("StartNextWave", 1.5f); 
        }
    }

    void WinGame()
    {
        Debug.Log("Selamat! Kamu berhasil mengalahkan 15 musuh dan memenangkan pertandingan!");
        // Tambahkan logika menang di sini (misal munculin UI menang atau pindah scene)
    }
}