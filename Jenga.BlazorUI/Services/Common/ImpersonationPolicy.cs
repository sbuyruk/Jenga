using System.Globalization;

namespace Jenga.BlazorUI.Services.Common;

/// <summary>Canlı test erişimi açıkça etkinleştirilmeli ve son kullanma tarihi taşımalıdır.</summary>
public sealed class ImpersonationPolicy(IConfiguration configuration, IHostEnvironment environment)
{
    public bool IsEnabled => IsEnabledAt(DateTimeOffset.UtcNow);

    public bool IsEnabledAt(DateTimeOffset now) => GetDisabledReasonAt(now) is null;

    public string? GetDisabledReasonAt(DateTimeOffset now)
    {
        if (environment.IsDevelopment()) return null;

        if (!bool.TryParse(configuration["Auth:Impersonation:Enabled"], out var enabled) || !enabled)
            return "Canlı ortamda kullanıcı değiştirme erişimi kapalı.";

        if (!DateTimeOffset.TryParseExact(configuration["Auth:Impersonation:ExpiresAtUtc"],
                "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var expires))
            return "Canlı test erişiminin bitiş tarihi eksik veya geçersiz. Sunucuda geçerli bir UTC bitiş tarihi tanımlanmalıdır.";

        return expires > now ? null : "Canlı ortamda kullanıcı değiştirme için tanımlanan test süresi dolmuş.";
    }
}
