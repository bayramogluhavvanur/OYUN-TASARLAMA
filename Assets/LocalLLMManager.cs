using System.Collections;
using UnityEngine;

public class LocalLLMManager : MonoBehaviour
{
    [Header("Test Ayarları")]
    [SerializeField] private bool useMockResponse = true; // Ollama yokken işaretli kalsın

    [Header("Referanslar")]
    [SerializeField] private CompanionBridge bridge;
    [SerializeField] private ChatUIManager chatUI;

    public void AskLLM(string userPrompt)
    {
        if (useMockResponse)
        {
            StartCoroutine(SimulateMockResponse(userPrompt));
        }
        else
        {
            // İleride Ollama kurulduğunda buradaki gerçek UnityWebRequest çalışacaktır
            Debug.Log("[LocalLLMManager] Ollama isteği gönderiliyor...");
        }
    }

    private IEnumerator SimulateMockResponse(string userPrompt)
    {
        // Gerçekçi görünmesi için 0.5 saniye bekleme süresi
        yield return new WaitForSeconds(0.5f);

        string lowerPrompt = userPrompt.ToLower();
        string responseText = "";
        string command = "NONE";

        // Gelen mesaja göre sahte yanıt ve komut belirleme
        if (lowerPrompt.Contains("ışık") || lowerPrompt.Contains("şalter") || lowerPrompt.Contains("buton"))
        {
            responseText = "Hemen hallediyorum, pembe butona gidiyorum!";
            command = "TOGGLE_LIGHTS";
        }
        else
        {
            responseText = "Seni anladım, ama şu an sadece butona gitmeyi biliyorum.";
            command = "NONE";
        }

        // 1. Ekrandaki Sohbet Geçmişini Güncelle
        if (chatUI != null)
        {
            chatUI.DisplayResponse($"Siz: {userPrompt}\nYoldaş: {responseText}");
        }

        // 2. Komutu CompanionBridge Üzerinden Ajana İlet
        if (bridge != null)
        {
            bridge.ProcessLLMCommand(command);
        }
    }
}