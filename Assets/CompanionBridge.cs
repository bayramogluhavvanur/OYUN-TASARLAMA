using UnityEngine;

public class CompanionBridge : MonoBehaviour
{
    [Header("ML-Agents Ajan Referansı")]
    [SerializeField] private YoldasZeka yoldasAgent;

    public void ProcessLLMCommand(string command)
    {
        Debug.Log($"[CompanionBridge] LLM'den Gelen Komut İşleniyor: '{command}'");

        // 1. LLM'den gelen metni temizle (Görünmez boşlukları sil ve hepsini BÜYÜK HARF yap)
        string temizKomut = command.Trim().ToUpper();

        // 2. Metnin İÇİNDE bu komutlar geçiyor mu diye kontrol et (Daha esnek yapı)
        if (temizKomut.Contains("TOGGLE_LIGHTS") || temizKomut.Contains("TOGGLE_SWITCH"))
        {
            ExecuteToggleLights();
        }
        else if (temizKomut.Contains("REMOVE_OBSTACLE"))
        {
            ExecuteRemoveObstacle();
        }
        else
        {
            Debug.Log("[CompanionBridge] Herhangi bir fiziksel eylem gerekmiyor.");
        }
    }

    private void ExecuteToggleLights()
    {
        Debug.Log("[CompanionBridge] Yoldaş, pembe butona/şaltere yönlendiriliyor...");
        if (yoldasAgent != null)
        {
            yoldasAgent.ButonaGitKomutuAl();
        }
        else
        {
            Debug.LogWarning("[CompanionBridge] YoldasZeka (yoldasAgent) slotu boş! Lütfen Inspector'dan bağlayın.");
        }
    }

    private void ExecuteRemoveObstacle()
    {
        Debug.Log("[CompanionBridge] Yoldaş engeli kaldırmaya yönlendiriliyor...");
    }
}