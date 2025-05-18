using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BallController : MonoBehaviour
{
    public int force;
    Rigidbody2D rigid;
    int scoreP1;
    int scoreP2;
    Text scoreUIP1;
    Text scoreUIP2;
    GameObject panelSelesai;
    Text txPemenang;

    public AudioClip startSound;
    public AudioClip hitSound;
    public AudioClip goalSound;
    public AudioClip gameOverSound;
    public AudioClip warningSound;  // Suara peringatan 10 detik
    private AudioSource audio;

    public Text waktuText;
    public float totalWaktu = 60f; // durasi permainan dalam detik
    private float sisaWaktu;
    private bool gameSelesai = false;
    private bool warningPlayed = false; // Flag untuk memastikan suara hanya diputar sekali

    public LayerMask playerLayer;
    public float spawnCheckRadius = 1f;
    private Vector2 defaultSpawnPosition = new Vector2(0, 0);
    private bool isResetting = false;

    public float topBoundary = 4.5f;
    public float bottomBoundary = -4.5f;
    public float leftBoundary = -8.5f;
    public float rightBoundary = 8.5f;

    public float bounceForce = 10f;

    private float stuckTimer = 0f;
    private const float MAX_STUCK_TIME = 1.0f;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        Vector2 arah = new Vector2(2, 0).normalized;
        rigid.AddForce(arah * force);

        scoreP1 = 0;
        scoreP2 = 0;
        scoreUIP1 = GameObject.Find("Score1").GetComponent<Text>();
        scoreUIP2 = GameObject.Find("Score2").GetComponent<Text>();

        panelSelesai = GameObject.Find("PanelSelesai");
        panelSelesai.SetActive(false);

        audio = GetComponent<AudioSource>();
        if (startSound != null) audio.PlayOneShot(startSound);

        sisaWaktu = totalWaktu;
    }

    void Update()
    {
        if (gameSelesai) return;

        // Timer permainan
        sisaWaktu -= Time.deltaTime;
        if (sisaWaktu < 0) sisaWaktu = 0;

        int menit = Mathf.FloorToInt(sisaWaktu / 60);
        int detik = Mathf.FloorToInt(sisaWaktu % 60);
        waktuText.text = string.Format("{0:00}:{1:00}", menit, detik);

        // Animasi waktu berkedip jika tinggal 10 detik
        if (sisaWaktu <= 11f)
        {
            Color color = (Mathf.FloorToInt(sisaWaktu) % 2 == 0) ? Color.red : Color.black;
            waktuText.color = color;
        }

        // Mainkan suara peringatan saat waktu tersisa 3 detik
        if (!warningPlayed && sisaWaktu <= 3f)
        {
            if (warningSound != null)
            {
                audio.PlayOneShot(warningSound);
            }
            warningPlayed = true;
        }

        if (sisaWaktu <= 0 && !gameSelesai)
        {
            // Penentuan pemenang berdasarkan skor saat waktu habis
            if (scoreP1 > scoreP2)
            {
                GameOver("Player 1 Menang!");
            }
            else if (scoreP2 > scoreP1)
            {
                GameOver("Player 2 Menang!");
            }
            else
            {
                GameOver("Seri!");
            }
        }

        // Paksa agar bola tetap dalam area
        EnforceBoundaries();

        // Deteksi jika bola berhenti
        if (!isResetting && rigid.linearVelocity.magnitude < 0.1f)
        {
            Vector2 direction = Random.value > 0.5f ? Vector2.right : Vector2.left;
            rigid.AddForce(direction * force);
        }
    }

    void FixedUpdate()
    {
        if (!isResetting && rigid.linearVelocity.magnitude < 0.5f)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer > MAX_STUCK_TIME)
            {
                FixStuckBall();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        // Batas kecepatan
        if (rigid.linearVelocity.magnitude > force * 2)
        {
            rigid.linearVelocity = rigid.linearVelocity.normalized * force * 2;
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (hitSound != null) audio.PlayOneShot(hitSound);

        if (coll.gameObject.name == "TepiKanan")
        {
            scoreP1++;
            TampilkanScore();
            if (goalSound != null) audio.PlayOneShot(goalSound);

            if (scoreP1 == 5)
            {
                panelSelesai.SetActive(true);
                txPemenang = GameObject.Find("Pemenang").GetComponent<Text>();

                // Cek apakah mode permainan adalah mode AI
                string modeMain = PlayerPrefs.GetString("ModeMain", "Player");
                if (modeMain == "AI")
                {
                    txPemenang.text = "Anda Menang!";
                }
                else
                {
                    txPemenang.text = "Player 1 Pemenang!";
                }

                Destroy(gameObject);
                return;
            }
            StartCoroutine(ResetBallWithForce(Vector2.right));
        }
        else if (coll.gameObject.name == "TepiKiri")
        {
            scoreP2++;
            TampilkanScore();
            if (goalSound != null) audio.PlayOneShot(goalSound);

            if (scoreP2 == 5)
            {
                panelSelesai.SetActive(true);
                txPemenang = GameObject.Find("Pemenang").GetComponent<Text>();

                // Cek apakah mode permainan adalah mode AI
                string modeMain = PlayerPrefs.GetString("ModeMain", "Player");
                if (modeMain == "AI")
                {
                    txPemenang.text = "Anda Kalah!";
                }
                else
                {
                    txPemenang.text = "Player 2 Pemenang!";
                }

                Destroy(gameObject);
                return;
            }
            StartCoroutine(ResetBallWithForce(Vector2.left));
        }
    }

    void GameOver(string message)
    {
        gameSelesai = true;
        panelSelesai.SetActive(true);
        txPemenang = GameObject.Find("Pemenang").GetComponent<Text>();

        string modeMain = PlayerPrefs.GetString("ModeMain", "Player");

        if (modeMain == "AI")
        {
            if (scoreP1 > scoreP2)
                txPemenang.text = "Anda Menang!";
            else if (scoreP2 > scoreP1)
                txPemenang.text = "Anda Kalah!";
            else
                txPemenang.text = "Seri!";
        }
        else
        {
            txPemenang.text = message;
        }

        if (gameOverSound != null) audio.PlayOneShot(gameOverSound);
        Destroy(gameObject);
    }

    void TampilkanScore()
    {
        scoreUIP1.text = scoreP1.ToString();
        scoreUIP2.text = scoreP2.ToString();
    }

    IEnumerator ResetBallWithForce(Vector2 direction)
    {
        isResetting = true;

        transform.position = FindSafeSpawnPosition();
        rigid.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.1f);

        Collider2D[] obstacles = Physics2D.OverlapCircleAll(transform.position, spawnCheckRadius, playerLayer);
        foreach (var obstacle in obstacles)
        {
            Rigidbody2D rb = obstacle.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 pushDir = ((Vector2)obstacle.transform.position - (Vector2)transform.position).normalized;
                rb.AddForce(pushDir * force * 0.5f);
            }
        }

        rigid.AddForce(direction * force);

        yield return new WaitForSeconds(0.2f);

        if (rigid.linearVelocity.magnitude < 0.1f)
        {
            Vector2 randomDir = new Vector2(direction.x, Random.Range(-0.5f, 0.5f)).normalized;
            rigid.linearVelocity = Vector2.zero;
            rigid.AddForce(randomDir * force * 1.5f);
        }

        isResetting = false;
    }

    Vector2 FindSafeSpawnPosition()
    {
        if (IsSafePosition(defaultSpawnPosition)) return defaultSpawnPosition;

        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.5f;
            Vector2 testPos = defaultSpawnPosition + offset;
            if (IsSafePosition(testPos)) return testPos;
        }

        return defaultSpawnPosition;
    }

    bool IsSafePosition(Vector2 position)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, spawnCheckRadius, playerLayer);
        return colliders.Length == 0;
    }

    void FixStuckBall()
    {
        Vector2 dir = transform.position.x < 0 ? Vector2.right : Vector2.left;
        rigid.linearVelocity = Vector2.zero;
        rigid.AddForce(dir * force);
    }

    void EnforceBoundaries()
    {
        Vector2 pos = transform.position;
        bool changed = false;
        Vector2 normal = Vector2.zero;

        if (pos.y > topBoundary)
        {
            pos.y = topBoundary;
            normal += Vector2.down;
            changed = true;
        }
        else if (pos.y < bottomBoundary)
        {
            pos.y = bottomBoundary;
            normal += Vector2.up;
            changed = true;
        }

        if (pos.x < leftBoundary && rigid.linearVelocity.x < 0)
        {
            pos.x = leftBoundary + 0.1f;
            changed = true;
        }
        else if (pos.x > rightBoundary && rigid.linearVelocity.x > 0)
        {
            pos.x = rightBoundary - 0.1f;
            changed = true;
        }

        if (changed)
        {
            transform.position = pos;
            if (normal != Vector2.zero)
            {
                Vector2 reflected = Vector2.Reflect(rigid.linearVelocity, normal.normalized);
                rigid.linearVelocity = reflected;
                if (audio != null && hitSound != null)
                    audio.PlayOneShot(hitSound);
            }
        }
    }
}
