using UnityEngine;
using UnityEngine.UI;

public class AutoScroll : MonoBehaviour
{
    public ScrollRect scrollRect;  // ScrollRect dari Scroll View
    public float scrollSpeed = 0.1f;  // Kecepatan scroll
    public bool autoScroll = true;  // Untuk mengaktifkan atau menonaktifkan scroll otomatis

    void Update()
    {
        if (autoScroll)
        {
            // Menggerakkan scroll ke bawah (scrolling vertikal)
            if (scrollRect.verticalNormalizedPosition > 0)
            {
                scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime;
            }
        }
    }
}
