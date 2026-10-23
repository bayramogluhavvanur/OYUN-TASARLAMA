using System.Collections;
using UnityEngine;

public class SwitchController : MonoBehaviour
{
    [Header("Küçülerek Yok Olacak Kapı")]
    public GameObject kapi;
    public float kuculmeHizi = 2f;

    private bool tetiklendi = false;

    private void OnTriggerEnter(Collider other)
    {
        // Yalnızca "Player" etiketli obje dokunduğunda tetiklenir
        if (!tetiklendi && other.CompareTag("Player"))
        {
            tetiklendi = true;
            Debug.Log("Switch ile etkileşime geçildi! Kapı küçülüyor..."); // İstenen Console logu
            StartCoroutine(KapiyiKucult());
        }
    }

    private IEnumerator KapiyiKucult()
    {
        Vector3 baslangicOlcegi = kapi.transform.localScale;
        float zaman = 0;

        // Kapının ölçeğini yavaşça 0'a doğru küçült
        while (zaman < 1f)
        {
            zaman += Time.deltaTime * kuculmeHizi;
            kapi.transform.localScale = Vector3.Lerp(baslangicOlcegi, Vector3.zero, zaman);
            yield return null;
        }

        // Küçülme bitince objeyi tamamen kapat
        kapi.SetActive(false);
    }
}