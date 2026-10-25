using System.Text;

public static class TextNormalizer
{
    /// <summary>Küçük harfe çevirir, Türkçe karakterleri sadeleştirir, yalnızca harf/rakam (ve istenirse '_') bırakır.</summary>
    public static string Fold(string s, bool keepUnderscore = false)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var sb = new StringBuilder(s.Length);
        foreach (char ch in s)
        {
            char c = ch;
            switch (c)
            {
                case 'İ': case 'I': case 'ı': c = 'i'; break;
                case 'Ş': case 'ş': c = 's'; break;
                case 'Ğ': case 'ğ': c = 'g'; break;
                case 'Ü': case 'ü': c = 'u'; break;
                case 'Ö': case 'ö': c = 'o'; break;
                case 'Ç': case 'ç': c = 'c'; break;
            }
            c = char.ToLowerInvariant(c);
            if (char.IsLetterOrDigit(c) || (keepUnderscore && c == '_')) sb.Append(c);
        }
        return sb.ToString();
    }
}

public interface ISecretVerifier
{
    bool ContainsSecret(string text);
}

/// <summary>Parolayı yalnızca C# tarafında tutar; büyük/küçük harf, Türkçe karakter ve harf aralarındaki boşlukları yok sayar.</summary>
public sealed class KeywordSecretVerifier : ISecretVerifier
{
    readonly string secret;
    public KeywordSecretVerifier(string secretWord) { secret = TextNormalizer.Fold(secretWord); }

    public bool ContainsSecret(string text)
    {
        return secret.Length > 0 && TextNormalizer.Fold(text).Contains(secret);
    }
}