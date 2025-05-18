using UnityEngine;

public class AIPaddleController : PaddleController
{
    [Header("AI Settings")]
    public GameObject bola;
    public float prediksiJarak = 2.0f;
    public float reaksiDelay = 0.1f;
    public float akurasi = 0.9f;
    public float randomOffset = 0.5f;

    [Header("Mode Kejar Bola")]
    public bool modeMengejarBola = true;        // Aktifkan mode mengejar bola
    public float jangkauanKejaran = 3.0f;       // Jarak maksimal AI akan mengejar bola
    public float prioritasKejaran = 0.7f;       // 0-1, semakin tinggi semakin agresif mengejar
    public float kecepatanHorizontal = 4.0f;    // Kecepatan AI bergerak secara horizontal

    [Header("Gerakan Kembali")]
    public bool kembaliKeAwal = true;          // Aktifkan mode kembali ke posisi awal
    public float batasBalikTengah = 0f;        // Batas tengah lapangan (0 = garis tengah)
    public float kecepatanKembali = 3.0f;      // Kecepatan kembali ke posisi awal
    private Vector3 posisiAwal;               // Menyimpan posisi awal AI

    private Vector3 targetPos;
    private Vector3 lastBallPos;
    private float lastDecisionTime;
    private float randomTargetOffset;
    private Rigidbody2D bolaRigidbody;
    private BallController bolaController;
    private bool sedangKembali = false;       // Status apakah AI sedang kembali ke posisi awal

    // Override Start method dari PaddleController
    protected virtual void Start()
    {
        base.Start(); // Panggil Start dari PaddleController

        // Simpan posisi awal AI
        posisiAwal = transform.position;

        if (bola == null)
        {
            bola = GameObject.FindGameObjectWithTag("bola");
            if (bola == null)
            {
                bola = GameObject.Find("Ball");
            }
        }

        if (bola == null)
        {
            Debug.LogError("Bola tidak ditemukan! AI tidak akan berfungsi.");
            enabled = false;
            return;
        }

        // Dapatkan komponen BallController dari bola
        bolaController = bola.GetComponent<BallController>();
        if (bolaController == null)
        {
            Debug.LogWarning("BallController tidak ditemukan pada bola. AI akan menggunakan Rigidbody2D saja.");
        }

        // Ambil juga Rigidbody2D dari bola untuk akses langsung ke data kecepatan
        bolaRigidbody = bola.GetComponent<Rigidbody2D>();
        if (bolaRigidbody == null)
        {
            Debug.LogError("Rigidbody2D tidak ditemukan pada bola! AI mungkin tidak akan berfungsi dengan baik.");
            // Kita masih bisa melanjutkan dan menggunakan metode perhitungan manual
        }

        lastBallPos = bola.transform.position;
        lastDecisionTime = Time.time;
        randomTargetOffset = Random.Range(-randomOffset, randomOffset);

        // Nonaktifkan input player untuk AI
        axisVertikal = "";
        axisHorizontal = "";
    }

