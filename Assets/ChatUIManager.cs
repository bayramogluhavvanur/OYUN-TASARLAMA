using UnityEngine;
using TMPro;

public class ChatUIManager : MonoBehaviour
{
    [Header("UI Elemanları")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI chatLogText;

    [Header("Referanslar")]
    [SerializeField] private LocalLLMManager llmManager;

    public void OnSendButtonClicked()
    {
        if (inputField != null && !string.IsNullOrWhiteSpace(inputField.text))
        {
            string userMessage = inputField.text;
            inputField.text = ""; // Mesaj kutusunu temizle
            inputField.ActivateInputField(); // Yazı yazma odağını kutuda tut

            if (llmManager != null)
            {
                llmManager.AskLLM(userMessage);
            }
        }
    }

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