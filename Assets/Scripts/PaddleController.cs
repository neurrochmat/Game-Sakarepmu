using UnityEngine;

public class PaddleController : MonoBehaviour
{
    public float batasAtas;
    public float batasBawah;
    public float batasKiri;
    public float batasKanan;
    public float kecepatan;
    public string axisVertikal;
    public string axisHorizontal;

    private Vector3 startPosition;

    protected virtual void Start()
    {
        startPosition = transform.position;

        string namaKarakter = gameObject.name;
        Debug.Log("Paddle Controller Start untuk: " + namaKarakter);

        // Hanya set default jika axis belum diatur dari luar (kosong/null)
        if (string.IsNullOrEmpty(axisVertikal) || string.IsNullOrEmpty(axisHorizontal))
        {
            if (namaKarakter == "Player1")
            {
                axisVertikal = "Vertical1";
                axisHorizontal = "Horizontal1";
            }
            else if (namaKarakter == "Player2")
            {
                axisVertikal = "Vertical2";
                axisHorizontal = "Horizontal2";
            }
            else
            {
                Debug.LogWarning("Karakter tidak dikenali: " + namaKarakter);
            }
        }

        Debug.Log(namaKarakter + " - Menggunakan axis: " + axisVertikal + ", " + axisHorizontal);
        Debug.Log(namaKarakter + " - Batas: Atas=" + batasAtas + ", Bawah=" + batasBawah +
                 ", Kiri=" + batasKiri + ", Kanan=" + batasKanan);
    }

    void Update()
    {
        float inputVertical = Input.GetAxis(axisVertikal);
        float inputHorizontal = Input.GetAxis(axisHorizontal);

        float gerakY = inputVertical * kecepatan * Time.deltaTime;
        float gerakX = inputHorizontal * kecepatan * Time.deltaTime;

        // Tambahan fallback input langsung
        if (gameObject.name == "Player1")
        {
            if (Input.GetKey(KeyCode.W)) gerakY += kecepatan * Time.deltaTime;
            if (Input.GetKey(KeyCode.S)) gerakY -= kecepatan * Time.deltaTime;
            if (Input.GetKey(KeyCode.A)) gerakX -= kecepatan * Time.deltaTime;
            if (Input.GetKey(KeyCode.D)) gerakX += kecepatan * Time.deltaTime;
        }
        else if (gameObject.name == "Player2")
        {
            if (Input.GetKey(KeyCode.UpArrow)) gerakY += kecepatan * Time.deltaTime;
            if (Input.GetKey(KeyCode.DownArrow)) gerakY -= kecepatan * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftArrow)) gerakX -= kecepatan * Time.deltaTime;
            if (Input.GetKey(KeyCode.RightArrow)) gerakX += kecepatan * Time.deltaTime;
        }

        Vector3 posisiAwal = transform.position;
        Vector3 nextPos = posisiAwal + new Vector3(gerakX, gerakY, 0);

        nextPos.y = Mathf.Clamp(nextPos.y, batasBawah, batasAtas);
        nextPos.x = Mathf.Clamp(nextPos.x, batasKiri, batasKanan);

        transform.position = nextPos;
    }
}
