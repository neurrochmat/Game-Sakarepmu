using UnityEngine;

public class SpawnKarakter2D : MonoBehaviour
{
    [Header("Daftar Prefab Karakter")]
    public GameObject[] daftarKarakter; // Drag semua prefab ke sini

    [Header("Titik Spawn")]
    public Transform Player1;
    public Transform Player2;

    [Header("Mode AI")]
    public bool cekModeAI = true;    // Cek apakah mode AI aktif
    public bool spawnAIController = true; // Apakah ingin otomatis tambahkan AIController

    // Index default untuk prefab
    [Header("Pengaturan Default")]
    [Tooltip("Index prefab default yang akan digunakan jika karakter tidak ditemukan")]
    public int defaultIndex = 6;

    void Start()
    {
        // Ambil data pemilihan karakter
        string namaKarakter1 = PlayerPrefs.GetString("Player1", "");
        string namaKarakter2 = PlayerPrefs.GetString("Player2", "");

        // Ambil mode permainan
        string modeMain = "";
        if (cekModeAI)
        {
            modeMain = PlayerPrefs.GetString("ModeMain", "Player");
        }

        Debug.Log($"Memuat karakter - Player1: {namaKarakter1}, Player2: {namaKarakter2}, Mode: {modeMain}");

        // Cari prefab berdasarkan nama
        GameObject prefab1 = CariPrefab(namaKarakter1);
        GameObject prefab2 = CariPrefab(namaKarakter2);

        Vector3 posisiPlayer1 = Player1 != null ? Player1.position : new Vector3(-5, 0, 0);
        Vector3 posisiPlayer2 = Player2 != null ? Player2.position : new Vector3(5, 0, 0);

        // === Spawn Player 1 ===
        if (prefab1 != null)
        {
            GameObject player1Instance = Instantiate(prefab1, posisiPlayer1, Quaternion.identity);
            player1Instance.name = "Player1";
            Debug.Log("Player1 di-spawn di posisi: " + posisiPlayer1);

            AturPaddleController(
                player1Instance,
                "Vertical1",
                "Horizontal1",
                kecepatan: 5f,
                batasAtas: 3f,
                batasBawah: -3f,
                batasKiri: -7f,
                batasKanan: -1f
            );
        }
        else
        {
            Debug.LogError("Gagal spawn Player1: Prefab tidak ditemukan!");
        }

        // === Spawn Player 2 atau AI ===
        if (modeMain == "AI" && spawnAIController)
        {
            // Mode lawan AI - spawn karakter dengan AI Controller
            if (prefab2 != null)
            {
                GameObject player2Instance = Instantiate(prefab2, posisiPlayer2, Quaternion.identity);
                player2Instance.name = "AIPlayer";
                Debug.Log("AIPlayer di-spawn di posisi: " + posisiPlayer2);

                // Cek apakah sudah memiliki AIPaddleController, jika tidak tambahkan
                if (player2Instance.GetComponent<AIPaddleController>() == null)
                {
                    AIPaddleController aiPaddle = player2Instance.AddComponent<AIPaddleController>();

                    // Konfigurasi AI Paddle
                    aiPaddle.axisVertikal = "";  // Nonaktifkan input player
                    aiPaddle.axisHorizontal = "";
                    aiPaddle.kecepatan = 5f;
                    aiPaddle.batasAtas = 2.8f;
                    aiPaddle.batasBawah = -2.8f;
                    aiPaddle.batasKiri = 2f;
                    aiPaddle.batasKanan = 7f;

                    // Cari bola
                    aiPaddle.bola = GameObject.FindGameObjectWithTag("bola") ?? GameObject.Find("bola");

                    // Set pengaturan berdasarkan kesulitan (default atau dari PlayerPrefs)
                    float aiDifficulty = PlayerPrefs.GetFloat("AIDifficulty", 0.5f);
                    aiPaddle.kecepatan = Mathf.Lerp(3f, 7f, aiDifficulty);
                    aiPaddle.reaksiDelay = Mathf.Lerp(0.5f, 0.05f, aiDifficulty);
                    aiPaddle.prediksiJarak = Mathf.Lerp(1f, 3f, aiDifficulty);
                    aiPaddle.akurasi = Mathf.Lerp(0.6f, 0.98f, aiDifficulty);
                    aiPaddle.randomOffset = Mathf.Lerp(1f, 0.1f, aiDifficulty);

                    // Tambahkan pengaturan untuk mode mengejar bola
                    aiPaddle.modeMengejarBola = true;  // Aktifkan mode mengejar
                    aiPaddle.jangkauanKejaran = Mathf.Lerp(2f, 4f, aiDifficulty);
                    aiPaddle.prioritasKejaran = Mathf.Lerp(0.5f, 0.9f, aiDifficulty);
                    aiPaddle.kecepatanHorizontal = Mathf.Lerp(3f, 6f, aiDifficulty);

                    Debug.Log("Menambahkan AIPaddleController ke AI Player dengan mode mengejar bola");
                }
                else
                {
                    Debug.Log("AIPlayer sudah memiliki AIPaddleController");
                }

                // Nonaktifkan PaddleController reguler jika ada
                PaddleController regController = player2Instance.GetComponent<PaddleController>();
                if (regController != null && !(regController is AIPaddleController))
                {
                    regController.enabled = false;
                    Debug.Log("Menonaktifkan PaddleController standar pada AIPlayer");
                }
            }
            else
            {
                Debug.LogError("Gagal spawn AIPlayer: Prefab tidak ditemukan!");
            }
        }
        else
        {
            // Mode normal lawan Player 2
            if (!string.IsNullOrEmpty(namaKarakter2) && prefab2 != null)
            {
                GameObject player2Instance = Instantiate(prefab2, posisiPlayer2, Quaternion.identity);
                player2Instance.name = "Player2";
                Debug.Log("Player2 di-spawn di posisi: " + posisiPlayer2);

                AturPaddleController(
                    player2Instance,
                    "Vertical2",
                    "Horizontal2",
                    kecepatan: 5f,
                    batasAtas: 3f,
                    batasBawah: -3f,
                    batasKiri: 1f,
                    batasKanan: 7f
                );
            }
            else
            {
                Debug.LogWarning("Player2 tidak di-spawn: Nama karakter kosong atau prefab tidak ditemukan");
            }
        }

        // Opsional: hapus penanda spawn agar tidak mengganggu
        // if (Player1 != null && Player1.gameObject != this.gameObject) Destroy(Player1.gameObject);
        // if (Player2 != null && Player2.gameObject != this.gameObject) Destroy(Player2.gameObject);
    }

