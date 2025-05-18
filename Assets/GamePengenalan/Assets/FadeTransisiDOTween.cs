using UnityEngine;
using DG.Tweening;

public class CharacterSelection : MonoBehaviour
{
    [System.Serializable]
    public class Character
    {
        public GameObject obj;
        public AudioSource appearSound; // Audio saat karakter muncul
    }

    [Header("Character Settings")]
    public Character[] characters;

    [Header("Animation Settings")]
    public float fadeDuration = 0.5f;
    public float moveDistance = 100f;

    private int currentIndex = 0;
    private bool isAnimating = false;

    void Start()
    {
        // Validasi
        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("Characters array not set up!");
            enabled = false;
            return;
        }

        // Non-aktifkan semua karakter terlebih dahulu
        foreach (var character in characters)
        {
            if (character.obj != null)
                character.obj.SetActive(false);
        }

        // Aktifkan karakter pertama
        ActivateCharacter(currentIndex, true);
    }

    void Update()
    {
        if (isAnimating) return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentIndex < characters.Length - 1)
                SwitchCharacter(currentIndex + 1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentIndex > 0)
                SwitchCharacter(currentIndex - 1);
        }
    }

    void ActivateCharacter(int index, bool isInitial)
    {
        if (index < 0 || index >= characters.Length) return;

        characters[index].obj.SetActive(true);
        
        // Mainkan audio muncul hanya jika bukan inisialisasi awal
        if (!isInitial && characters[index].appearSound != null)
        {
            characters[index].appearSound.Play();
        }

        // Set state awal untuk animasi
        CanvasGroup cg = characters[index].obj.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = isInitial ? 1 : 0;
            characters[index].obj.transform.localPosition = isInitial 
                ? Vector3.zero 
                : new Vector3(0, moveDistance, 0);
        }
    }

    void SwitchCharacter(int newIndex)
    {
        isAnimating = true;

        // Animasi keluar karakter lama
        Character oldChar = characters[currentIndex];
        if (oldChar.obj != null)
        {
            CanvasGroup oldCG = oldChar.obj.GetComponent<CanvasGroup>();
            if (oldCG != null)
            {
                oldCG.DOFade(0, fadeDuration);
                oldChar.obj.transform.DOLocalMoveY(moveDistance, fadeDuration)
                    .OnComplete(() => oldChar.obj.SetActive(false));
            }
        }

        // Aktifkan dan animasi karakter baru
        Character newChar = characters[newIndex];
        ActivateCharacter(newIndex, false);
        
        if (newChar.obj != null)
        {
            CanvasGroup newCG = newChar.obj.GetComponent<CanvasGroup>();
            if (newCG != null)
            {
                newCG.DOFade(1, fadeDuration);
                newChar.obj.transform.DOLocalMoveY(0, fadeDuration)
                    .OnComplete(() => {
                        currentIndex = newIndex;
                        isAnimating = false;
                    });
            }
        }
    }
}