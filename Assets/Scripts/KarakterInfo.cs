using UnityEngine;

public class KarakterInfo : MonoBehaviour
{
    [Header("Informasi Karakter")]
    public string namaKarakter;
    [TextArea(3, 5)]
    public string deskripsi;
    public Sprite icon;
    
    [Header("Statistik")]
    [Range(1, 10)]
    public int kecepatan = 5;
    [Range(1, 10)]
    public int kekuatan = 5;
    [Range(1, 10)]
    public int kontrol = 5;
    
    [Header("Visual")]
    public Color warnaTema = Color.white;
    public AudioClip suaraPukulan;
    
    void Start()
    {
        // Jika nama karakter kosong, gunakan nama GameObject
        if (string.IsNullOrEmpty(namaKarakter))
        {
            namaKarakter = gameObject.name;
        }
        
        // Jika icon kosong, coba ambil dari sprite renderer
        if (icon == null)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                icon = spriteRenderer.sprite;
            }
        }
    }
    
    // Metode untuk mendapatkan faktor kecepatan berdasarkan statistik
    public float GetKecepatanFaktor()
    {
        return 0.8f + (kecepatan * 0.04f); // 0.8 sampai 1.2
    }
    
    // Metode untuk mendapatkan faktor kekuatan berdasarkan statistik
    public float GetKekuatanFaktor()
    {
        return 0.8f + (kekuatan * 0.04f); // 0.8 sampai 1.2
    }
    
    // Metode untuk mendapatkan faktor kontrol berdasarkan statistik
    public float GetKontrolFaktor()
    {
        return 0.8f + (kontrol * 0.04f); // 0.8 sampai 1.2
    }
    
    // Metode untuk menampilkan informasi karakter dalam UI
    public string GetInfoText()
    {
        return $"{namaKarakter}\n" +
               $"Kecepatan: {GenBars(kecepatan)}\n" +
               $"Kekuatan: {GenBars(kekuatan)}\n" +
               $"Kontrol: {GenBars(kontrol)}";
    }
    
    // Helper untuk membuat visual bar statistik ("|||||     ")
    private string GenBars(int value)
    {
        string bars = "";
        for (int i = 0; i < 10; i++)
        {
            bars += (i < value) ? "|" : " ";
        }
        return bars;
    }
}