using UnityEngine;

public class CompanionBridge : MonoBehaviour
{
    [Header("ML-Agents Ajan Referansı")]
    [SerializeField] private YoldasZeka yoldasAgent;

    public void ProcessLLMCommand(string command)
    {
        Debug.Log($"[CompanionBridge] LLM'den Gelen Komut İşleniyor: {command}");

        switch (command)
        {
            case "TOGGLE_LIGHTS":
            case "TOGGLE_SWITCH":
                ExecuteToggleLights();
                break;

            case "REMOVE_OBSTACLE":
                ExecuteRemoveObstacle();
                break;

            case "NONE":
            default:
                Debug.Log("[CompanionBridge] Herhangi bir fiziksel eylem gerekmiyor.");
                break;
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