using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // Tambah listener
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main2" || scene.name == "MainMenu")
        {
            Destroy(gameObject); // Hentikan musik dengan menghancurkan GameObject-nya
        }
    }

    private void OnDestroy()
    {
        // Lepas listener untuk mencegah error
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
