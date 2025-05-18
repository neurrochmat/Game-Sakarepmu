using UnityEngine;
using UnityEngine.UI;

public class ExitGameManager : MonoBehaviour
{
    public GameObject exitPopup;
    public Button exitButton;
    public Button yesButton;
    public Button noButton;

    void Start()
    {
        // Sembunyikan popup saat awal
        exitPopup.SetActive(false);

        // Pasang listener tombol
        exitButton.onClick.AddListener(ShowExitPopup);
        yesButton.onClick.AddListener(ExitGame);
        noButton.onClick.AddListener(HideExitPopup);
    }

    void ShowExitPopup()
    {
        exitPopup.SetActive(true);
    }

    void HideExitPopup()
    {
        exitPopup.SetActive(false);
    }

    void ExitGame()
    {
        // Jika di editor Unity
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
