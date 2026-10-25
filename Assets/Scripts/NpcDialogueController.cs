using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcDialogueController : MonoBehaviour
{
    [Header("Sahne bağlantıları")]
    [SerializeField] MonoBehaviour viewBehaviour;           // IDialogueView (DialogueView)
    [SerializeField] MonoBehaviour actionHandlerBehaviour;  // INpcActionHandler (DoorUnlockHandler)
    [Tooltip("Konuşma sırasında kapatılacak oyuncu hareket/kamera scriptleri")]
    [SerializeField] Behaviour[] disabledWhileTalking;

    [Header("Ollama (yerel)")]
    [SerializeField] OllamaSettings ollama = new OllamaSettings();

    [Header("NPC")]
    [SerializeField] NpcPersona persona = new NpcPersona();
    [Tooltip("Parola SADECE burada tutulur, modele ASLA gönderilmez.")]
    [SerializeField] string secretWord = "ikarus";
    [SerializeField, Range(0, 10)] int maxHistoryTurns = 6;
    [SerializeField, Range(0, 3)] int maxRetries = 1;

    IDialogueView view;
    INpcActionHandler handler;
    ILlmClient llm;
    INpcReplyParser parser;
    ISecretVerifier secret;

    readonly List<ChatMessage> history = new List<ChatMessage>();
    bool[] prevEnabled;
    bool busy, secretSpoken, completed;
    int session;

    public bool IsOpen { get { return view != null && view.IsOpen; } }
   
    void Awake()
    {
        view = viewBehaviour as IDialogueView;
        handler = actionHandlerBehaviour as INpcActionHandler;
        if (view == null || handler == null)
        {
            Debug.LogError("NpcDialogueController: view/handler IDialogueView/INpcActionHandler uygulamıyor veya atanmamış.");
            enabled = false;
            return;
        }
        // Composition root: bağımlılıklar arayüzler üzerinden tutulur
        llm = new OllamaClient(ollama);
        parser = new JsonNpcReplyParser();
        secret = new KeywordSecretVerifier(secretWord);

        view.MessageSubmitted += OnMessageSubmitted;
        view.CloseRequested += EndConversation;
    }

    void OnDestroy()
    {
        if (view == null) return;
        view.MessageSubmitted -= OnMessageSubmitted;
        view.CloseRequested -= EndConversation;
    }

    /// <summary>Test için bağımlılık enjeksiyonu (ör. sahte LLM istemcisi).</summary>
    public void Inject(ILlmClient client, INpcReplyParser replyParser, ISecretVerifier verifier)
    {
        llm = client; parser = replyParser; secret = verifier;
    }

    public void BeginConversation()
    {
        Debug.Log("BeginConversation çağrıldı: enabled=" + enabled + " completed=" + completed + " açık=" + (view != null && view.IsOpen));
        if (!enabled || completed || view.IsOpen) return;
        view.Open();
        SetPlayerLocked(true);
        StartCoroutine(HealthCheck(session));
    }

    public void EndConversation()
    {
        if (!enabled || completed) return;
        session++;
        busy = false;
        view.ShowThinking(false);
        view.Close();
        SetPlayerLocked(false);
    }

    IEnumerator HealthCheck(int mySession)
    {
        LlmResult r = null;
        yield return llm.CheckHealth(x => r = x);
        if (mySession != session || r == null) yield break;
        view.ShowStatus(r.IsSuccess ? "Bekçi seni dinliyor." : Describe(r, null));
    }

    void OnMessageSubmitted(string playerText)
    {
        if (busy || completed) return;
        StartCoroutine(Converse(playerText));
    }

    IEnumerator Converse(string playerText)
    {
        int mySession = session;
        busy = true;
        view.SetInputEnabled(false);
        view.ShowStatus("");
        view.ShowThinking(true);

        if (!secretSpoken && secret.ContainsSecret(playerText)) secretSpoken = true;

        LlmRequest request = BuildRequest(playerText);
        LlmResult result = null;
        NpcReply reply = null;
        string parseError = null;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            result = null;
            yield return llm.Generate(request, r => result = r);
            if (mySession != session) yield break;   // konuşma kapatıldı, sonuç atılır

            if (result == null) result = LlmResult.Fail(LlmStatus.HttpError, "Yanıt alınamadı.");

            if (!result.IsSuccess)
            {
                if (result.Status == LlmStatus.EmptyResponse && attempt < maxRetries) continue;
                break;   // sunucu yok / timeout / model yok: tekrar denemenin anlamı yok
            }

            if (parser.TryParse(result.Text, out reply, out parseError)) break;

            reply = null;
            Debug.LogWarning("NPC yanıtı geçersiz (deneme " + (attempt + 1) + "): " + parseError + " | Ham: " + result.Text);
        }

        view.ShowThinking(false);

        if (reply == null) view.ShowStatus(Describe(result, parseError));
        else ApplyReply(playerText, reply);

        busy = false;
        if (!completed) view.SetInputEnabled(true);
    }

    LlmRequest BuildRequest(string playerText)
    {
        var req = new LlmRequest();
        req.Messages.Add(new ChatMessage(ChatMessage.SystemRole, NpcPromptBuilder.BuildSystemPrompt(persona)));
        req.Messages.AddRange(history);
        req.Messages.Add(new ChatMessage(ChatMessage.UserRole, NpcPromptBuilder.WrapPlayerMessage(playerText, secretSpoken)));
        return req;
    }

    void ApplyReply(string playerText, NpcReply reply)
    {
        string text = reply.Text;
        NpcAction action = reply.Action;

        // Savunma 1: model parolayı sızdırdıysa göstermeden engelle
        if (!secretSpoken && secret.ContainsSecret(text))
        {
            Debug.LogWarning("Model parolayı sızdırmaya çalıştı, yanıt engellendi.");
            text = "Bekçi homurdanıp başka yöne baktı...";
            action = NpcAction.None;
        }

        // Savunma 2: parola doğrulanmadan kapı asla açılmaz (prompt injection'a karşı)
        if (action == NpcAction.OpenDoor && !secretSpoken)
        {
            Debug.LogWarning("Model kapıyı açmak istedi ama parola doğrulanmadı, eylem iptal edildi.");
            action = NpcAction.None;
        }

        var finalReply = new NpcReply { Text = text, Action = action, Mood = reply.Mood };
        Remember(playerText, finalReply);
        view.ShowNpcText(text, reply.Mood);

        if (action == NpcAction.OpenDoor)
        {
            completed = true;
            view.SetInputEnabled(false);
            handler.Execute(action);
        }
    }

    void Remember(string playerText, NpcReply reply)
    {
        history.Add(new ChatMessage(ChatMessage.UserRole, playerText));
        history.Add(new ChatMessage(ChatMessage.AssistantRole, reply.ToJson()));
        int max = maxHistoryTurns * 2;
        while (history.Count > max) history.RemoveAt(0);
    }

    string Describe(LlmResult r, string parseError)
    {
        if (r == null) return "Bekçiyle bağlantı kurulamadı.";
        switch (r.Status)
        {
            case LlmStatus.ServerUnreachable:
                return "Ollama sunucusuna ulaşılamıyor. Ollama'nın çalıştığından emin ol (ollama serve).";
            case LlmStatus.Timeout:
                return "Model zamanında yanıt vermedi. Biraz bekleyip tekrar dene (ilk istek model yüklenirken uzun sürebilir).";
            case LlmStatus.ModelNotFound:
                return "'" + ollama.modelName + "' modeli bulunamadı. Terminalde: ollama pull " + ollama.modelName;
            case LlmStatus.EmptyResponse:
                return "Bekçi sessiz kaldı (boş yanıt). Tekrar dene.";
            case LlmStatus.HttpError:
                return "Ollama hata döndürdü: " + r.Detail;
            default:
                return "Bekçinin sözleri anlaşılamadı (geçersiz yanıt biçimi). Tekrar dene.";
        }
    }

    void SetPlayerLocked(bool locked)
    {
        if (disabledWhileTalking == null) return;
        if (locked)
        {
            prevEnabled = new bool[disabledWhileTalking.Length];
            for (int i = 0; i < disabledWhileTalking.Length; i++)
            {
                var b = disabledWhileTalking[i];
                if (b == null) continue;
                prevEnabled[i] = b.enabled;
                b.enabled = false;
            }
        }
        else if (prevEnabled != null)
        {
            for (int i = 0; i < disabledWhileTalking.Length && i < prevEnabled.Length; i++)
                if (disabledWhileTalking[i] != null) disabledWhileTalking[i].enabled = prevEnabled[i];
        }
    }
}