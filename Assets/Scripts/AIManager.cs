using UnityEngine;
using UnityEngine.UI;

public class AIManager : MonoBehaviour
{
    [Header("Mode AI")]
    public bool isAIActive = false;
    public GameObject aiPlayerPrefab;    // Prefab karakter untuk AI
    public Transform aiSpawnPoint;       // Posisi spawn AI
    
    [Header("Pengaturan AI")]
    [Range(0f, 1f)]
    public float aiDifficulty = 0.5f;    // 0 = Mudah, 1 = Sulit
    public Slider difficultySlider;      // Optional: UI slider untuk mengatur kesulitan
    
    [Header("Tingkat Kesulitan Preset")]
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    
    private GameObject aiInstance;
    private AIController aiController;

    private void Awake()
    {
        // Setup UI event listeners jika ada
        if (difficultySlider != null)
        {
            difficultySlider.value = aiDifficulty;
            difficultySlider.onValueChanged.AddListener(SetAIDifficulty);
        }
        
        if (easyButton != null)
            easyButton.onClick.AddListener(() => SetDifficultyPreset(0.25f));
        
        if (mediumButton != null)
            mediumButton.onClick.AddListener(() => SetDifficultyPreset(0.5f));
        
        if (hardButton != null)
            hardButton.onClick.AddListener(() => SetDifficultyPreset(0.85f));
    }

    private void Start()
    {
        // Cek mode permainan dari PlayerPrefs
        string modeMain = PlayerPrefs.GetString("ModeMain", "Player");
        isAIActive = (modeMain == "AI");
        
        // Load tingkat kesulitan yang tersimpan
        aiDifficulty = PlayerPrefs.GetFloat("AIDifficulty", 0.5f);
        
        if (difficultySlider != null)
            difficultySlider.value = aiDifficulty;
        
        if (isAIActive)
        {
            // Tunggu sebentar untuk memastikan semua komponen lain sudah terinisialisasi
            Invoke("SpawnAIPlayer", 0.2f);
        }
    }
    
    public void SetDifficultyPreset(float difficultyValue)
    {
        // Set difficulty dan simpan ke PlayerPrefs
        aiDifficulty = difficultyValue;
        PlayerPrefs.SetFloat("AIDifficulty", aiDifficulty);
        
        if (difficultySlider != null)
            difficultySlider.value = aiDifficulty;
            
        SetAIDifficulty(aiDifficulty);
    }
    
    public void SpawnAIPlayer()
    {
        if (aiSpawnPoint == null)
        {
            aiSpawnPoint = GameObject.Find("Player2")?.transform;
            
            if (aiSpawnPoint == null)
            {
                Debug.LogError("Titik spawn AI tidak ditemukan! Coba mencari dengan nama lain...");
                var spawnPoints = GameObject.FindObjectsOfType<Transform>();
                foreach (var point in spawnPoints)
                {
                    if (point.name.Contains("Player2") || point.name.Contains("P2") || 
                        point.name.Contains("AISpawn") || point.name.Contains("RightPlayer"))
                    {
                        aiSpawnPoint = point;
                        Debug.Log("Menemukan titik spawn alternatif: " + point.name);
                        break;
                    }
                }
                
                if (aiSpawnPoint == null)
                {
                    Debug.LogError("Tidak dapat menemukan titik spawn AI! Menggunakan posisi default.");
                    aiSpawnPoint = transform; // Gunakan posisi objek ini sebagai fallback
                }
            }
        }
        
        // Cek apakah AI sudah di-spawn oleh SpawnKarakter2D
        GameObject existingAI = GameObject.Find("Player2") ?? GameObject.Find("AIPlayer");
        if (existingAI != null)
        {
            // Jika sudah ada, tambahkan AI Controller saja
            aiInstance = existingAI;
            Debug.Log("Menggunakan karakter AI yang sudah di-spawn: " + existingAI.name);
        }
        else
        {
            // Gunakan prefab karakter yang dipilih untuk AI
            string aiKarakterNama = PlayerPrefs.GetString("Player2", "");
            GameObject aiKarakterPrefab = aiPlayerPrefab;
            
            // Jika ada karakter khusus yang dipilih untuk AI
            if (!string.IsNullOrEmpty(aiKarakterNama))
            {
                SpawnKarakter2D spawnManager = FindObjectOfType<SpawnKarakter2D>();
                if (spawnManager != null && spawnManager.daftarKarakter != null && spawnManager.daftarKarakter.Length > 0)
                {
                    foreach (GameObject karakter in spawnManager.daftarKarakter)
                    {
                        if (karakter.name == aiKarakterNama)
                        {
                            aiKarakterPrefab = karakter;
                            Debug.Log("Menggunakan karakter khusus untuk AI: " + aiKarakterNama);
                            break;
                        }
                    }
                }
            }
            
            // Jika tidak ada prefab yang valid, gunakan default
            if (aiKarakterPrefab == null)
            {
                Debug.LogWarning("Prefab AI tidak ditemukan, menggunakan default");
                
                // Cari prefab default pertama jika tersedia
                SpawnKarakter2D spawnManager = FindObjectOfType<SpawnKarakter2D>();
                if (spawnManager != null && spawnManager.daftarKarakter != null && spawnManager.daftarKarakter.Length > 0)
                {
                    aiKarakterPrefab = spawnManager.daftarKarakter[0];
                }
                else if (aiPlayerPrefab != null)
                {
                    aiKarakterPrefab = aiPlayerPrefab;
                }
                else
                {
                    Debug.LogError("Tidak ada prefab karakter yang tersedia untuk AI!");
                    return;
                }
            }
            
            // Spawn AI
            aiInstance = Instantiate(aiKarakterPrefab, aiSpawnPoint.position, Quaternion.identity);
            aiInstance.name = "AIPlayer";
        }
        
        // Cek apakah sudah memiliki AIController atau AIPaddleController
        aiController = aiInstance.GetComponent<AIController>();
        AIPaddleController aiPaddleController = aiInstance.GetComponent<AIPaddleController>();
        
        // Pilih salah satu controller (prioritaskan AIPaddleController jika ada)
        if (aiPaddleController != null)
        {
            // Setup AIPaddleController
            aiPaddleController.bola = GameObject.FindGameObjectWithTag("bola") ?? GameObject.Find("bola");
            
            // Sesuaikan pengaturan berdasarkan tingkat kesulitan
            aiPaddleController.kecepatan = Mathf.Lerp(3f, 7f, aiDifficulty);
            aiPaddleController.reaksiDelay = Mathf.Lerp(0.5f, 0.05f, aiDifficulty);
            aiPaddleController.prediksiJarak = Mathf.Lerp(1f, 3f, aiDifficulty);
            aiPaddleController.akurasi = Mathf.Lerp(0.6f, 0.98f, aiDifficulty);
            aiPaddleController.randomOffset = Mathf.Lerp(1f, 0.1f, aiDifficulty);
            
            // Atur batasan gerak
            aiPaddleController.batasAtas = 2.8f;
            aiPaddleController.batasBawah = -2.8f;
            aiPaddleController.batasKiri = 3f;
            aiPaddleController.batasKanan = 7f;
            
            Debug.Log("Menggunakan AIPaddleController untuk AI");
        }
        else
        {
            // Jika belum ada AIController, tambahkan
            if (aiController == null)
            {
                aiController = aiInstance.AddComponent<AIController>();
                Debug.Log("Menambahkan AIController ke karakter AI");
            }
            
            // Cari referensi bola
           // Cari referensi bola
            GameObject bolaObj = GameObject.FindGameObjectWithTag("bola") ?? GameObject.Find("bola");
            aiController.bola = bolaObj;
            
            // Periksa apakah ada BallController pada bola
            if (bolaObj != null && bolaObj.GetComponent<BallController>() == null)
            {
                Debug.LogWarning("Bola tidak memiliki BallController! AI mungkin tidak akan berfungsi dengan baik.");
            }
            
            // Set pengaturan AI berdasarkan tingkat kesulitan
            SetAIDifficulty(aiDifficulty);
            
            // Atur batasan gerak
            aiController.batasAtas = 2.8f;
            aiController.batasBawah = -2.8f;
            aiController.batasKiri = 3f;
            aiController.batasKanan = 7f;
        }
        
        Debug.Log("AI Player berhasil dikonfigurasi");
    }
    
