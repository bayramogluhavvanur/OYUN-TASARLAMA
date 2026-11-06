using UnityEngine;
using TMPro; // Eğer bitiş yazısı TextMeshPro ise

public class OyunBitisController : MonoBehaviour
{
    public GameObject bitisEkraniPaneli; // Oyun bitince açılacak UI Paneli

    private void OnTriggerEnter(Collider other)
    {
        // Eğer tetikleyiciye giren obje Player ise
        if (other.CompareTag("Player"))
        {
            // Bitiş panelini aktif et
            if (bitisEkraniPaneli != null)
            {
                bitisEkraniPaneli.SetActive(true);
            }

            // İsteğe bağlı: Oyunu durdur (Zamanı dondur)
            Time.timeScale = 0f;
        }
    }
}