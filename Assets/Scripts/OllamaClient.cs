using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class OllamaSettings
{
    public string baseUrl = "http://127.0.0.1:11434";
    public string modelName = "qwen2.5:7b";
    [Range(5, 180)] public int timeoutSeconds = 90;
    [Range(0f, 1.5f)] public float temperature = 0.4f;
    public int maxTokens = 160;
    public string keepAlive = "10m";
}

[Serializable] internal class OllamaMessageDto { public string role; public string content; }
[Serializable] internal class OllamaOptionsDto { public float temperature; public int num_predict; }
[Serializable]
internal class OllamaChatRequestDto
{
    public string model;
    public OllamaMessageDto[] messages;
    public bool stream;
    public string format;
    public string keep_alive;
    public OllamaOptionsDto options;
}
[Serializable] internal class OllamaChatResponseDto { public OllamaMessageDto message; public string error; }
[Serializable] internal class OllamaTagItemDto { public string name; public string model; }
[Serializable] internal class OllamaTagsDto { public OllamaTagItemDto[] models; }

public sealed class OllamaClient : ILlmClient
{
    readonly OllamaSettings s;
    public OllamaClient(OllamaSettings settings) { s = settings; }

    string Url(string path) { return s.baseUrl.TrimEnd('/') + path; }

    public IEnumerator CheckHealth(Action<LlmResult> onDone)
    {
        using (var req = UnityWebRequest.Get(Url("/api/tags")))
        {
            req.timeout = 5;
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                if (onDone != null) onDone(MapFailure(req));
                yield break;
            }

            bool listed = ModelListed(req.downloadHandler.text, s.modelName);
            if (onDone != null)
                onDone(listed ? LlmResult.Ok("") : LlmResult.Fail(LlmStatus.ModelNotFound, s.modelName));
        }
    }

    public IEnumerator Generate(LlmRequest request, Action<LlmResult> onDone)
    {
        string body;
        try { body = JsonUtility.ToJson(BuildPayload(request)); }
        catch (Exception e)
        {
            if (onDone != null) onDone(LlmResult.Fail(LlmStatus.HttpError, e.Message));
            yield break;
        }

        using (var req = new UnityWebRequest(Url("/api/chat"), "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = s.timeoutSeconds;

            yield return req.SendWebRequest();   // asenkron: frame'i bloklamaz

            if (req.result != UnityWebRequest.Result.Success)
            {
                if (onDone != null) onDone(MapFailure(req));
                yield break;
            }

            string text = null;
            try
            {
                var resp = JsonUtility.FromJson<OllamaChatResponseDto>(req.downloadHandler.text);
                if (resp != null && resp.message != null) text = resp.message.content;
            }
            catch (Exception e)
            {
                if (onDone != null) onDone(LlmResult.Fail(LlmStatus.HttpError, "Yanıt çözümlenemedi: " + e.Message));
                yield break;
            }

            if (onDone != null)
                onDone(string.IsNullOrWhiteSpace(text)
                    ? LlmResult.Fail(LlmStatus.EmptyResponse, "Model boş yanıt döndürdü.")
                    : LlmResult.Ok(text));
        }
    }

    OllamaChatRequestDto BuildPayload(LlmRequest r)
    {
        var msgs = new OllamaMessageDto[r.Messages.Count];
        for (int i = 0; i < msgs.Length; i++)
            msgs[i] = new OllamaMessageDto { role = r.Messages[i].Role, content = r.Messages[i].Content };

        return new OllamaChatRequestDto
        {
            model = s.modelName,
            messages = msgs,
            stream = false,
            format = "json",
            keep_alive = s.keepAlive,
            options = new OllamaOptionsDto { temperature = s.temperature, num_predict = s.maxTokens }
        };
    }

    static LlmResult MapFailure(UnityWebRequest r)
    {
        string err = r.error ?? "";
        if (r.result == UnityWebRequest.Result.ProtocolError)
        {
            if (r.responseCode == 404) return LlmResult.Fail(LlmStatus.ModelNotFound, err);
            return LlmResult.Fail(LlmStatus.HttpError, "HTTP " + r.responseCode);
        }
        if (err.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0 ||
            err.IndexOf("timed out", StringComparison.OrdinalIgnoreCase) >= 0)
            return LlmResult.Fail(LlmStatus.Timeout, err);
        return LlmResult.Fail(LlmStatus.ServerUnreachable, err);
    }

    static bool ModelListed(string json, string wanted)
    {
        try
        {
            var tags = JsonUtility.FromJson<OllamaTagsDto>(json);
            if (tags == null || tags.models == null) return false;
            bool hasTag = wanted.Contains(":");
            foreach (var m in tags.models)
            {
                if (m == null || m.name == null) continue;
                if (m.name == wanted) return true;
                if (!hasTag && m.name.StartsWith(wanted + ":")) return true;
            }
        }
        catch (Exception) { }
        return false;
    }
}