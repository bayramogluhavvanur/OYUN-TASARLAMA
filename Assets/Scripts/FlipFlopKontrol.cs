using UnityEngine;

public class FlipFlopKontrol : MonoBehaviour
{
    public enum ObjeTipi
    {
        AnaKontrolcu, // FlipFlopPanel objesine takılacak
        Switch_J1,
        Switch_K1,
        Switch_J2,
        Switch_K2
    }

    [Header("Bu Objenin Görevi")]
    public ObjeTipi gorev = ObjeTipi.AnaKontrolcu;

    [Header("Ana Kontrolcü Bağlantısı (Sadece Switch'ler İçin)")]
    public FlipFlopKontrol anaPanel;
    public Transform leverGorseli; // Şalterin içindeki 'Lever' objesi (isteğe bağlı)
    public Material acikMateryal;  // Yeşil (1) materyal (isteğe bağlı)
    public Material kapaliMateryal;// Kırmızı (0) materyal (isteğe bağlı)

    [Header("1. JK Flip-Flop Kapıları (Sadece Ana Kontrolcü İçin)")]
    public GameObject gate_1A;
    public GameObject gate_1B;

    [Header("2. JK Flip-Flop Kapıları (Sadece Ana Kontrolcü İçin)")]
    public GameObject gate_2A;
    public GameObject gate_2B;

    // JK Giriş Durumları (false = 0, true = 1)
    [HideInInspector] public bool J1 = false;
    [HideInInspector] public bool K1 = false;
    [HideInInspector] public bool J2 = false;
    [HideInInspector] public bool K2 = false;

    // Flip-Flop Çıkış Durumları (Q1 ve Q2)
    private bool Q1 = false;
    private bool Q2 = false;

    // Şalterin kendi anlık durumu
    private bool salterAcikMi = false;

    private void Start()
    {
        // Sadece Ana Kontrolcü başlangıçta kapıları ayarlar
        if (gorev == ObjeTipi.AnaKontrolcu)
        {
            KapilariGuncelle();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Eğer bu obje AnaKontrolcu ise trigger çalıştırma, sadece şalterler tetiklenir
        if (gorev == ObjeTipi.AnaKontrolcu) return;

        if (other.CompareTag("Player"))
        {
            if (anaPanel == null)
            {
                Debug.LogWarning(gameObject.name + " objesinde Ana Panel atanmamış!");
                return;
            }

            // Şalterin durumunu değiştir (0 ise 1, 1 ise 0 yap)
            salterAcikMi = !salterAcikMi;
            SalterGorseliniGuncelle();

            // Ana paneldeki ilgili girişi güncelle ve Flip-Flop'u tetikle
            anaPanel.GirisDegisti(gorev, salterAcikMi);
        }
    }

    public void GirisDegisti(ObjeTipi tetiklenenSalter, bool yeniDeger)
    {
        switch (tetiklenenSalter)
        {
            case ObjeTipi.Switch_J1:
                J1 = yeniDeger;
                Q1 = JK_Hesapla(J1, K1, Q1);
                break;

            case ObjeTipi.Switch_K1:
                K1 = yeniDeger;
                Q1 = JK_Hesapla(J1, K1, Q1);
                break;

            case ObjeTipi.Switch_J2:
                J2 = yeniDeger;
                Q2 = JK_Hesapla(J2, K2, Q2);
                break;

            case ObjeTipi.Switch_K2:
                K2 = yeniDeger;
                Q2 = JK_Hesapla(J2, K2, Q2);
                break;
        }

        KapilariGuncelle();
    }

    // Gerçek JK Flip-Flop Doğruluk Tablosu Hesaplaması
    private bool JK_Hesapla(bool j, bool k, bool mevcutQ)
    {
        if (!j && !k) return mevcutQ;       // J=0, K=0 -> Değişim yok (Hold)
        if (j && !k) return true;          // J=1, K=0 -> Q = 1 (Set)
        if (!j && k) return false;         // J=0, K=1 -> Q = 0 (Reset)
        return !mevcutQ;                    // J=1, K=1 -> Tersle (Toggle)
    }

    private void KapilariGuncelle()
    {
        // 1. Flip-Flop Kapıları (Q1 ve Q1'in değili)
        if (gate_1A != null) gate_1A.SetActive(!Q1);
        if (gate_1B != null) gate_1B.SetActive(Q1);

        // 2. Flip-Flop Kapıları (Q2 ve Q2'nin değili)
        if (gate_2A != null) gate_2A.SetActive(!Q2);
        if (gate_2B != null) gate_2B.SetActive(Q2);
    }

    private void SalterGorseliniGuncelle()
    {
        if (leverGorseli != null)
        {
            // Eğer materyaller atandıysa rengini değiştir (Yeşil / Kırmızı)
            Renderer rend = leverGorseli.GetComponent<Renderer>();
            if (rend != null && acikMateryal != null && kapaliMateryal != null)
            {
                rend.material = salterAcikMi ? acikMateryal : kapaliMateryal;
            }
        }
    }
}