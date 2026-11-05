using System;
using Unity.InferenceEngine.Tokenization.Normalizers;
using UnityEngine;

public enum NpcAction { None, OpenDoor }
public enum NpcMood { Calm, Pleased, Suspicious, Angry }

public sealed class NpcReply
{
    public string Text;
    public NpcAction Action;
    public NpcMood Mood;

    /// <summary>Konuşma geçmişine, modelden beklenen şemayla aynı biçimde yazılır (küçük modeller formatı korur).</summary>
    public string ToJson()
    {
        return JsonUtility.ToJson(new NpcReplyDto
        {
            cevap = Text,
            eylem = Action == NpcAction.OpenDoor ? "kapiyi_ac" : "yok",
            duygu = Mood == NpcMood.Pleased ? "memnun" :
                    Mood == NpcMood.Suspicious ? "supheci" :
                    Mood == NpcMood.Angry ? "kizgin" : "sakin"
        });
    }
}

[Serializable]
internal class NpcReplyDto
{
    public string cevap;
    public string eylem;
    public string duygu;
}

public interface INpcReplyParser
{
    bool TryParse(string raw, out NpcReply reply, out string error);
}

/// <summary>Modelin JSON çıktısını güvenli ayrıştırır. Bozuk/eksik/bilinmeyen alanlarda oyun durumu DEĞİŞMEZ.</summary>
public sealed class JsonNpcReplyParser : INpcReplyParser
{
    public bool TryParse(string raw, out NpcReply reply, out string error)
    {
        reply = null; error = null;
        if (string.IsNullOrWhiteSpace(raw)) { error = "Boş yanıt"; return false; }

        // ```json ... ``` veya ek metin gelirse ilk '{' ile son '}' arasını al
        int a = raw.IndexOf('{'), b = raw.LastIndexOf('}');
        if (a < 0 || b <= a) { error = "JSON nesnesi bulunamadı"; return false; }

        NpcReplyDto dto;
        try { dto = JsonUtility.FromJson<NpcReplyDto>(raw.Substring(a, b - a + 1)); }
        catch (Exception e) { error = e.Message; return false; }

        if (dto == null || string.IsNullOrWhiteSpace(dto.cevap)) { error = "'cevap' alanı eksik"; return false; }

        reply = new NpcReply
        {
            Text = dto.cevap.Trim(),
            Action = ParseAction(dto.eylem),
            Mood = ParseMood(dto.duygu)
        };
        return true;
    }

    static NpcAction ParseAction(string v)
    {
        return TextNormalizer.Fold(v, true) == "kapiyi_ac" ? NpcAction.OpenDoor : NpcAction.None;
    }

    static NpcMood ParseMood(string v)
    {
        switch (TextNormalizer.Fold(v, true))
        {
            case "memnun": return NpcMood.Pleased;
            case "supheci": return NpcMood.Suspicious;
            case "kizgin": return NpcMood.Angry;
            default: return NpcMood.Calm;
        }
    }
}