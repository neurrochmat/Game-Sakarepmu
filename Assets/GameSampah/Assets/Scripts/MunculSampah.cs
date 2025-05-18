using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MunculSampah : MonoBehaviour
{
    public float jeda = 0.8f;
    float timer;
    public GameObject[] obyekSampah;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f; // Reset waktu agar game berjalan normal saat scene dimulai
        Data.score = 0; // Reset score setiap kali scene Gameplay dimulai
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= jeda)
        {
            int random = Random.Range(0, obyekSampah.Length);
            Instantiate(obyekSampah[random], transform.position, transform.rotation);
            timer = 0;
        }
    }
}
