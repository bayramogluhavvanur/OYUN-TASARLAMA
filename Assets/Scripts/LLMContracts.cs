using System;
using System.Collections;
using System.Collections.Generic;

public enum LlmStatus { Success, ServerUnreachable, Timeout, ModelNotFound, HttpError, EmptyResponse }

public sealed class ChatMessage
{
    public const string SystemRole = "system";
    public const string UserRole = "user";
    public const string AssistantRole = "assistant";

    public readonly string Role;
    public readonly string Content;
    public ChatMessage(string role, string content) { Role = role; Content = content; }
}

public sealed class LlmRequest
{
    public readonly List<ChatMessage> Messages = new List<ChatMessage>();
}

public sealed class LlmResult
{
    public LlmStatus Status { get; private set; }
    public string Text { get; private set; }
    public string Detail { get; private set; }
    public bool IsSuccess { get { return Status == LlmStatus.Success; } }

    public static LlmResult Ok(string text) { return new LlmResult { Status = LlmStatus.Success, Text = text }; }
    public static LlmResult Fail(LlmStatus status, string detail) { return new LlmResult { Status = status, Detail = detail }; }
}

/// <summary>LLM sağlayıcısından bağımsız soyutlama (DIP). Ollama yerine başka yerel sunucu takılabilir.</summary>
public interface ILlmClient
{
    IEnumerator CheckHealth(Action<LlmResult> onDone);
    IEnumerator Generate(LlmRequest request, Action<LlmResult> onDone);
}