    // Override Update method dari PaddleController
    void Update()
    {
        if (bola == null) return;

        Vector3 bolaPos = bola.transform.position;

        // Periksa apakah bola telah melewati batas tengah
        bool bolaMelewatiBatas = false;

        // Jika AI berada di sisi kanan (x > 0)
        if (transform.position.x > 0)
        {
            bolaMelewatiBatas = bolaPos.x < batasBalikTengah;
        }
        // Jika AI berada di sisi kiri (x < 0)
        else
        {
            bolaMelewatiBatas = bolaPos.x > batasBalikTengah;
        }

        // Jika bola telah melewati batas tengah dan mode kembali aktif
        if (bolaMelewatiBatas && kembaliKeAwal)
        {
            sedangKembali = true;
            // Bergerak kembali ke posisi awal
            MoveTowardsHome();
            return; // Keluar dari fungsi karena tidak perlu memprediksi posisi bola
        }
        else
        {
            sedangKembali = false;
        }

        // Perbarui target hanya setelah delay reaksi
        if (Time.time - lastDecisionTime > reaksiDelay)
        {
            // Gunakan velocity dari rigidbody jika ada
            Vector3 bolaVelocity;

            if (bolaRigidbody != null)
            {
                bolaVelocity = bolaRigidbody.linearVelocity;
            }
            else
            {
                // Jika tidak ada rigidbody, hitung velocity secara manual
                bolaVelocity = (bolaPos - lastBallPos) / Time.deltaTime;
            }

            lastBallPos = bolaPos;

            // Jika mode mengejar bola aktif, tentukan target berdasarkan kombinasi prediksi dan posisi bola
            if (modeMengejarBola)
            {
                float jarakKeBola = Vector3.Distance(transform.position, bolaPos);

                // Jika bola mendekat atau dalam jangkauan kejaran, kejar bola
                if (IsBolaMendekat(bolaPos, bolaVelocity) || jarakKeBola < jangkauanKejaran)
                {
                    // Kombinasikan posisi bola aktual dengan posisi prediksi
                    Vector3 prediksiPos = PrediksiPosisiBola(bolaPos, bolaVelocity);

                    // Gunakan prioritasKejaran untuk menentukan seberapa banyak posisi aktual vs prediksi
                    targetPos = Vector3.Lerp(
                        prediksiPos,        // Posisi hasil prediksi
                        bolaPos,            // Posisi bola aktual 
                        prioritasKejaran    // Seberapa banyak mengejar posisi aktual
                    );

                    // Tambahkan sedikit ketidakakuratan
                    if (Random.value > akurasi)
                    {
                        randomTargetOffset = Random.Range(-randomOffset, randomOffset);
                    }

                    targetPos.y += randomTargetOffset;
                }
                else
                {
                    // Jika bola jauh, gunakan mode prediksi normal
                    targetPos = PrediksiPosisiBola(bolaPos, bolaVelocity);

                    if (Random.value > akurasi)
                    {
                        randomTargetOffset = Random.Range(-randomOffset, randomOffset);
                    }

                    targetPos.y += randomTargetOffset;
                }
            }
            else
            {
                // Mode prediksi original jika mode kejar bola dinonaktifkan
                if (IsBolaMendekat(bolaPos, bolaVelocity))
                {
                    targetPos = PrediksiPosisiBola(bolaPos, bolaVelocity);

                    if (Random.value > akurasi)
                    {
                        randomTargetOffset = Random.Range(-randomOffset, randomOffset);
                    }

                    targetPos.y += randomTargetOffset;
                }
            }

            // Batasi target dalam batas yang diizinkan
            targetPos.y = Mathf.Clamp(targetPos.y, batasBawah, batasAtas);
            targetPos.x = Mathf.Clamp(targetPos.x, batasKiri, batasKanan);

            lastDecisionTime = Time.time;
        }

        // Gerakkan paddle AI
        MoveTowardsTarget();

        // Jangan panggil base.Update() karena kita ingin mengganti logika gerakan
    }

