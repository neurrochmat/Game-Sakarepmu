using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BatasAkhirSampah : MonoBehaviour
{
    private static int counter = 0;
    public int nyawa = 3;
    public Text poinHP;
    public Text poinWaktu;
    public float waktu = 60f;
    private bool isGameOver = false;
    private Coroutine blinkCoroutine;
    private Color defaultColor;
    private bool isBlinking = false;

    // Use this for initialization
    void Start()
    {
        if (poinHP == null)
        {
            poinHP = GameObject.Find("PoinHP").GetComponent<Text>();
        }
        UpdatePoinHP();

        if (poinWaktu == null)
        {
            poinWaktu = GameObject.Find("PoinWaktu").GetComponent<Text>();
        }
        UpdatePoinWaktu();

        if (poinWaktu != null)
        {
            defaultColor = poinWaktu.color;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver) return;
        waktu -= Time.deltaTime;
        if (waktu < 0f) waktu = 0f;
        UpdatePoinWaktu();
        if (waktu <= 0f)
        {
            GameOver();
        }
        // Mulai/berhenti berkedip jika waktu < 10 detik
        if (waktu < 10f && !isBlinking)
        {
            if (poinWaktu != null)
                blinkCoroutine = StartCoroutine(BlinkPoinWaktu());
            isBlinking = true;
        }
        else if (waktu >= 10f && isBlinking)
        {
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);
            if (poinWaktu != null)
                poinWaktu.color = defaultColor;
            isBlinking = false;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(collision.gameObject);
        nyawa--;
        UpdatePoinHP();
        if (nyawa <= 0 && !isGameOver)
        {
            GameOver();
        }
    }

    void UpdatePoinHP()
    {
        if (poinHP != null)
        {
            poinHP.text = nyawa+"x";
        }
    }

    void GameOver()
    {
        isGameOver = true;
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            if (poinWaktu != null)
                poinWaktu.color = defaultColor;
        }
        SceneManager.LoadScene("GameOver");
        // Hentikan spawn sampah
        MunculSampah spawner = FindObjectOfType<MunculSampah>();
        if (spawner != null)
        {
            spawner.enabled = false;
        }
        Time.timeScale = 0f;
    }

    public void UpdatePoinWaktu()
    {
        if (poinWaktu != null)
        {
            int menit = Mathf.FloorToInt(waktu / 60f);
            int detik = Mathf.FloorToInt(waktu % 60f);
            poinWaktu.text = menit.ToString("00") + ":" + detik.ToString("00");
        }
    }

    IEnumerator BlinkPoinWaktu()
    {
        while (true)
        {
            if (poinWaktu != null)
                poinWaktu.color = Color.red;
            yield return new WaitForSeconds(0.3f);
            if (poinWaktu != null)
                poinWaktu.color = defaultColor;
            yield return new WaitForSeconds(0.3f);
        }
    }
}
