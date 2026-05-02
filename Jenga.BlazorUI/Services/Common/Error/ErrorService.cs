using Jenga.BlazorUI.Services.Common.Toast;
using Jenga.Utility.Logging;
using Microsoft.Extensions.Logging;

namespace Jenga.BlazorUI.Services.Common.Error;

public class ErrorService : IErrorService
{
    public event Action<ErrorModel>? OnError;

    private readonly IToastService _toastService;
    private readonly ILogService _logger;

    public ErrorService(IToastService toastService, ILogService logger)
    {
        _toastService = toastService;
        _logger = logger;
    }

    public void RaiseError(string message, ErrorSeverity severity = ErrorSeverity.Error, string? detail = null, int duration = 5000)
    {
        OnError?.Invoke(new ErrorModel
        {
            Id = Guid.NewGuid(),
            Message = message,
            Detail = detail,
            Severity = severity,
            Duration = duration
        });
        _toastService.ShowToast(message, MapToastLevel(severity));
        _logger.Log(message, MapLogLevel(severity));
    }

    private static ToastType MapToastLevel(ErrorSeverity severity) => severity switch
    {
        ErrorSeverity.Error => ToastType.Error,
        ErrorSeverity.Warning => ToastType.Warning,
        ErrorSeverity.Info => ToastType.Info,
        _ => ToastType.Info
    };

    private static LogLevel MapLogLevel(ErrorSeverity severity) => severity switch
    {
        ErrorSeverity.Error => LogLevel.Error,
        ErrorSeverity.Warning => LogLevel.Warning,
        ErrorSeverity.Info => LogLevel.Information,
        _ => LogLevel.Information
    };
}
