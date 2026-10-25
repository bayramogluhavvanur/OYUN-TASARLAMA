using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class LocalLLMManager : MonoBehaviour
{
    [Header("Ollama Ayarları")]
    [SerializeField] private bool useMockResponse = false; // Ollama çalışırken false yapın!
    [SerializeField] private string ollamaUrl = "http://localhost:11434/api/generate";
    [SerializeField] private string modelName = "llama3.2:1b";

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

    [System.Serializable]
    public class LLMDecisionJSON
    {
        public string responseText;
        public string command;
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
        if (chatUI != null) chatUI.DisplayResponse($"Siz: {userPrompt}\nYoldaş: Düşünüyor...");

        string systemPrompt =
            "Sen bir oyun içi sadık yoldaş asistanısın. Oyuncuya her zaman kusursuz, akıcı ve anlaşılır Türkçe ile kısa cevaplar ver.\n" +
            "ASLA anlamsız, bozuk veya uyduruk kelimeler üretme (Örneğin 'içtişimi açtım' gibi hatalı kelimeler yazmak yasaktır).\n" +
            "Sadece ve sadece şu JSON formatında yanıt ver:\n\n" +
            "{\n" +
            "  \"responseText\": \"Tamam patron, hemen şaltere gidip ışıkları açıyorum.\",\n" +
            "  \"command\": \"TOGGLE_LIGHTS\"\n" +
            "}\n\n" +
            "Kurallar:\n" +
            "- Eğer oyuncu ışık, şalter, aç veya buton derse 'command' kısmına kesinlikle 'TOGGLE_LIGHTS' yaz.\n" +
            "- Eğer sıradan bir sohbetse 'command' kısmına 'NONE' yaz.\n" +
            "- 'responseText' kısmı her zaman mantıklı, düzgün bir Türkçe cümle olmalıdır.\n\n" +
            $"Kullanıcı Mesajı: \"{userPrompt}\"";

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

            // Zaman aşımı koruması: 10 saniye yanıt gelmezse isteği iptal et (4.3 İsteri)
            request.timeout = 10;

            yield return request.SendWebRequest();

            string cleanText = "Seni anlayamadım ama bekliyorum.";
            string command = "NONE";

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    OllamaResponse respData = JsonUtility.FromJson<OllamaResponse>(request.downloadHandler.text);

                    // JSON ayrıştırma işlemi (4.2 İsteri)
                    LLMDecisionJSON parsedDecision = JsonUtility.FromJson<LLMDecisionJSON>(respData.response);
                    if (parsedDecision != null && !string.IsNullOrEmpty(parsedDecision.command))
                    {
                        cleanText = parsedDecision.responseText;
                        command = parsedDecision.command;
                    }
                }
                catch (System.Exception ex)
                {
                    // Bozuk/Geçersiz JSON üretilirse oyunun çökmemesi için koruma (4.2 & 4.3 İsteri)
                    Debug.LogWarning("[LocalLLMManager] Bozuk JSON formatı alındı, varsayılan duruma geçiliyor: " + ex.Message);
                    cleanText = "Biraz kafam karıştı ama buradayım.";
                    command = "NONE";
                }
            }
            else
            {
                // Ollama sunucusuna ulaşılamazsa anlamlı geri bildirim (4.3 İsteri)
                Debug.LogError("[LocalLLMManager] Ollama Bağlantı Hatası: " + request.error);
                cleanText = "(Yerel LLM sunucusuna ulaşılamadı. Lütfen Ollama'nın açık olduğunu kontrol edin.)";
            }

            // UI ve Ajan tetikleme
            if (chatUI != null) chatUI.DisplayResponse($"Siz: {userPrompt}\nYoldaş: {cleanText}");
            if (bridge != null) bridge.ProcessLLMCommand(command);
        }
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