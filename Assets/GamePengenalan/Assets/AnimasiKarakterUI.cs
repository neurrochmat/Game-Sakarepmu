using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class AnimasiKarakterUI : MonoBehaviour
{
    public CanvasGroup gambar;
    public CanvasGroup nama;
    public CanvasGroup[] statistik;

    // Simpan posisi awal dari Editor
    private Vector3[] statistikAwalPosisi;
    private Vector3 gambarAwalPosisi;
    private Vector3 namaAwalPosisi;

    void Awake()
    {
        // Simpan posisi awal dari Editor
        gambarAwalPosisi = gambar.transform.localPosition;
        namaAwalPosisi = nama.transform.localPosition;
        
        statistikAwalPosisi = new Vector3[statistik.Length];
        for (int i = 0; i < statistik.Length; i++)
        {
            statistikAwalPosisi[i] = statistik[i].transform.localPosition;
        }
    }

    void OnEnable()
    {
        // Pastikan elemen aktif sebelum animasi
        gambar.gameObject.SetActive(true);
        nama.gameObject.SetActive(true);
        foreach (CanvasGroup stat in statistik)
        {
            stat.gameObject.SetActive(true);
        }

        // Reset alpha dan posisi sebelum animasi dimulai
        gambar.alpha = 0;
        gambar.transform.localPosition = gambarAwalPosisi + new Vector3(0, 100, 0);  // Geser ke atas dari posisi awal
        nama.alpha = 0;
        nama.transform.localPosition = namaAwalPosisi + new Vector3(0, 100, 0);

        // Animasi gambar
        gambar.DOFade(1, 0.5f).SetDelay(0f).SetEase(Ease.OutQuad);
        gambar.transform.DOLocalMoveY(gambarAwalPosisi.y, 0.5f).SetDelay(0f).SetEase(Ease.OutQuad);

        // Animasi nama
        nama.DOFade(1, 0.5f).SetDelay(0.3f).SetEase(Ease.OutQuad);
        nama.transform.DOLocalMoveY(namaAwalPosisi.y, 0.5f).SetDelay(0.3f).SetEase(Ease.OutQuad);

        // Animasi statistik (fade in + turun dari atas)
        for (int i = 0; i < statistik.Length; i++)
        {
            statistik[i].alpha = 0;
            statistik[i].transform.localPosition = statistikAwalPosisi[i] + new Vector3(0, 100, 0);
            statistik[i].gameObject.SetActive(true);
            statistik[i].DOFade(1, 0.5f).SetDelay(0.6f + i * 0.2f).SetEase(Ease.OutSine);
            statistik[i].transform.DOLocalMoveY(statistikAwalPosisi[i].y, 0.5f).SetDelay(0.6f + i * 0.2f).SetEase(Ease.OutQuad);
        }
    }
}