using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class LocalLLMManager : MonoBehaviour
{
    [Header("Ollama Ayarları")]
    [SerializeField] private bool useMockResponse = false; // Ollama çalışırken false yapın!
    [SerializeField] private string ollamaUrl = "http://localhost:11434/api/generate";
    [SerializeField] private string modelName = "llama3";

    [Header("Referanslar")]
    [SerializeField] private CompanionBridge bridge;
    [SerializeField] private ChatUIManager chatUI;

    // Ollama API İstek Yapısı
    [System.Serializable]
    private class OllamaRequest
    {
        public string model;
        public string prompt;
        public bool stream = false;
    }

    // Ollama API Yanıt Yapısı
    [System.Serializable]
    private class OllamaResponse
    {
        public string model;
        public string response;
        public bool done;
    }

    public void AskLLM(string userPrompt)
    {
        if (useMockResponse)
        {
            StartCoroutine(SimulateMockResponse(userPrompt));
        }
        else
        {
            StartCoroutine(SendOllamaRequest(userPrompt));
        }
    }

    private IEnumerator SendOllamaRequest(string userPrompt)
    {
        // 1. Chat UI üzerinde "Düşünüyor..." bilgisi göster
        if (chatUI != null)
        {
            chatUI.DisplayResponse($"Siz: {userPrompt}\nYoldaş: Düşünüyor...");
        }

        // 2. Ollama'ya yönlendirici Sistem Prompt'u hazırla
        string systemPrompt = 
            "Sen bir oyundaki 'Yoldaş' adında yardımcı yapay zeka karakterisin.\n" +
            $"Kullanıcının mesajı: \"{userPrompt}\"\n\n" +
            "GÖREVİN:\n" +
            "1. Kullanıcıya Türkçe kısa ve doğal bir yanıt ver.\n" +
            "2. Eğer kullanıcı ışığı açmayı, şalteri indirmeyi veya butona basmayı istiyorsa yanıtının EN SONUNA [TOGGLE_LIGHTS] ekle.\n" +
            "3. Eğer engeli kaldırmayı istiyorsa yanıtının EN SONUNA [REMOVE_OBSTACLE] ekle.\n" +
            "4. Herhangi bir eylem gerekmiyorsa yanıtının EN SONUNA [NONE] ekle.\n\n" +
            "Örnek Yanıt: Hemen gidip pembe butona basıyorum! [TOGGLE_LIGHTS]";

        OllamaRequest reqBody = new OllamaRequest
        {
            model = modelName,
            prompt = systemPrompt,
            stream = false
        };

        string jsonPayload = JsonUtility.ToJson(reqBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(ollamaUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                OllamaResponse respData = JsonUtility.FromJson<OllamaResponse>(request.downloadHandler.text);
                string rawText = respData.response;

                // Komutu ve Ekrana Basılacak Temiz Metni Ayıkla
                string command = ParseCommand(rawText);
                string cleanText = CleanResponseText(rawText);

                // 3. Ekrana Yanıtı Yazdır
                if (chatUI != null)
                {
                    chatUI.DisplayResponse($"Siz: {userPrompt}\nYoldaş: {cleanText}");
                }

                // 4. Komutu Ajana Gönder
                if (bridge != null)
                {
                    bridge.ProcessLLMCommand(command);
                }
            }
            else
            {
                Debug.LogError("[LocalLLMManager] Ollama Bağlantı Hatası: " + request.error);
                if (chatUI != null)
                {
                    chatUI.DisplayResponse($"Siz: {userPrompt}\nYoldaş: (Bağlantı hatası! Ollama arka planda açık mı?)");
                }
            }
        }
    }

    // Metin içindeki [TOGGLE_LIGHTS] gibi komut etiketlerini bulur
    private string ParseCommand(string text)
    {
        if (text.Contains("[TOGGLE_LIGHTS]")) return "TOGGLE_LIGHTS";
        if (text.Contains("[REMOVE_OBSTACLE]")) return "REMOVE_OBSTACLE";
        return "NONE";
    }

    // Ekran arayüzünde köşeli parantezli komut yazısının görünmemesi için temizler
    private string CleanResponseText(string text)
    {
        return text.Replace("[TOGGLE_LIGHTS]", "")
                   .Replace("[REMOVE_OBSTACLE]", "")
                   .Replace("[NONE]", "")
                   .Trim();
    }

    private IEnumerator SimulateMockResponse(string userPrompt)
    {
        yield return new WaitForSeconds(0.5f);
        if (chatUI != null) chatUI.DisplayResponse($"Siz: {userPrompt}\nYoldaş: (Mock yanıtı aktif)");
    }
}