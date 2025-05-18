using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ButtonSceneDelay : MonoBehaviour
{
    public Button myButton;
    public string nextSceneName;
    public float delay = 3f;

    void Start()
    {
        myButton.onClick.AddListener(() => StartCoroutine(LoadSceneWithDelay()));
    }

    IEnumerator LoadSceneWithDelay()
    {
        Debug.Log("Tombol diklik, tunggu " + delay + " detik...");
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(nextSceneName);
    }
}
