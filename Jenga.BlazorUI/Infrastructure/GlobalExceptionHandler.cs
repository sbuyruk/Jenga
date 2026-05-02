using Jenga.Utility.Logging;
using Microsoft.AspNetCore.Diagnostics;

namespace Jenga.BlazorUI.Infrastructure;

/// <summary>
/// HTTP pipeline'ında yakalanmamış istisnaları merkezi olarak loglar.
/// app.UseExceptionHandler() ile birlikte çalışır; gerçek kullanıcı yönlendirmesini
/// pipeline yapar (Program.cs içindeki "/Error" handler).
/// </summary>
/// <remarks>
/// IExceptionHandler ExceptionHandlerMiddleware tarafından <b>root provider</b>
/// üzerinden resolve edildiği için bu sınıf Singleton olmak zorundadır.
/// ILogService de Singleton kayıtlı olduğundan doğrudan inject edilebilir.
/// </remarks>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogService _logService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogService logService, ILogger<GlobalExceptionHandler> logger)
    {
        _logService = logService;
        _logger = logger;
    }

    public ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;
        var path = httpContext.Request?.Path.Value ?? "(unknown)";
        var user = httpContext.User?.Identity?.Name ?? "(anonymous)";

        var message = $"Unhandled exception | TraceId={traceId} | Path={path} | User={user}";

        _logger.LogError(exception, "{Message}", message);

        try
        {
            _logService.LogException(exception, source: "GlobalExceptionHandler", message: message);
        }
        catch
        {
            // Loglama ana akışı bloklamasın.
        }

        // false döndürerek default exception handler middleware'ine devrederiz
        // (Program.cs -> app.UseExceptionHandler("/Error", ...)).
        return ValueTask.FromResult(false);
    }
}
