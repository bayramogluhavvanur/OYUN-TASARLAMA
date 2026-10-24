using UnityEngine;

public class FlipFlopKontrol : MonoBehaviour
{
    public GameObject kapiA;
    public GameObject kapiB;
    private bool durum = false;

    private void Start()
    {
        // Başlangıçta Kapi_A sahnede var olsun, Kapi_B yok olsun
        kapiA.SetActive(true);
        kapiB.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Sadece sen (Player) şaltere bastığında çalışsın
        if (other.CompareTag("Player"))
        {
            durum = !durum; // Mantıksal tersleme (Toggle)
            kapiA.SetActive(!durum);
            kapiB.SetActive(durum);
        }
    }
}