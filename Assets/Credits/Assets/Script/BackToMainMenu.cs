using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainMenu : MonoBehaviour
{
    // Fungsi ini dipanggil saat tombol diklik
    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
