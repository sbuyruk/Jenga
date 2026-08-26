namespace Jenga.DataAccess.Services.Common;

/// <summary>
/// Abstraction for sending e-mail notifications.
/// </summary>
public interface IEmailService
{
    /// <summary>Sends a plain-text/HTML e-mail.</summary>
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);

    /// <summary>Sends an e-mail to multiple recipients.</summary>
    Task SendAsync(IEnumerable<string> recipients, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
