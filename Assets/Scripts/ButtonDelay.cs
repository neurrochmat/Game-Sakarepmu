using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonDelay : MonoBehaviour
{
    public Button myButton;

    void Start()
    {
        myButton.onClick.AddListener(() => StartCoroutine(DelayedAction()));
    }

    IEnumerator DelayedAction()
    {
        yield return new WaitForSeconds(5f); // delay 2 detik
        Debug.Log("Tombol diproses setelah 5 detik!");
        // Jalankan aksi di sini
    }
}
