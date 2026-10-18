using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class OllamaManager : MonoBehaviour
{
    [Header("Arayüz Bağlantıları")]
    // diyalogPaneli değişkeni karmaşayı önlemek için kaldırıldı.
    public TMP_InputField kullaniciGirdisi;
    public Button gonderButonu;
    public TextMeshProUGUI bekciMetni;
    public CanvasGroup panelCanvasGroup;

    [Header("Oyun İçi Etkileşim")]
    public GameObject engelKapi;

    [Header("Model Ayarları")]
    private string ollamaUrl = "http://127.0.0.1:11434/api/generate";
    private string modelName = "qwen2.5:7b";

    private string sistemTalimati =
        "Sen fantastik bir RPG oyununda Kadim Bekçi adlı bir NPCsın. Karşındaki gezgin seni ikna edip geçmek istiyor. " +
        "Eğer mantıklı, destansı veya saygılı bir sebep sunarsa kapıyı açmayı kabul et. Saygısızsa reddet. " +
        "SADECE aşağıdaki JSON formatında cevap ver, başka hiçbir açıklama veya markdown (```json vb.) kullanma: " +
        "{\"cevap\": \"Buraya gezgine söyleyeceğin 2 kısa cümlelik sözler gelecek.\", \"kapiAcilsinMi\": true_veya_false}";

    private MonoBehaviour oyuncuHareket; // KahramanHareket scriptini genel bir MonoBehaviour olarak tuttuk ki hata vermesin

    void Start()
    {
        // Oyuncu hareket scriptini bul (ismini kendi scriptine göre uyarlayabilirsin)
        oyuncuHareket = FindAnyObjectByType<YoldasZeka>();

        gonderButonu.onClick.AddListener(MesajGonder);
        kullaniciGirdisi.onSubmit.AddListener(delegate { MesajGonder(); });
    }

    public void PaneliAc()
    {
        if (panelCanvasGroup != null)
        {
            StartCoroutine(FadeInPanel(panelCanvasGroup, 0.5f));
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        kullaniciGirdisi.ActivateInputField();

        if (oyuncuHareket != null) oyuncuHareket.enabled = false;
    }

    public void PaneliKapat()
    {
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.gameObject.SetActive(false);
            panelCanvasGroup.alpha = 0f;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (oyuncuHareket != null) oyuncuHareket.enabled = true;
    }

    void MesajGonder()
    {
        if (string.IsNullOrWhiteSpace(kullaniciGirdisi.text)) return;

        string mesaj = kullaniciGirdisi.text;
        kullaniciGirdisi.text = "";
        bekciMetni.text = "Bekçi zihnini topluyor...";

        StartCoroutine(OllamaIstekGonder(mesaj));
    }

    IEnumerator OllamaIstekGonder(string prompt)
    {
        OllamaIstekPaketi istekPaketi = new OllamaIstekPaketi
        {
            model = modelName,
            prompt = prompt,
            system = sistemTalimati,
            stream = false,
            format = "json"
        };

        string jsonVerisi = JsonUtility.ToJson(istekPaketi);

        using (UnityWebRequest istek = new UnityWebRequest(ollamaUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonVerisi);
            istek.uploadHandler = new UploadHandlerRaw(bodyRaw);
            istek.downloadHandler = new DownloadHandlerBuffer();
            istek.SetRequestHeader("Content-Type", "application/json");
            istek.timeout = 60;

            yield return istek.SendWebRequest();

            if (istek.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    OllamaYanit yanit = JsonUtility.FromJson<OllamaYanit>(istek.downloadHandler.text);
                    BekciCevabi bekciVerisi = JsonUtility.FromJson<BekciCevabi>(yanit.response);

                    bekciMetni.text = bekciVerisi.cevap.Trim();

                    if (bekciVerisi.kapiAcilsinMi)
                    {
                        if (engelKapi != null)
                        {
                            engelKapi.SetActive(false);
                            Debug.Log("Başarılı: Bekçi ikna oldu, kapı açıldı!");
                        }
                    }
                }
                catch (System.Exception e)
                {
                    bekciMetni.text = "Bekçi'nin zihni bulandı... (JSON formatı bozuk)";
                    Debug.LogError("JSON Çözümleme Hatası: " + e.Message);
                }
            }
            else
            {
                bekciMetni.text = "Zihinsel bağ koptu (Zaman aşımı veya bağlantı hatası).";
                Debug.LogError("Ollama Hatası: " + istek.error);
            }
        }
    }

    public IEnumerator FadeInPanel(CanvasGroup panelGroup, float sure)
    {
        panelGroup.alpha = 0f;
        panelGroup.gameObject.SetActive(true);

        while (panelGroup.alpha < 1f)
        {
            panelGroup.alpha += Time.deltaTime / sure;
            yield return null;
        }

        panelGroup.alpha = 1f;
    }
}

// İstek göndermek için yapılandırılmış güvenli JSON sınıfı
[System.Serializable]
public class OllamaIstekPaketi
{
    public string model;
    public string prompt;
    public string system;
    public bool stream;
    public string format;
}

[System.Serializable]
public class OllamaYanit
{
    public string response;
}

[System.Serializable]
public class BekciCevabi
{
    public string cevap;
    public bool kapiAcilsinMi;
}