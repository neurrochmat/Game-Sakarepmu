using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CharacterSelector : MonoBehaviour
{
    // Singleton instance
    public static CharacterSelector Instance;

    [Header("Karakter")]
    public GameObject[] daftarKarakter;        // Array karakter yang tersedia
    public GameObject selectedCharacterP1;     // Karakter yang dipilih untuk Player 1
    public GameObject selectedCharacterP2;     // Karakter yang dipilih untuk Player 2

    [Header("UI")]
    public Transform characterContainer;       // Container untuk karakter yang dapat dipilih
    public GameObject characterButtonPrefab;   // Prefab untuk tombol pemilihan karakter
    public TextMeshProUGUI characterNameText;  // Text untuk menampilkan nama karakter
    public TextMeshProUGUI characterDescText;  // Text untuk deskripsi karakter
    public Image characterPreviewImage;        // Preview karakter yang dipilih

    private string currentPlayer = "Player1";  // Player yang sedang memilih karakter
    private List<GameObject> spawnedButtons = new List<GameObject>();  // Daftar tombol yang di-spawn

    void Awake()
    {
        // Setup singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Cari prefab karakter jika belum diset
        if (daftarKarakter == null || daftarKarakter.Length == 0)
        {
            SpawnKarakter2D spawner = FindObjectOfType<SpawnKarakter2D>();
            if (spawner != null && spawner.daftarKarakter != null && spawner.daftarKarakter.Length > 0)
            {
                daftarKarakter = spawner.daftarKarakter;
                Debug.Log("Daftar karakter diambil dari SpawnKarakter2D");
            }
        }
    }

    void Start()
    {
        // Load karakter yang sebelumnya dipilih (jika ada)
        string player1Character = PlayerPrefs.GetString("Player1", "");
        string player2Character = PlayerPrefs.GetString("Player2", "");

        if (!string.IsNullOrEmpty(player1Character))
        {
            selectedCharacterP1 = CariKarakterByName(player1Character);
        }

        if (!string.IsNullOrEmpty(player2Character))
        {
            selectedCharacterP2 = CariKarakterByName(player2Character);
        }

        // Tampilkan UI pemilihan karakter
        if (characterContainer != null && characterButtonPrefab != null)
        {
            SpawnCharacterButtons();
        }
    }

    // Inisialisasi tombol pemilihan karakter
    void SpawnCharacterButtons()
    {
        // Hapus tombol yang sudah ada sebelumnya
        foreach (var button in spawnedButtons)
        {
            Destroy(button);
        }
        spawnedButtons.Clear();

        // Pastikan container ada
        if (characterContainer == null)
        {
            Debug.LogError("Character Container belum diatur!");
            return;
        }

        // Pastikan prefab tombol ada
        if (characterButtonPrefab == null)
        {
            Debug.LogError("Character Button Prefab belum diatur!");
            return;
        }

        // Buat tombol untuk setiap karakter
        foreach (var karakter in daftarKarakter)
        {
            if (karakter == null) continue;

            GameObject buttonObj = Instantiate(characterButtonPrefab, characterContainer);
            Button button = buttonObj.GetComponent<Button>();
            
            // Set nama karakter sebagai teks tombol (jika ada)
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = karakter.name;
            }

            // Set gambar preview karakter (jika ada)
            Image buttonImage = buttonObj.GetComponent<Image>();
            SpriteRenderer spriteRenderer = karakter.GetComponent<SpriteRenderer>();
            if (buttonImage != null && spriteRenderer != null)
            {
                buttonImage.sprite = spriteRenderer.sprite;
            }

            // Tambahkan event untuk pemilihan karakter
            string karakterName = karakter.name;
            button.onClick.AddListener(() => SelectCharacter(karakterName));

            // Tambahkan ke daftar tombol yang di-spawn
            spawnedButtons.Add(buttonObj);
        }

        // Perbarui UI awal
        UpdateCharacterPreview(null);
    }

    // Pilih karakter berdasarkan nama
    public void SelectCharacter(string characterName)
    {
        Debug.Log($"CharacterSelector.SelectCharacter called with: {characterName}");
        GameObject selectedCharacter = CariKarakterByName(characterName);
        
        if (selectedCharacter != null)
        {
            // Simpan pilihan berdasarkan player saat ini
            if (currentPlayer == "Player1")
            {
                selectedCharacterP1 = selectedCharacter;
                PlayerPrefs.SetString("Player1", characterName);
                
                // PENTING: Jangan mengubah currentPlayer atau mengubah UI di sini
                // Biarkan CharacterSelectionUI yang menangani perpindahan panel
                
                // Update preview
                UpdateCharacterPreview(selectedCharacter);
                
                // Lebih baik panggil CharacterSelectionUI di sini untuk perpindahan panel
                CharacterSelectionUI selectionUI = FindObjectOfType<CharacterSelectionUI>();
                if (selectionUI != null)
                {
                    selectionUI.OnCharacterSelected(characterName, "Player1");
                    Debug.Log("Notified CharacterSelectionUI about Player1 selection");
                }
            }
            else if (currentPlayer == "Player2")
            {
                selectedCharacterP2 = selectedCharacter;
                PlayerPrefs.SetString("Player2", characterName);
                
                // Update preview
                UpdateCharacterPreview(selectedCharacter);
                
                // Notifikasi ke CharacterSelectionUI
                CharacterSelectionUI selectionUI = FindObjectOfType<CharacterSelectionUI>();
                if (selectionUI != null)
                {
                    selectionUI.OnCharacterSelected(characterName, "Player2");
                    Debug.Log("Notified CharacterSelectionUI about Player2 selection");
                }
            }
        }
        else
        {
            Debug.LogWarning("Karakter dengan nama " + characterName + " tidak ditemukan!");
        }
    }
    
    // Metode untuk mengubah pemain saat ini
    public void SetCurrentPlayer(string playerKey)
    {
        if (playerKey == "Player1" || playerKey == "Player2")
        {
            currentPlayer = playerKey;
            Debug.Log($"Current player set to: {currentPlayer}");
        }
    }

    // Update preview karakter yang dipilih
    void UpdateCharacterPreview(GameObject character)
    {
        if (characterPreviewImage != null)
        {
            if (character != null)
            {
                SpriteRenderer spriteRenderer = character.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    characterPreviewImage.sprite = spriteRenderer.sprite;
                    characterPreviewImage.enabled = true;
                }
            }
            else
            {
                characterPreviewImage.sprite = null;
                characterPreviewImage.enabled = false;
            }
        }

        if (characterNameText != null)
        {
            characterNameText.text = character != null ? character.name : "Pilih Karakter";
        }

        if (characterDescText != null)
        {
            // Tampilkan deskripsi karakter jika ada
            KarakterInfo karakterInfo = character?.GetComponent<KarakterInfo>();
            characterDescText.text = karakterInfo != null ? karakterInfo.deskripsi : "Pilih karakter untuk melihat deskripsi";
        }
    }

    // Preview karakter saat mouse hover
    public void PreviewCharacter(string characterName)
    {
        GameObject character = CariKarakterByName(characterName);
        if (character != null)
        {
            UpdateCharacterPreview(character);
        }
    }

    // Cari karakter berdasarkan nama
    GameObject CariKarakterByName(string nama)
    {
        if (string.IsNullOrEmpty(nama)) return null;

        foreach (var karakter in daftarKarakter)
        {
            if (karakter != null && karakter.name == nama)
                return karakter;
        }

        return null;
    }

    // Reset pemilihan karakter (untuk kembali ke menu)
    public void ResetSelection()
    {
        selectedCharacterP1 = null;
        selectedCharacterP2 = null;
        currentPlayer = "Player1";
        UpdateCharacterPreview(null);
    }
}