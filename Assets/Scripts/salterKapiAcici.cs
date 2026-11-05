using UnityEngine;

public class SalterKapiAcici : MonoBehaviour
{
    [Header("Açılacak Kapılar (Sahneden Sürükle)")]
    public GameObject kapi1;
    public GameObject kapi2;

    [Header("Yükselme Ayarı")]
    [Tooltip("Kapıların Y ekseninde ne kadar yukarı çıkacağı")]
    public float yukselmeMiktariY = 4f;

    // Şalterin sadece bir kez çalışmasını sağlayan kontrol
    private bool tetiklendiMi = false;

    private void OnTriggerEnter(Collider other)
    {
        // Eğer daha önce basılmadıysa ve dokunan objenin Tag'i "CompanionAgent" ise
        if (!tetiklendiMi &&
    (other.CompareTag("CompanionAgent") ||
     other.CompareTag("CompanionAgent 1") ||
     other.CompareTag("CompanionAgent 1 (1)")))
        {
            tetiklendiMi = true; // Tekrar tetiklenmesini kilitle

            KapiyiYukselt(kapi1);
            KapiyiYukselt(kapi2);

            Debug.Log(gameObject.name + " şalteri Companion Agent tarafından açıldı!");
        }
    }

    private void KapiyiYukselt(GameObject kapi)
    {
        if (kapi != null)
        {
            // Kapının mevcut konumunu al ve sadece Y değerine yukselmeMiktariY ekle
            Vector3 yeniKonum = kapi.transform.position;
            yeniKonum.y += yukselmeMiktariY;
            kapi.transform.position = yeniKonum;
        }
    }
}