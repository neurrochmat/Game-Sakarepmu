using UnityEngine;
using UnityEngine.UI;

public class KarakterSelectorUI : MonoBehaviour
{
    public GameObject[] daftarKarakter;
    public Button nextButton, backButton, selectButton;
    public string playerKey; // This should be set to "Player1" or "Player2"
    
    // Elemen UI Text
    public Text characterNameText;       // Menampilkan nama karakter
    public Text characterStatusText;     // Menampilkan status pemilihan karakter
    public Text selectionHeaderText;     // Menampilkan judul panel pemilihan
    
    private int index = 7; // Starting with first character instead of 7
    private CharacterSelector characterSelector;
    private GameObject currentPreview;

    void Start()
    {
        if (daftarKarakter == null || daftarKarakter.Length == 0)
        {
            Debug.LogError("Character array is empty or null!");
            return;
        }

        if (nextButton) nextButton.onClick.AddListener(NextKarakter);
        if (backButton) backButton.onClick.AddListener(BackKarakter);
        if (selectButton) selectButton.onClick.AddListener(PilihKarakter);

        characterSelector = CharacterSelector.Instance;

        // Ensure we have a valid player key
        if (string.IsNullOrEmpty(playerKey))
        {
            Debug.LogWarning("playerKey is not set! Using default Player1");
            playerKey = "Player1";
        }

        Debug.Log($"KarakterSelectorUI initialized for {playerKey}");
        
        // Set judul panel sesuai dengan player
        UpdateSelectionHeader();
        
        TampilkanKarakter();
    }
    
    // Metode untuk mengatur teks judul panel pemilihan
    private void UpdateSelectionHeader()
    {
        if (selectionHeaderText != null)
        {
            if (playerKey == "Player1")
            {
                selectionHeaderText.text = "CHOOSE YOUR CHARACTER P1";
            }
            else if (playerKey == "Player2")
            {
                // Cek apakah ini AI atau Player 2
                string modeMain = PlayerPrefs.GetString("ModeMain", "Player");
                if (modeMain == "AI")
                {
                    selectionHeaderText.text = "CHOOSE CHARACTER AI";
                }
                else
                {
                    selectionHeaderText.text = "CHOOSE YOUR CHARACTER P2";
                }
            }
        }
        
        // Update status teks
        if (characterStatusText != null)
        {
            if (playerKey == "Player1")
            {
                characterStatusText.text = "Silahkan pilih karakter Anda";
            }
            else if (playerKey == "Player2")
            {
                string modeMain = PlayerPrefs.GetString("ModeMain", "Player");
                if (modeMain == "AI")
                {
                    characterStatusText.text = "Silahkan pilih karakter untuk AI";
                }
                else
                {
                    characterStatusText.text = "Silahkan pilih karakter untuk Pemain 2";
                }
            }
        }
    }

    void OnDestroy()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }
    }

    public void NextKarakter()
    {
        if (daftarKarakter.Length == 0) return;

        index = (index + 1) % daftarKarakter.Length;
        TampilkanKarakter();
    }

    public void BackKarakter()
    {
        if (daftarKarakter.Length == 0) return;

        index--;
        if (index < 0) index = daftarKarakter.Length - 1;
        TampilkanKarakter();
    }

    public void PilihKarakter()
    {
        if (daftarKarakter.Length == 0 || index >= daftarKarakter.Length) return;

        // Save the selected character
        string characterName = daftarKarakter[index].name;
        PlayerPrefs.SetString(playerKey, characterName);
        PlayerPrefs.Save(); // Make sure to save the preferences

        Debug.Log($"Selected character {characterName} for {playerKey}");
        
        // Update status teks
        if (characterStatusText != null)
        {
            characterStatusText.text = "Karakter " + characterName + " dipilih!";
        }

        // Notify the CharacterSelector
        if (characterSelector != null)
        {
            characterSelector.SelectCharacter(characterName);
        }

        // Notify the CharacterSelectionUI
        var uiManager = FindObjectOfType<CharacterSelectionUI>();
        if (uiManager != null)
        {
            // FIXED: Swapped parameter order to match CharacterSelectionUI.OnCharacterSelected
            uiManager.OnCharacterSelected(characterName, playerKey);
        }
        else
        {
            Debug.LogWarning("CharacterSelectionUI not found.");
        }

        if (currentPreview != null)
        {
            currentPreview.SetActive(false);
        }
    }

    void TampilkanKarakter()
    {
        if (daftarKarakter.Length == 0 || index >= daftarKarakter.Length)
        {
            Debug.LogWarning("Invalid character index or empty character array.");
            return;
        }

        GameObject currentCharacter = daftarKarakter[index];

        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }

        if (currentCharacter != null)
        {
            currentPreview = Instantiate(currentCharacter, Vector3.zero, Quaternion.identity);

            Rigidbody2D rb = currentPreview.GetComponent<Rigidbody2D>();
            if (rb != null) rb.isKinematic = true;

            // currentPreview.transform.localScale = new Vector3(1, 1, 1);
            currentPreview.layer = LayerMask.NameToLayer("UI");

            // Update nama karakter dengan teks yang baru
            if (characterNameText != null)
            {
                characterNameText.text = currentCharacter.name;
            }
            else
            {
                // Legacy support for existing text object
                Text legacyNameText = GameObject.Find("CharacterNameDisplay")?.GetComponent<Text>();
                if (legacyNameText != null)
                {
                    legacyNameText.text = currentCharacter.name;
                }
            }
        }
    }
}