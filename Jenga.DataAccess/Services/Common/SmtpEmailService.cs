using Jenga.Utility.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jenga.DataAccess.Services.Common;

/// <summary>
/// Simple SMTP e-mail sender. Configure "Email:SmtpHost", "Email:From" in appsettings.
/// Falls back to logging when SMTP host is not configured (dev/test scenarios).
/// </summary>
public sealed class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogService _logService;

    public SmtpEmailService(IConfiguration configuration, ILogService logService)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
        => SendAsync(new[] { to }, subject, htmlBody, cancellationToken);

    public async Task SendAsync(IEnumerable<string> recipients, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var validRecipients = recipients
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .ToList();

        if (validRecipients.Count == 0) return;

        var smtpHost = _configuration["Email:SmtpHost"];
        var from = _configuration["Email:From"] ?? "noreply@jenga.local";

        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            _logService.Log($"[EmailService] SMTP not configured. Would send to: {string.Join(";", validRecipients)} | Subject: {subject}", Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        try
        {
            using var client = new System.Net.Mail.SmtpClient(smtpHost);
            var message = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(from),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            foreach (var r in validRecipients)
                message.To.Add(r);

            await client.SendMailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, "SmtpEmailService.SendAsync");
        }
    }
}
