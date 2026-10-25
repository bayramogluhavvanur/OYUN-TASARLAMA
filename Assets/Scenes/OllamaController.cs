using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using TMPro;

[System.Serializable]
public class OllamaMessage { public string role; public string content; }

[System.Serializable]
public class OllamaChatRequest { public string model; public OllamaMessage[] messages; public bool stream; }

[System.Serializable]
public class OllamaChatResponse { public string model; public OllamaMessage message; public bool done; }

[System.Serializable]
public class AjanKarari { public string cevap; public bool kapi_acilsin_mi; }

public class OllamaController : MonoBehaviour
{
    private string ollamaUrl = "http://localhost:11434/api/chat";
    private string modelName = "qwen2.5:7b";

    [Header("UI Bağlantıları")]
    public TMP_InputField oyuncuGirdiAlani;
    public TMP_Text ajanCevapEkrani;

    [Header("Oyun Objeleri")]
    public GameObject labirentKapisi;

    public void MesajiGonder()
    {
        if (string.IsNullOrWhiteSpace(oyuncuGirdiAlani.text)) return;
        string mesaj = oyuncuGirdiAlani.text;
        ajanCevapEkrani.text = "Sistemleri taranıyor...";
        StartCoroutine(PostRequest(mesaj));
        oyuncuGirdiAlani.text = "";
    }

    private IEnumerator PostRequest(string userText)
    {
        // 1. ŞİFRE KONTROLÜNÜ C# YAPIYOR
        string kucukHarfliMesaj = userText.ToLower(new System.Globalization.CultureInfo("tr-TR"));
        bool sifreDogruMu = kucukHarfliMesaj.Contains("ikarus") || kucukHarfliMesaj.Contains("ıkarus");

        // 2. YAPAY ZEKAYA GİDEN EMİR DİNAMİK OLARAK DEĞİŞİYOR
        string aktifPrompt = sifreDogruMu
            ? "Sen labirentin muhafızısın. Oyuncu doğru şifreyi bildi. Onu hafif alaycı bir şekilde tebrik et ve kapıyı açtığını söyle. YALNIZCA şu JSON formatını üret:\n{\"cevap\": \"tebrik mesajın\", \"kapi_acilsin_mi\": true}"
            : "Sen labirentin alaycı muhafızısın. Oyuncu şifreyi henüz bilmiyor. Ona güneşe uçup balmumu kanatları eriyen adamla ilgili gizemli bir ipucu ver. KURAL: Mesajında şifreyi veya herhangi bir özel ismi ASLA kullanma. YALNIZCA şu JSON formatını üret:\n{\"cevap\": \"ipucu mesajın\", \"kapi_acilsin_mi\": false}";

        OllamaMessage sysMsg = new OllamaMessage { role = "system", content = aktifPrompt };
        OllamaMessage usrMsg = new OllamaMessage { role = "user", content = userText };

        OllamaChatRequest requestData = new OllamaChatRequest
        {
            model = modelName,
            messages = new OllamaMessage[] { sysMsg, usrMsg },
            stream = false
        };

        using (UnityWebRequest request = new UnityWebRequest(ollamaUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(requestData));
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                ajanCevapEkrani.text = "*Bağlantı Hatası*";
            }
            else
            {
                OllamaChatResponse response = JsonUtility.FromJson<OllamaChatResponse>(request.downloadHandler.text);
                if (response != null && response.message != null)
                {
                    string rawContent = response.message.content.Replace("```json", "").Replace("```", "").Trim();
                    int firstBrace = rawContent.IndexOf('{');
                    int lastBrace = rawContent.LastIndexOf('}');

                    if (firstBrace != -1 && lastBrace != -1 && lastBrace > firstBrace)
                    {
                        string cleanJson = rawContent.Substring(firstBrace, lastBrace - firstBrace + 1);
                        try
                        {
                            AjanKarari karar = JsonUtility.FromJson<AjanKarari>(cleanJson);
                            ajanCevapEkrani.text = karar.cevap;

                            if (karar.kapi_acilsin_mi && labirentKapisi != null)
                            {
                                labirentKapisi.SetActive(false);
                            }
                        }
                        catch
                        {
                            ajanCevapEkrani.text = rawContent;
                        }
                    }
                    else
                    {
                        ajanCevapEkrani.text = rawContent;
                    }
                }
            }
        }
    }
}