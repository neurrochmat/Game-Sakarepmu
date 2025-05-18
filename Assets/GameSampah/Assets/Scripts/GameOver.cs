using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public Text scoreText;
    public AudioClip gameOverClip;
    private AudioSource sfxSource;
    float timer = 0;

    void Start()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        if (gameOverClip != null)
        {
            sfxSource.PlayOneShot(gameOverClip);
        }
        if (scoreText != null)
        {
            scoreText.text = "Skor Akhir: " + Data.score.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 2)
        {
            Data.score = 0;
            SceneManager.LoadScene("Gameplay");
        }
    }
}