    // Metode baru untuk bergerak kembali ke posisi awal
    private void MoveTowardsHome()
    {
        // Buat target dengan posisi X dari spawn, tapi Y tetap seperti sekarang
        Vector3 homeTarget = new Vector3(posisiAwal.x, transform.position.y, transform.position.z);

        // Gerakkan secara horizontal menuju posisi awal
        if (transform.position.x < homeTarget.x - 0.1f)
        {
            // Gerak ke kanan
            transform.position += new Vector3(kecepatanKembali * Time.deltaTime, 0, 0);
        }
        else if (transform.position.x > homeTarget.x + 0.1f)
        {
            // Gerak ke kiri
            transform.position -= new Vector3(kecepatanKembali * Time.deltaTime, 0, 0);
        }

        // Gerakkan juga secara vertikal untuk mendekati posisi Y tengah
        float targetY = (batasAtas + batasBawah) / 2.0f; // Posisi Y tengah antara batas atas dan bawah

        if (transform.position.y < targetY - 0.1f)
        {
            // Gerak ke atas
            transform.position += new Vector3(0, kecepatanKembali * Time.deltaTime, 0);
        }
        else if (transform.position.y > targetY + 0.1f)
        {
            // Gerak ke bawah
            transform.position -= new Vector3(0, kecepatanKembali * Time.deltaTime, 0);
        }

        // Batasi posisi paddle dalam range yang diizinkan
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, batasBawah, batasAtas);
        pos.x = Mathf.Clamp(pos.x, batasKiri, batasKanan);
        transform.position = pos;
    }

    private bool IsBolaMendekat(Vector3 bolaPos, Vector3 bolaVelocity)
    {
        // Cek apakah bola bergerak ke arah kita 
        // Asumsi: AI ada di sebelah kanan (x > 0)
        if (transform.position.x > 0)
        {
            return bolaVelocity.x > 0;
        }
        // Atau jika AI di sebelah kiri (x < 0)
        else
        {
            return bolaVelocity.x < 0;
        }
    }

    private Vector3 PrediksiPosisiBola(Vector3 bolaPos, Vector3 bolaVelocity)
    {
        // Prediksi posisi bola berdasarkan vektor kecepatan
        Vector3 prediksi = bolaPos;

        // Jarak AI ke bola berdasarkan sumbu x
        float jarakX = Mathf.Abs(transform.position.x - bolaPos.x);

        // Perkiraan waktu sampai bola mencapai paddle (dalam detik)
        float estimasiWaktu = jarakX / Mathf.Abs(bolaVelocity.x);

        // Prediksikan posisi bola secara vertikal
        prediksi.y = bolaPos.y + (bolaVelocity.y * estimasiWaktu * prediksiJarak);

        // Prediksikan posisi bola secara horizontal dengan offset sesuai prediksi
        // Ini memungkinkan AI maju mundur untuk mengantisipasi bola
        prediksi.x = bolaPos.x + (bolaVelocity.x * estimasiWaktu * prediksiJarak);

        // Batasi prediksi X dalam range yang diizinkan untuk AI
        prediksi.x = Mathf.Clamp(prediksi.x, batasKiri, batasKanan);

        // Hitung pantulan jika bola akan mengenai batas atas/bawah
        float maxTime = 5.0f; // Batas waktu prediksi (untuk menghindari loop tak terbatas)
        float elapsedTime = 0f;

        while (elapsedTime < maxTime)
        {
            // Jika prediksi melewati batas atas
            if (prediksi.y > batasAtas)
            {
                // Hitung sisa waktu setelah mengenai batas
                float timeToHitBorder = (batasAtas - bolaPos.y) / bolaVelocity.y;
                float remainingTime = estimasiWaktu - timeToHitBorder;

                // Pantulan arah
                bolaVelocity.y = -bolaVelocity.y;

                // Posisi baru setelah pantulan
                prediksi.y = batasAtas - (bolaVelocity.y * remainingTime);
            }
            // Jika prediksi melewati batas bawah
            else if (prediksi.y < batasBawah)
            {
                // Hitung sisa waktu setelah mengenai batas
                float timeToHitBorder = (batasBawah - bolaPos.y) / bolaVelocity.y;
                float remainingTime = estimasiWaktu - timeToHitBorder;

                // Pantulan arah
                bolaVelocity.y = -bolaVelocity.y;

                // Posisi baru setelah pantulan
                prediksi.y = batasBawah - (bolaVelocity.y * remainingTime);
            }
            else
            {
                // Prediksi sudah dalam batas, keluar dari loop
                break;
            }

            elapsedTime += estimasiWaktu;
        }

        // Kembalikan prediksi dengan posisi x dan y yang sudah dihitung
        return new Vector3(prediksi.x, prediksi.y, 0);
    }

    private void MoveTowardsTarget()
    {
        // Gerakkan paddle menuju posisi target yang diprediksi

        // Gerakan vertikal (atas/bawah)
        if (transform.position.y < targetPos.y - 0.1f)
        {
            // Gerak ke atas
            transform.position += new Vector3(0, kecepatan * Time.deltaTime, 0);
        }
        else if (transform.position.y > targetPos.y + 0.1f)
        {
            // Gerak ke bawah
            transform.position -= new Vector3(0, kecepatan * Time.deltaTime, 0);
        }

        // Gerakan horizontal (maju/mundur) jika mode mengejar bola aktif
        if (modeMengejarBola)
        {
            if (transform.position.x < targetPos.x - 0.1f)
            {
                // Gerak ke kanan
                transform.position += new Vector3(kecepatanHorizontal * Time.deltaTime, 0, 0);
            }
            else if (transform.position.x > targetPos.x + 0.1f)
            {
                // Gerak ke kiri
                transform.position -= new Vector3(kecepatanHorizontal * Time.deltaTime, 0, 0);
            }
        }

        // Batasi posisi paddle dalam range yang diizinkan
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, batasBawah, batasAtas);
        pos.x = Mathf.Clamp(pos.x, batasKiri, batasKanan);
        transform.position = pos;
    }

    // Untuk debugging, visualisasi prediksi
    private void OnDrawGizmos()
    {
        if (bola != null)
        {
            // Visualisasi target prediksi
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPos, 0.2f);

            // Visualisasi jalur prediksi
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPos);

            // Visualisasi jangkauan kejaran jika mode mengejar aktif
            if (modeMengejarBola && Application.isPlaying)
            {
                Gizmos.color = new Color(0, 1, 0, 0.2f);
                Gizmos.DrawWireSphere(transform.position, jangkauanKejaran);
            }

            // Visualisasi batas tengah
            if (kembaliKeAwal && Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(new Vector3(batasBalikTengah, batasAtas + 1, 0),
                               new Vector3(batasBalikTengah, batasBawah - 1, 0));
            }
        }
    }
}