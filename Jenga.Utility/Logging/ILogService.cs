using Microsoft.Extensions.Logging;

namespace Jenga.Utility.Logging
{
    public interface ILogService
    {
        void Log(string message, LogLevel logLevel);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception? ex = null);

        // Yeni: Global exception handler ve servis katmanları için.
        // 'source' alanı (örn. "RoleService.AddWithRelationsAsync") logda kategori olarak yazılır.
        void LogException(Exception ex, string? source = null, string? message = null);
    }
}
