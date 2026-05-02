using Microsoft.Extensions.Logging;

namespace Jenga.Utility.Logging
{
    /// <summary>
    /// Null Object pattern: log altyapısı yapılandırılmamışsa hiçbir şey yapmaz.
    /// DI kaydı <c>TryAddSingleton</c> ile yapıldığından gerçek <see cref="LogService"/>
    /// kayıtlıysa bu sınıf devreye girmez.
    /// </summary>
    public sealed class NullLogService : ILogService
    {
        public static readonly NullLogService Instance = new();

        public void Log(string message, LogLevel logLevel) { }
        public void LogInfo(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message, Exception? ex = null) { }
        public void LogException(Exception ex, string? source = null, string? message = null) { }
    }
}
