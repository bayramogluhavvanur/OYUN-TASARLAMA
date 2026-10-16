using UnityEngine;
using TMPro;

public class ChatUIManager : MonoBehaviour
{
    [Header("UI Elemanları")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI chatLogText;

    [Header("Referanslar")]
    [SerializeField] private LocalLLMManager llmManager;

    // Gönder butonuna basıldığında çağrılır
    public void OnSendButtonClicked()
    {
        if (inputField != null && !string.IsNullOrWhiteSpace(inputField.text))
        {
            string userMessage = inputField.text;
            inputField.text = ""; // Mesaj gönderilince kutuyu temizle

            if (llmManager != null)
            {
                llmManager.AskLLM(userMessage);
            }
        }
    }

    // LocalLLMManager'dan gelen cevabı ekrana yazdıran metod
    public void DisplayResponse(string message)
    {
        if (chatLogText != null)
        {
            chatLogText.text = message;
        }
        else
        {
            Debug.LogWarning("[ChatUIManager] Chat Log Text slotu boş!");
        }
    }
}