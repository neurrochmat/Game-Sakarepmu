using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ModeSelection : MonoBehaviour
{
    [Header("UI Elements")]
    public Button playerVsPlayerButton;
    public Button playerVsAIButton;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Visual Elements")]
    public Image playerVsPlayerImage;
    public Image playerVsAIImage;
    public Color selectedColor = new Color(0.8f, 1f, 0.8f);
    public Color normalColor = Color.white;

    [Header("Audio")]
    public AudioClip selectionSound;
    private AudioSource audioSource;

    private void Start()
    {
        // Inisialisasi komponen audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && selectionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set listener untuk button
        if (playerVsPlayerButton != null)
        {
            playerVsPlayerButton.onClick.AddListener(PilihLawanPlayer);
        }

        if (playerVsAIButton != null)
        {
            playerVsAIButton.onClick.AddListener(PilihLawanAI);
        }

        // Atur teks deskripsi default
        UpdateDescription("", true);
    }

    // Saat tombol mode VS Player dipilih
    public void PilihLawanPlayer()
    {
        // Simpan mode permainan
        PlayerPrefs.SetString("ModeMain", "Player");
        PlayerPrefs.Save();

        // Efek visual button yang dipilih
        UpdateButtonVisual(true);

        // Efek suara
        PlaySelectionSound();

        // Update deskripsi
        UpdateDescription("Mode 2 Pemain: Tantang teman Anda dalam pertandingan pingpong langsung!", false);

        // Tunggu sebentar sebelum pindah scene
        Invoke("LoadCharacterSelection", 0.5f);
    }

    // Saat tombol mode VS AI dipilih
    public void PilihLawanAI()
    {
        // Simpan mode permainan
        PlayerPrefs.SetString("ModeMain", "AI");
        PlayerPrefs.Save();

        // Efek visual button yang dipilih
        UpdateButtonVisual(false);

        // Efek suara
        PlaySelectionSound();

        // Update deskripsi
        UpdateDescription("Mode VS AI: Tantang komputer dengan berbagai tingkat kesulitan!", false);

        // Tunggu sebentar sebelum pindah scene
        Invoke("LoadCharacterSelection", 0.5f);
    }

    private void LoadCharacterSelection()
    {
        SceneManager.LoadScene("Character");
    }

    private void UpdateButtonVisual(bool isPlayerVsPlayer)
    {
        // Update visual untuk tombol yang dipilih
        if (playerVsPlayerImage != null)
        {
            playerVsPlayerImage.color = isPlayerVsPlayer ? selectedColor : normalColor;
        }

        if (playerVsAIImage != null)
        {
            playerVsAIImage.color = isPlayerVsPlayer ? normalColor : selectedColor;
        }
    }

    private void UpdateDescription(string message, bool isDefault)
    {
        if (descriptionText != null)
        {
            descriptionText.text = message;
            
            // Jika default, ganti dengan pesan default
            if (isDefault && string.IsNullOrEmpty(message))
            {
                descriptionText.text = "Pilih mode permainan untuk memulai!";
            }
        }
    }

    private void PlaySelectionSound()
    {
        if (audioSource != null && selectionSound != null)
        {
            audioSource.PlayOneShot(selectionSound);
        }
    }

    // Untuk menampilkan informasi saat cursor hover di atas tombol
    public void OnHoverPlayerVsPlayer()
    {
        UpdateDescription("Mode 2 Pemain: Tantang teman Anda dalam pertandingan pingpong langsung!", false);
    }

    public void OnHoverPlayerVsAI()
    {
        UpdateDescription("Mode VS AI: Tantang komputer dengan berbagai tingkat kesulitan!", false);
    }

    public void OnHoverExit()
    {
        UpdateDescription("", true);
    }
}