using System.Globalization;

namespace Jenga.BlazorUI.Services.Common;

/// <summary>Canlı test erişimi açıkça etkinleştirilmeli ve son kullanma tarihi taşımalıdır.</summary>
public sealed class ImpersonationPolicy(IConfiguration configuration, IHostEnvironment environment)
{
    public bool IsEnabled => IsEnabledAt(DateTimeOffset.UtcNow);

    public bool IsEnabledAt(DateTimeOffset now)
    {
        if (environment.IsDevelopment()) return true;

        return configuration.GetValue<bool>("Auth:Impersonation:Enabled")
            && DateTimeOffset.TryParseExact(configuration["Auth:Impersonation:ExpiresAtUtc"],
                "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var expires)
            && expires > now;
    }
}