    // Tambahkan pada method SetAIDifficulty di AIManager.cs
// Cari bagian dimana kita mengatur AIPaddleController

    public void SetAIDifficulty(float difficulty)
    {
        // Pastikan nilai kesulitan dalam rentang 0-1
        difficulty = Mathf.Clamp01(difficulty);
        aiDifficulty = difficulty;
        
        // Simpan ke PlayerPrefs
        PlayerPrefs.SetFloat("AIDifficulty", aiDifficulty);
        
        if (aiController != null)
        {
            // Sesuaikan pengaturan AI berdasarkan tingkat kesulitan
            aiController.kecepatan = Mathf.Lerp(3f, 7f, difficulty);       // Kecepatan 3-7
            aiController.reaksiDelay = Mathf.Lerp(0.5f, 0.05f, difficulty); // Delay reaksi 0.5-0.05
            aiController.prediksiJarak = Mathf.Lerp(1f, 3f, difficulty);   // Prediksi jarak 1-3
            aiController.akurasi = Mathf.Lerp(0.6f, 0.98f, difficulty);    // Akurasi 60%-98%
            aiController.randomOffset = Mathf.Lerp(1f, 0.1f, difficulty);  // Offset 1-0.1
            
            Debug.Log($"AI diatur ke tingkat kesulitan: {difficulty} " +
                    $"(Kecepatan={aiController.kecepatan}, " +
                    $"Delay={aiController.reaksiDelay}, " +
                    $"Akurasi={aiController.akurasi})");
        }
        
        // Update juga AIPaddleController jika ada
        AIPaddleController aiPaddleController = aiInstance?.GetComponent<AIPaddleController>();
        if (aiPaddleController != null)
        {
            aiPaddleController.kecepatan = Mathf.Lerp(3f, 7f, difficulty);
            aiPaddleController.reaksiDelay = Mathf.Lerp(0.5f, 0.05f, difficulty);
            aiPaddleController.prediksiJarak = Mathf.Lerp(1f, 3f, difficulty);
            aiPaddleController.akurasi = Mathf.Lerp(0.6f, 0.98f, difficulty);
            aiPaddleController.randomOffset = Mathf.Lerp(1f, 0.1f, difficulty);
            
            // Tambahkan pengaturan untuk mode mengejar bola
            aiPaddleController.modeMengejarBola = true;  // Aktifkan mode mengejar
            aiPaddleController.jangkauanKejaran = Mathf.Lerp(2f, 4f, difficulty);  // Jangkauan kejaran: 2-4
            aiPaddleController.prioritasKejaran = Mathf.Lerp(0.5f, 0.9f, difficulty);  // Prioritas: 0.5-0.9
            aiPaddleController.kecepatanHorizontal = Mathf.Lerp(3f, 6f, difficulty);  // Kecepatan horizontal: 3-6
            
            Debug.Log($"Mode Mengejar Bola diaktifkan untuk AI dengan jangkauan={aiPaddleController.jangkauanKejaran}, " +
                    $"prioritas={aiPaddleController.prioritasKejaran}");
        }
    }
}