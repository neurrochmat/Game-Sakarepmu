using UnityEngine;

public class AIController : MonoBehaviour
{
    [Header("Referensi Bola")]
    public GameObject bola;
    private BallController bolaController;
    private Rigidbody2D bolaRigidbody;

    [Header("Pengaturan AI")]
    public float kecepatan = 5f;
    public float reaksiDelay = 0.1f;    // Waktu reaksi AI (semakin tinggi nilai, semakin lambat reaksi)
    public float prediksiJarak = 2.0f;   // Seberapa jauh AI akan memprediksi pergerakan bola
    public float akurasi = 0.9f;         // Nilai 0-1, semakin rendah semakin tidak akurat
    public float randomOffset = 0.5f;    // Penambahan offset acak pada posisi target
    public float batasAtas = 2.8f;
    public float batasBawah = -2.8f;
    public float batasKiri = 3f;
    public float batasKanan = 7f;

    private Vector3 targetPos;
    private Vector3 lastBallPos;
    private float lastDecisionTime;
    private float randomTargetOffset;

    private void Start()
    {
        if (bola == null)
        {
            bola = GameObject.FindGameObjectWithTag("Ball");
            if (bola == null)
            {
                bola = GameObject.Find("Ball");
                if (bola == null)
                {
                    Debug.LogError("Bola tidak ditemukan! Pastikan bola memiliki tag 'Ball' atau nama 'Ball'");
                    enabled = false;
                    return;
                }
            }
        }

        // Dapatkan komponen BallController dan Rigidbody2D dari bola
        bolaController = bola.GetComponent<BallController>();
        bolaRigidbody = bola.GetComponent<Rigidbody2D>();

        if (bolaRigidbody == null)
        {
            Debug.LogWarning("Rigidbody2D pada bola tidak ditemukan. AI mungkin tidak akan berfungsi dengan baik.");
        }

        lastBallPos = bola.transform.position;
        lastDecisionTime = Time.time;
        randomTargetOffset = Random.Range(-randomOffset, randomOffset);

        // Tambahkan PaddleController jika belum ada
        if (GetComponent<PaddleController>() == null)
        {
            PaddleController controller = gameObject.AddComponent<PaddleController>();
            controller.batasAtas = batasAtas;
            controller.batasBawah = batasBawah;
            controller.batasKiri = batasKiri;
            controller.batasKanan = batasKanan;
            controller.kecepatan = kecepatan;
            
            // Nonaktifkan input controller karena kita menggunakan AI
            controller.axisVertikal = "";
            controller.axisHorizontal = "";
        }
    }

    private void Update()
    {
        if (bola == null)
            return;

        // Perbarui target hanya setelah delay reaksi
        if (Time.time - lastDecisionTime > reaksiDelay)
        {
            // Ambil arah dan kecepatan bola
            Vector3 bolaPos = bola.transform.position;
            Vector3 bolaVelocity;
            
            // Gunakan kecepatan dari Rigidbody2D jika tersedia
            if (bolaRigidbody != null)
            {
                bolaVelocity = bolaRigidbody.linearVelocity;
            }
            else
            {
                // Jika tidak ada Rigidbody2D, gunakan cara lama
                bolaVelocity = (bolaPos - lastBallPos) / Time.deltaTime;
            }
            
            lastBallPos = bolaPos;

            // Cek apakah bola menuju ke arah kita
            if (IsBolaMendekat(bolaPos, bolaVelocity))
            {
                // Prediksi di mana bola akan berada
                targetPos = PrediksiPosisiBola(bolaPos, bolaVelocity);
                
                // Tambahkan sedikit ketidakakuratan
                if (Random.value > akurasi)
                {
                    randomTargetOffset = Random.Range(-randomOffset, randomOffset);
                }
                
                targetPos.y += randomTargetOffset;
                
                // Batasi target dalam batas yang diizinkan
                targetPos.y = Mathf.Clamp(targetPos.y, batasBawah, batasAtas);
            }
            
            lastDecisionTime = Time.time;
        }

        // Gerakkan paddle AI
        MoveTowardsTarget();
    }

    private bool IsBolaMendekat(Vector3 bolaPos, Vector3 bolaVelocity)
    {
        // Cek apakah bola bergerak ke arah kita 
        // Asumsi: AI ada di sebelah kanan (Player 2)
        if (transform.position.x > 0)
        {
            return bolaVelocity.x > 0;
        }
        // Atau jika AI di sebelah kiri (Player 1)
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
        
        // Batasi posisi paddle dalam range yang diizinkan
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, batasBawah, batasAtas);
        pos.x = Mathf.Clamp(pos.x, batasKiri, batasKanan);
        transform.position = pos;
    }

    // Untuk debugging
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
        }
    }
}