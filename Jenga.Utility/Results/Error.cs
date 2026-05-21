namespace Jenga.Utility.Results;

/// <summary>
/// Bir işlem hatasını taşıyan değişmez (immutable) tip.
/// Code: makine tarafından okunabilir kısa kod (ör. "Role.NotFound").
/// Message: kullanıcıya/log'a yansıtılacak mesaj (Türkçe).
/// Exception: opsiyonel — alttaki teknik istisna (loglama için).
/// </summary>
public sealed record Error(string Code, string Message, Exception? Exception = null)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    // Yaygın hata sınıfları için kısayollar
    public static Error NotFound(string message, string code = "General.NotFound") => new(code, message);
    public static Error Validation(string message, string code = "General.Validation") => new(code, message);
    public static Error Conflict(string message, string code = "General.Conflict") => new(code, message);
    public static Error Unexpected(string message, Exception? ex = null, string code = "General.Unexpected") => new(code, message, ex);
    public static Error Forbidden(string message, string code = "General.Forbidden") => new(code, message);

    public override string ToString() =>
        string.IsNullOrEmpty(Code) ? Message : $"[{Code}] {Message}";
}