    void AturPaddleController(GameObject obj, string axisV, string axisH, float kecepatan, float batasAtas, float batasBawah, float batasKiri, float batasKanan)
    {
        PaddleController paddle = obj.GetComponent<PaddleController>();
        if (paddle == null)
        {
            paddle = obj.AddComponent<PaddleController>();
            Debug.Log($"Menambahkan PaddleController ke {obj.name}");
        }

        paddle.axisVertikal = axisV;
        paddle.axisHorizontal = axisH;
        paddle.kecepatan = kecepatan;
        paddle.batasAtas = batasAtas;
        paddle.batasBawah = batasBawah;
        paddle.batasKiri = batasKiri;
        paddle.batasKanan = batasKanan;

        Debug.Log($"{obj.name} - PaddleController dikonfigurasi: AxisV={axisV}, AxisH={axisH}, Batas=[{batasKiri}, {batasKanan}], [{batasBawah}, {batasAtas}]");
    }

    GameObject CariPrefab(string nama)
    {
        // Pastikan defaultIndex dalam batas yang valid
        if (defaultIndex >= daftarKarakter.Length || defaultIndex < 0)
        {
            Debug.LogWarning("Default index tidak valid. Menggunakan index 6 jika tersedia.");
            defaultIndex = daftarKarakter.Length > 6 ? 6 : 0;
        }

        // Jika nama kosong, langsung gunakan prefab default dari index 6
        if (string.IsNullOrEmpty(nama))
        {
            Debug.LogWarning("Nama karakter kosong. Menggunakan karakter default dari index " + defaultIndex);

            // Pastikan array memiliki element
            if (daftarKarakter.Length > 0)
            {
                // Prioritaskan selalu menggunakan index 6 jika tersedia
                if (daftarKarakter.Length > defaultIndex && daftarKarakter[defaultIndex] != null)
                {
                    return daftarKarakter[defaultIndex];
                }
                else
                {
                    Debug.LogWarning("Prefab pada index " + defaultIndex + " tidak tersedia. Menggunakan index 0.");
                    return daftarKarakter[0];
                }
            }
            else
            {
                Debug.LogError("Daftar karakter kosong!");
                return null;
            }
        }

        // Cari prefab berdasarkan nama
        foreach (var prefab in daftarKarakter)
        {
            if (prefab != null && prefab.name == nama)
                return prefab;
        }

        // Jika tidak ditemukan, gunakan prefab default dari index 6
        Debug.LogWarning("Prefab '" + nama + "' tidak ditemukan. Menggunakan karakter default dari index " + defaultIndex);

        // Prioritaskan selalu menggunakan index 6 jika tersedia
        if (daftarKarakter.Length > defaultIndex && daftarKarakter[defaultIndex] != null)
        {
            return daftarKarakter[defaultIndex];
        }
        else if (daftarKarakter.Length > 0)
        {
            Debug.LogWarning("Prefab pada index " + defaultIndex + " tidak tersedia. Menggunakan index 0.");
            return daftarKarakter[0];
        }
        else
        {
            Debug.LogError("Daftar karakter kosong!");
            return null;
        }
    }
}