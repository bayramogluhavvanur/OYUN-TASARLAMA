using UnityEngine;

public class FlipFlopKontrol : MonoBehaviour
{
    [Header("Şalter Ayarları")]
    [Tooltip("J1/K1 şalterleri için 1, J2/K2 şalterleri için 2 yapın")]
    public int flipFlopNumarasi = 1;

    [Tooltip("Bu bir J şalteriyse işaretleyin, K şalteriyse işareti kaldırın")]
    public bool isJSwitch = true;

    [Header("Turuncu Kapı Referansları")]
    public GameObject turuncuKapi1;
    public GameObject turuncuKapi2;
    public GameObject turuncuKapi3;
    public GameObject turuncuKapi4;

    [Header("Kırmızı Kapı Referansları")]
    public GameObject kirmiziKapiPlayer; // Player'ın kırmızı kapısı
    public GameObject kirmiziKapiAgent;  // Agent'ın kırmızı kapısı

    // Bütün şalterlerin ortaklaşa kullandığı global hafıza (static)
    private static int q1 = 0;
    private static int q2 = 0;

    private void Start()
    {
        KapilariGuncelle();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            int yeniDeger = isJSwitch ? 1 : 0;

            if (flipFlopNumarasi == 1)
            {
                q1 = yeniDeger;
            }
            else if (flipFlopNumarasi == 2)
            {
                q2 = yeniDeger;
            }

            KapilariGuncelle();
        }
    }

    private void KapilariGuncelle()
    {
        if (turuncuKapi1 == null) return;

        // Önce tüm kapıları kapat (true = engel var)
        turuncuKapi1.SetActive(true);
        turuncuKapi2.SetActive(true);
        turuncuKapi3.SetActive(true);
        turuncuKapi4.SetActive(true);
        if (kirmiziKapiPlayer != null) kirmiziKapiPlayer.SetActive(true);
        if (kirmiziKapiAgent != null) kirmiziKapiAgent.SetActive(true);

        // DURUM 1: (0, 1) -> Sadece ilk kapı açılır
        if (q1 == 0 && q2 == 1)
        {
            turuncuKapi1.SetActive(false);
        }
        // DURUM 2: (1, 0) -> İlk 2 kapı açılır
        else if (q1 == 1 && q2 == 0)
        {
            turuncuKapi1.SetActive(false);
            turuncuKapi2.SetActive(false);
        }
        // DURUM 3: (1, 1) -> Bütün şifre çözüldü, tüm kapılar açılır!
        else if (q1 == 1 && q2 == 1)
        {
            turuncuKapi1.SetActive(false);
            turuncuKapi2.SetActive(false);
            turuncuKapi3.SetActive(false);
            turuncuKapi4.SetActive(false);
            if (kirmiziKapiPlayer != null) kirmiziKapiPlayer.SetActive(false);
            if (kirmiziKapiAgent != null) kirmiziKapiAgent.SetActive(false);
        }
    }
}