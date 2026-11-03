using UnityEngine;

public class KahramanHareket : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float hiz = 5f;
    public float fareHassasiyeti = 2f;

    [Header("Kamera Bağlantısı")]
    public Transform oyuncuKamerasi; // Hiyerarşideki Main Camera'yı buraya sürükleyeceğiz

    private float xRotasyonu = 0f;

    void Start()
    {
        // Fare imlecini oyun ekranına kilitleyip gizler (FPS/TPS tarzı için)
        // Eğer fare imlecini geri görmek istersen klavyeden ESC tuşuna basabilirsin
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- 1. ETRAFA BAKMA (MOUSE) ---
        float fareX = Input.GetAxis("Mouse X") * fareHassasiyeti;
        float fareY = Input.GetAxis("Mouse Y") * fareHassasiyeti;

        // Sağa Sola Bakma (Tüm karakteri Y ekseninde döndürür)
        transform.Rotate(Vector3.up * fareX);

        // Aşağı Yukarı Bakma (Sadece kamerayı X ekseninde döndürür)
        if (oyuncuKamerasi != null)
        {
            xRotasyonu -= fareY;
            xRotasyonu = Mathf.Clamp(xRotasyonu, -60f, 60f); // Kafayı geriye kırmamak için sınır
            oyuncuKamerasi.localRotation = Quaternion.Euler(xRotasyonu, 0f, 0f);
        }

        // --- 2. YÜRÜME (WASD) ---
        float yatay = Input.GetAxis("Horizontal");
        float dikey = Input.GetAxis("Vertical");

        // Karakterin o an baktığı yöne doğru (lokal) hareket etmesini sağlar
        Vector3 hareket = transform.right * yatay + transform.forward * dikey;
        transform.position += hareket * hiz * Time.deltaTime;
    }
}