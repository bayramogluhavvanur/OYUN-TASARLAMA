using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float hiz = 6f;
    public float donmeHizi = 150f;

    [Header("Başlangıç Konumu Ayarları")]
    public Transform baslangicNoktasi; // İstersen boş bir obje sürükle
    public Vector3 sabitKoordinat;     // Obje yoksa bu koordinatı kullanır
    public Vector3 sabitRotasyon;      // Başlangıçta bakacağı yön

    [Header("Arayüz (UI) Kontrolü")]
    public GameObject diyalogPaneli;   // Inspector'dan DialoguePanel'i buraya sürükleyeceğiz

    void Start()
    {
        // Eğer Inspector'dan bir başlangıç noktası objesi atandıysa oraya ışınla
        if (baslangicNoktasi != null)
        {
            transform.position = baslangicNoktasi.position;
            transform.rotation = baslangicNoktasi.rotation;
        }
        else
        {
            // Obje atanmadıysa Inspector'daki Sabit Koordinat değerlerine ışınla
            transform.position = sabitKoordinat;
            transform.eulerAngles = sabitRotasyon;
        }
    }

    void Update()
    {
        // Eğer diyalog paneli atandıysa ve sahnede aktifse, hareket kodlarını çalıştırma
        if (diyalogPaneli != null && diyalogPaneli.activeSelf)
        {
            return;
        }

        float yuru = Input.GetAxis("Vertical") * hiz * Time.deltaTime;
        float don = Input.GetAxis("Horizontal") * donmeHizi * Time.deltaTime;

        // İleri-geri yürüme ve sağa-sola dönme işlemi
        transform.Translate(0, 0, yuru);
        transform.Rotate(0, don, 0);
    }
}