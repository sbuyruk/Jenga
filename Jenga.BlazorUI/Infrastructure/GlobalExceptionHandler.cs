using Jenga.Utility.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Jenga.BlazorUI.Infrastructure;

/// <summary>
/// HTTP pipeline'ında yakalanmamış istisnaları merkezi olarak loglar.
/// app.UseExceptionHandler() ile birlikte çalışır; gerçek kullanıcı yönlendirmesini
/// pipeline yapar (Program.cs içindeki "/Error" handler).
/// </summary>
/// <remarks>
/// IExceptionHandler ExceptionHandlerMiddleware tarafından <b>root provider</b>
/// üzerinden resolve edildiği için bu sınıf Singleton olmak zorundadır.
/// Scoped servislere (ILogService gibi) erişmek için IServiceScopeFactory kullanılır.
/// </remarks>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IServiceScopeFactory scopeFactory, ILogger<GlobalExceptionHandler> logger)
    {
        _scopeFactory = scopeFactory;
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

        // ILogger her zaman güvenli (singleton).
        _logger.LogError(exception, "{Message}", message);

        // ILogService scoped; ayrı bir scope üzerinden çöz.
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var logService = scope.ServiceProvider.GetService<ILogService>();
            logService?.LogException(exception, source: "GlobalExceptionHandler", message: message);
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
