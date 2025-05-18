using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// using TMPro;

public class CharacterSelectionUI : MonoBehaviour
{
    [Header("Panel UI")]
    public GameObject panelPlayer1;
    public GameObject panelPlayer2;
    public GameObject panelAIDifficulty;  // Panel pengaturan kesulitan AI
    
    [Header("Teks Informasi")]
    public Text gameTitle;               // Judul game
    public Text screenInstructionText;   // Instruksi umum

    [Header("AI Difficulty")]
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;

    [Header("Navigasi")]
    public Button backButton;
    public Button skipButton;             // Untuk melewati pemilihan karakter AI

    // Character selector references
    public KarakterSelectorUI player1Selector;
    public KarakterSelectorUI player2Selector;

    private string modeMain;
    private bool player1SudahPilih = false;
    private float aiDifficulty = 0.5f;

    void Start()
    {
        // Ambil mode permainan
        modeMain = PlayerPrefs.GetString("ModeMain", "Player");
        Debug.Log($"Mode permainan: {modeMain}");

        // Load pengaturan AI yang sudah ada
        aiDifficulty = PlayerPrefs.GetFloat("AIDifficulty", 0.5f);

        // Setup UI
        panelPlayer1.SetActive(true);
        panelPlayer2.SetActive(false);
        
        if (panelAIDifficulty != null)
            panelAIDifficulty.SetActive(false);

        // Setup character selectors
        if (player1Selector != null)
            player1Selector.playerKey = "Player1";
            
        if (player2Selector != null)
            player2Selector.playerKey = "Player2";

        // Setup teks instruksi
        if (gameTitle != null)
            gameTitle.text = "PEMILIHAN KARAKTER";
            
        if (screenInstructionText != null)
        {
            if (modeMain == "AI")
                screenInstructionText.text = "Pilih karakter untuk bermain melawan AI";
            else
                screenInstructionText.text = "Pilih karakter untuk pertandingan dua pemain";
        }

        // Setup button events
        if (backButton != null)
        {
            backButton.onClick.AddListener(KembaliKeMenu);
        }

        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(modeMain == "AI");
            skipButton.onClick.AddListener(LewatiPemilihanAI);
        }

        // Setup AI difficulty buttons
        if (easyButton != null)
            easyButton.onClick.AddListener(() => SetDifficultyAndContinue(0.25f));
        
        if (mediumButton != null)
            mediumButton.onClick.AddListener(() => SetDifficultyAndContinue(0.5f));
        
        if (hardButton != null)
            hardButton.onClick.AddListener(() => SetDifficultyAndContinue(0.85f));
        
        // Set tampilan awal tingkat kesulitan
        UpdateDifficultyText();
            
        Debug.Log("CharacterSelectionUI initialized");
    }

    public void OnCharacterSelected(string characterName, string player)
    {
        Debug.Log($"Character selected: {characterName} for {player}");
        
        if (player == "Player1")
        {
            PlayerPrefs.SetString("Player1", characterName);
            PlayerPrefs.Save();
            player1SudahPilih = true;

            if (modeMain == "AI")
            {
                // Jika melawan AI, otomatis tunjukkan panel pengaturan AI
                if (panelAIDifficulty != null)
                {
                    Debug.Log("Showing AI difficulty panel");
                    panelPlayer1.SetActive(false);
                    panelAIDifficulty.SetActive(true);
                    
                    // Update teks instruksi
                    if (screenInstructionText != null)
                        screenInstructionText.text = "Atur tingkat kesulitan AI lawan";
                    
                    return;
                }
                
                // Jika tidak ada panel AI, langsung pilih karakter untuk AI
                Debug.Log("No AI difficulty panel, proceeding to AI character selection");
                panelPlayer1.SetActive(false);
                panelPlayer2.SetActive(true);
                
                // Update teks instruksi
                if (screenInstructionText != null)
                    screenInstructionText.text = "Pilih karakter untuk AI lawan";
            }
            else
            {
                // Mode dua pemain
                Debug.Log("Switching to Player 2 selection");
                panelPlayer1.SetActive(false);
                panelPlayer2.SetActive(true);
                
                // Update teks instruksi
                if (screenInstructionText != null)
                    screenInstructionText.text = "Pemain 2, pilih karakter Anda";
            }
        }
        else if (player == "Player2")
        {
            PlayerPrefs.SetString("Player2", characterName);
            PlayerPrefs.Save();
            Debug.Log("Both characters selected, loading game scene");
            
            // Update teks instruksi sebelum pindah scene
            if (screenInstructionText != null)
                screenInstructionText.text = "Persiapan pertandingan...";
                
            SceneManager.LoadScene("Main");
        }
    }

    // Menggabungkan fungsi set difficulty dan lanjut ke panel berikutnya
    public void SetDifficultyAndContinue(float value)
    {
        aiDifficulty = value;
        PlayerPrefs.SetFloat("AIDifficulty", aiDifficulty);
        PlayerPrefs.Save();
        UpdateDifficultyText();
        
        // Setelah memilih tingkat kesulitan, otomatis pindah ke pemilihan karakter AI
        Debug.Log("Proceeding to AI character selection after setting difficulty");
        if (panelAIDifficulty != null)
            panelAIDifficulty.SetActive(false);
            
        panelPlayer2.SetActive(true);
        
        // Update teks instruksi
        if (screenInstructionText != null)
            screenInstructionText.text = "Pilih karakter untuk AI lawan";
    }

    private void UpdateDifficultyText()
    {
        // Tidak perlu memperbarui teks kesulitan karena difficultyText telah dihapus
        Debug.Log($"AI difficulty set to: {aiDifficulty}");
    }

    public void LewatiPemilihanAI()
    {
        Debug.Log("Skipping AI character selection");
        // Gunakan karakter default untuk AI
        if (daftarKarakter.Length > 0)
        {
            PlayerPrefs.SetString("Player2", daftarKarakter[0].name);
        }
        else
        {
            PlayerPrefs.SetString("Player2", "DefaultCharacter");
        }
        
        PlayerPrefs.Save();
        
        // Update teks instruksi sebelum pindah scene
        if (screenInstructionText != null)
            screenInstructionText.text = "Persiapan pertandingan dengan AI...";
            
        SceneManager.LoadScene("Main");
    }

    public void KembaliKeMenu()
    {
        SceneManager.LoadScene("Character2");
    }
    
    // Properti untuk mengakses daftar karakter (mengambil dari SpawnKarakter2D jika ada)
    public GameObject[] daftarKarakter
    {
        get
        {
            SpawnKarakter2D spawner = FindObjectOfType<SpawnKarakter2D>();
            if (spawner != null && spawner.daftarKarakter != null && spawner.daftarKarakter.Length > 0)
                return spawner.daftarKarakter;
                
            return new GameObject[0];
        }
    }
}